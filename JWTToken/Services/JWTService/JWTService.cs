using JWTToken.Common.Models;
using JWTToken.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JWTToken.Services.JWTService
{
    public class JWTService: IJWTService
    {
        private readonly JWTSettings _jwtSettings;

        public JWTService(IOptions<JWTSettings> options)
        {
            _jwtSettings = options.Value;
        }

        public async Task<TokenResult> GetJwtTokenAsync(IEnumerable<Claim> claims)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(issuer: _jwtSettings.Issuer,
                                                        audience: _jwtSettings.Audience,
                                                        claims: claims,
                                                        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                                                        signingCredentials: signingCredentials);

            return new TokenResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                ExpiresOn = jwtSecurityToken.ValidTo
            };
        }
    }
}
