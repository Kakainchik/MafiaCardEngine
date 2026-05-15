using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebServer.Model.User;

namespace WebServer.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings jwtSettings;
        private readonly ITokenRepository tokenRepository;

        public JwtService(IOptions<JwtSettings> jwtSettings, ITokenRepository tokenRepository)
        {
            this.jwtSettings = jwtSettings.Value;
            this.tokenRepository = tokenRepository;
        }

        /// <summary>
        /// Gets random token sized 40 bytes.
        /// </summary>
        /// <returns>Token string.</returns>
        public string GenerateJwtToken(ulong userId, string username)
        {
            //Generate token that is valid for 20 day
            byte[] key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString(), ClaimValueTypes.Integer64),
                    new Claim(ClaimTypes.Name, username, ClaimValueTypes.String)
                }),
                Expires = DateTime.UtcNow.AddDays(20),
                Audience = jwtSettings.Audience,
                Issuer = jwtSettings.Issuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<RefreshTokenDTO> GenerateRefreshToken()
        {
            async Task<string> GetUniqueToken()
            {
                string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
                bool isExist = await tokenRepository.IsTokenExist(token);

                if(isExist)
                {
                    return await GetUniqueToken();
                }
                else
                {
                    return token;
                }
            }

            string newToken = await GetUniqueToken();
            RefreshTokenDTO entity = new RefreshTokenDTO(refreshToken: newToken,
                created: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMonths(4));

            return entity;
        }

        public int? ValidateJwtToken(string token)
        {
            byte[] key = Encoding.ASCII.GetBytes(token);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "Id").Value);

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}