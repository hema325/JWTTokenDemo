using JWTToken.Common.Models;
using JWTToken.DTOs;

namespace JWTToken.Services.AuthService
{
    public interface IAuthService
    {
        Task<AuthResult> Authenticate(AuthenticateDTO auth);
        Task<AuthResult> GetJWTTokenUsingRefreshTokenAsync(string token);
        Task<Result> RevokeAsync(string token);
        Task<AuthResult> Register(RegisterDTO dto);
    }
}
