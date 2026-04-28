using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebServer.Model.User;
using WebServer.Services;

namespace WebServer.Controllers.Authentication
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private const string REFRESH_TOKEN_COOKIE = "RefreshToken";

        private readonly IUserService userService;
        private readonly ITokenService tokenService;

        public AuthenticationController(IUserService userService, ITokenService tokenService)
        {
            this.userService = userService;
            this.tokenService = tokenService;
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1)
        {
            IEnumerable<UserDTO> users = await userService.GetUsers(page);
            return Ok(users);
        }

        [HttpPost("sign_up")]
        public async Task<IActionResult> SignUp([FromBody] CreateUserDTO request)
        {
            bool success = await userService.CreateUser(request);

            if(success)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("sign_in")]
        public async Task<IActionResult> SignIn([FromBody] AuthenticateUserDTO request)
        {
            JwtDTO jwt = await tokenService.Authenticate(request);

            AuthenticationResponse response = new AuthenticationResponse(jwt.Id,
                jwt.Username,
                jwt.JwtToken,
                jwt.RefreshToken);
            SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [HttpPost("refresh_token")]
        public async Task<IActionResult> RefreshToken([FromQuery] string? tkn)
        {
            var token = Request.Cookies[REFRESH_TOKEN_COOKIE] ?? tkn;
            if(string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            JwtDTO jwt = await tokenService.RefreshToken(token);

            AuthenticationResponse response = new AuthenticationResponse(jwt.Id,
                jwt.Username,
                jwt.JwtToken,
                jwt.RefreshToken);
            SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [HttpPost("revoke_token")]
        public async Task<IActionResult> RevokeToken()
        {
            //Accept refresh token in cookie
            string? token = Request.Cookies[REFRESH_TOKEN_COOKIE];

            if(string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            await tokenService.RevokeToken(token);
            return Ok();
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append(REFRESH_TOKEN_COOKIE, token, cookieOptions);
        }
    }
}