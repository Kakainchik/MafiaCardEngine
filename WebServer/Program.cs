using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebServer.Hubs;
using WebServer.Model;
using WebServer.Model.Game;
using WebServer.Model.ParanoiaCorp;
using WebServer.Model.Room;
using WebServer.Model.User;
using WebServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure JWT
IConfigurationSection jwtSection = builder.Configuration.GetSection("Jwt");
JwtSettings jwtSettings = jwtSection.Get<JwtSettings>() ?? new JwtSettings
{
    Issuer = "MafiaCardOSDebug",
    Audience = "MafiaCardClientDebug",
    RefreshTokenTTL = 7
};

string jwtSecret = builder.Configuration["UserAuth:JwtSecret"]
    ?? builder.Configuration["Jwt:Secret"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? "ParanoiaCorpSecureMidCenturyNoirJwtSecretKey2026!#";

jwtSettings.Secret = jwtSecret;
builder.Services.Configure<JwtSettings>(jwt =>
{
    jwt.Secret = jwtSecret;
    jwt.Issuer = jwtSettings.Issuer;
    jwt.Audience = jwtSettings.Audience;
    jwt.RefreshTokenTTL = jwtSettings.RefreshTokenTTL;
});

// Logging
builder.Services.AddLogging(log =>
{
    log.AddDebug();
    log.AddConsole();
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        SymmetricSecurityKey ssk = new(Encoding.ASCII.GetBytes(jwtSecret));
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = ssk,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = delegate (MessageReceivedContext context)
            {
                StringValues accessToken = context.Request.Query["accessToken"];
                PathString path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

// Exception handler
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = async delegate (HttpContext context)
    {
        switch (context.Features.Get<IExceptionHandlerPathFeature>()?.Error)
        {
            case BadHttpRequestException ex:
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { Error = ex.Message });
                break;
            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }
    };
});

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DB Context with dynamic provider selection: SQLite, PostgreSQL, or SQL Server
string? defaultConn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration["UserAuth:DefaultConnection"];

if (string.IsNullOrEmpty(defaultConn) || defaultConn.Contains(".db") || defaultConn.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
{
    string sqliteConn = string.IsNullOrEmpty(defaultConn) ? "Data Source=paranoiacorp.db" : defaultConn;
    builder.Services.AddDbContext<AuthContext>(options => options.UseSqlite(sqliteConn));
}
else if (defaultConn.Contains("Port=") || defaultConn.Contains("Host=") || defaultConn.Contains("Username=") || defaultConn.StartsWith("postgres", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AuthContext>(options => options.UseNpgsql(defaultConn));
}
else
{
    builder.Services.AddDbContext<AuthContext>(options => options.UseSqlServer(defaultConn));
}

// Configure SignalR
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IHallService, HallService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddSingleton<IWaitRoomRepository, RoomRepository>();
builder.Services.AddSingleton<IGameRepository, GameRepository>();
builder.Services.AddSingleton<IContextManager, ContextManager>();
builder.Services.AddSingleton<RequestManager>();

// Paranoia Corp services
builder.Services.AddSingleton<IParanoiaGameRepository, ParanoiaGameRepository>();
builder.Services.AddTransient<IParanoiaLobbyService, ParanoiaLobbyService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseRouting();

// Serve Web UI static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LobbyHub>("hub/lobby", options => options.CloseOnAuthenticationExpiration = false);

// Fallback to index.html for Single Page Application
app.MapFallbackToFile("index.html");

app.Run();
Console.WriteLine("[*] Paranoia Corp Server is running.");
