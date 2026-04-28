using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Diagnostics;
using System.Text;
using WebServer.Hubs;
using WebServer.Model;
using WebServer.Model.Game;
using WebServer.Model.Room;
using WebServer.Model.User;
using WebServer.Services;

var builder = WebApplication.CreateBuilder(args);

//Configure JWT
IConfigurationSection jwtSection = builder.Configuration.GetSection("Jwt");
JwtSettings? jwtSettings = jwtSection.Get<JwtSettings>();
string? jwtSecret = builder.Configuration["UserAuth:JwtSecret"];

if(jwtSettings is not null
    && jwtSecret is not null)
{
    jwtSettings.Secret = jwtSecret;
    builder.Services.Configure<JwtSettings>(jwt =>
    {
        jwt.Secret = jwtSecret;
        jwt.Issuer = jwtSettings.Issuer;
        jwt.Audience = jwtSettings.Audience;
        jwt.RefreshTokenTTL = jwtSettings.RefreshTokenTTL;
    });
}
else
{
    Console.Error.WriteLine($"[jwtSettings]: {jwtSettings}");
    throw new NullReferenceException("JWT Secret is not installed!");
}

// Add services to the container.

//Logging
builder.Services.AddLogging(log =>
{
    log.AddDebug();
    log.AddConsole();
});

//Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        SymmetricSecurityKey ssk = new(Encoding.ASCII.GetBytes(jwtSecret));
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = ssk,
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = delegate (MessageReceivedContext context)
            {
                StringValues accessToken = context.Request.Query["accessToken"];

                //If the request is for hubs
                PathString path = context.HttpContext.Request.Path;
                if(!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hub"))
                {
                    //Read the token out of the query string
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

//Exception handler
builder.Services.AddExceptionHandler(options =>
{
    options.ExceptionHandler = async delegate (HttpContext context)
    {
        switch(context.Features.Get<IExceptionHandlerPathFeature>()?.Error)
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

//Configure DB Context
//var sqlBuilder = new NpgsqlConnectionStringBuilder(builder.Configuration["UserAuth:DefaultConnection"]);
var sqlBuilder = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));

//builder.Services.AddDbContext<AuthContext>(options => options.UseNpgsql(sqlBuilder.ConnectionString));
builder.Services.AddDbContext<AuthContext>(options => options.UseSqlServer(sqlBuilder.ConnectionString));

//Configure SignalR
builder.Services.AddSignalR();

//Add custom services
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Middlewares
app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LobbyHub>("hub/lobby", options => options.CloseOnAuthenticationExpiration = false);

app.Run();

Console.WriteLine("[*] The app has ran successfully!");