using JWTToken.Common.Models;
using System.Security.Claims;

namespace JWTToken.Services.JWTService
{
    public interface IJWTService
    {
        Task<TokenResult> GetJwtTokenAsync(IEnumerable<Claim> claims);
    }
}
