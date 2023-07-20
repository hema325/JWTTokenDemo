using JWTToken.DTOs;
using JWTToken.Services.AuthService;
using Microsoft.AspNetCore.Mvc;

namespace JWTToken.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            return Ok(await _auth.Register(dto));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Authenticate(AuthenticateDTO dto)
        {
            var result = await _auth.Authenticate(dto);
            AddRefreshTokenToCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RequestJWTTokenUsingRefreshToken(string token)
        {
            var refreshToken = token ?? Request.Cookies["refreshToken"];
            var result = await _auth.GetJWTTokenUsingRefreshTokenAsync(refreshToken);
            
            if (!result.IsSucceeded)
            {
                Response.Cookies.Delete("refreshToken");
                return BadRequest(result);
            }

            AddRefreshTokenToCookie(result.RefreshToken, result.RefreshTokenExpiration);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> RevokeToken(string token)
        {
            var refreshToken = token ?? Request.Cookies["refreshToken"];
            var result = await _auth.RevokeAsync(refreshToken);

            Response.Cookies.Delete("refreshToken");
            if (!result.IsSucceeded)
                return BadRequest(result);

            return Ok(result);
        }

        private void AddRefreshTokenToCookie(string token,DateTime expires)
        {
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                IsEssential = true,
                Expires = expires
            };

            Response.Cookies.Append("refreshToken", token, options);
        }
    }
}
