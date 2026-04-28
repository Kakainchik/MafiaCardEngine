using WebServer.Helpers;
using WebServer.Model.User;

namespace WebServer.Services
{
    public class TokenService : ITokenService
    {
        private const string CREDENTIALS_ERROR = "Username or password is incorrect.";
        private const string INVALID_TOKEN = "Invalid token.";

        private readonly IJwtService jwtService;
        private readonly IUserRepository userRepository;
        private readonly ITokenRepository tokenRepository;

        public TokenService(IJwtService jwtService, IUserRepository userRepository, ITokenRepository tokenRepository)
        {
            this.jwtService = jwtService;
            this.userRepository = userRepository;
            this.tokenRepository = tokenRepository;
        }

        public async Task<JwtDTO> Authenticate(AuthenticateUserDTO dto)
        {
            UserEntity? user = await userRepository.GetUser(dto.Username);

            if(user == null || !PasswordHasher.ValidatePassword(dto.Password, user.Hash))
            {
                throw new BadHttpRequestException(CREDENTIALS_ERROR, StatusCodes.Status400BadRequest);
            }

            RefreshTokenDTO refreshToken = await jwtService.GenerateRefreshToken();

            RefreshTokenEntity tokenEntity = new RefreshTokenEntity()
            {
                Token = refreshToken.RefreshToken,
                Created = refreshToken.Created,
                Experies = refreshToken.Experies
            };
            await tokenRepository.AttachTokenToUser(user.Id, tokenEntity);

            await tokenRepository.RemoveUserOldTokens(user.Id);

            return new JwtDTO(user.Id,
                user.Username,
                jwtService.GenerateJwtToken(user.Id, user.Username),
                tokenEntity.Token);
        }

        public async Task<JwtDTO> RefreshToken(string token)
        {
            UserEntity? user = await userRepository.GetUserByRefreshToken(token);
            if(user is null)
            {
                ThrowInvalidToken();
            }

            RefreshTokenEntity refreshToken = user!.RefreshTokens.Single(t => t.Token == token);

            if(refreshToken.IsRevoked)
            {
                RevokeDescendantRefreshTokens(refreshToken,
                    user,
                    "Attempted reuse of revoked ancestor token");
                await userRepository.UpdateUser(user);
            }

            if(refreshToken.IsExpired)
            {
                ThrowInvalidToken();
            }

            //Replace old token with a new one
            RefreshTokenEntity newRefreshToken = await RotateRefreshToken(refreshToken);
            user.RefreshTokens.Add(newRefreshToken);

            await userRepository.UpdateUser(user);

            //Remove old refresh tokens
            await tokenRepository.RemoveUserOldTokens(user.Id);

            return new JwtDTO(user.Id,
                user.Username,
                jwtService.GenerateJwtToken(user.Id, user.Username),
                newRefreshToken.Token);
        }

        public async Task RevokeToken(string token)
        {
            UserEntity? user = await userRepository.GetUserByRefreshToken(token);
            if(user is null)
            {
                ThrowInvalidToken();
            }

            RefreshTokenEntity refreshToken = user!.RefreshTokens.Single(x => x.Token == token);

            if(!refreshToken.IsActive)
            {
                ThrowInvalidToken();
            }

            //Revoke token and save
            RevokeRefreshToken(refreshToken, "Revoked without replacement");
            
            await userRepository.UpdateUser(user);
        }

        private void RevokeDescendantRefreshTokens(RefreshTokenEntity refreshToken, UserEntity user, string reason)
        {
            //Recursively traverse the refresh token chain and ensure all descendants are revoked
            if(!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = user.RefreshTokens.SingleOrDefault(
                    x => x.Token == refreshToken.ReplacedByToken);

                if(childToken == null) return;

                if(childToken.IsActive)
                {
                    RevokeRefreshToken(childToken, reason);
                }
                else
                {
                    RevokeDescendantRefreshTokens(childToken, user, reason);
                }
            }
        }

        private void RevokeRefreshToken(RefreshTokenEntity token,
            string? reason = null,
            string? replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        private async Task<RefreshTokenEntity> RotateRefreshToken(RefreshTokenEntity token)
        {
            RefreshTokenDTO tokenDTO = await jwtService.GenerateRefreshToken();
            RefreshTokenEntity newRefreshToken = new RefreshTokenEntity()
            {
                 Token = tokenDTO.RefreshToken,
                 Created = tokenDTO.Created,
                 Experies = tokenDTO.Experies
            };
            RevokeRefreshToken(newRefreshToken,
                "Replaced by new token",
                newRefreshToken.Token);

            return newRefreshToken;
        }

        private void ThrowInvalidToken() => throw new BadHttpRequestException(INVALID_TOKEN, StatusCodes.Status400BadRequest);
    }
}