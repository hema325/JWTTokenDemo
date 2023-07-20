using JWTToken.DTOs;
using JWTToken.Entities;
using JWTToken.Constants;
using JWTToken.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using JWTToken.Common.Models;
using JWTToken.Services.JWTService;

namespace JWTToken.Services.AuthService
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJWTService _jwtService;

        public AuthService(UserManager<ApplicationUser> userManager, IJWTService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<AuthResult> Authenticate(AuthenticateDTO auth)
        {
            var user = await _userManager.FindByEmailAsync(auth.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, auth.Password))
                return new AuthResult { Errors = new[] { "Email Or Password Is not correct" } };

            var jwtToken = await _jwtService.GetJwtTokenAsync(await GetUserClaimsAsync(user));
            var refreshToken = await GetFirstActiveOrNewUserRefreshToken(user);

            return new AuthResult
            {
                IsSucceeded = true,
                JwtToken = jwtToken.Token,
                JwtTokenExpiration = jwtToken.ExpiresOn,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn
            };
        }

        public async Task<AuthResult> GetJWTTokenUsingRefreshTokenAsync(string token)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == token));

            if (user == null)
                return new AuthResult { Errors = new[] { "Invalid Token" } };

            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive)
                return new AuthResult { Errors = new[] { "Invalid Token" } };

            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            var jwtToken = await _jwtService.GetJwtTokenAsync(await GetUserClaimsAsync(user));

            return new AuthResult
            {
                JwtToken = jwtToken.Token,
                JwtTokenExpiration = jwtToken.ExpiresOn,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiresOn,
                IsSucceeded = true
            };
        }

        public async Task<Result> RevokeAsync(string token)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == token));

            if (user == null)
                return new Result { Errors = new[] { "Invalid Token" } };

            var refreshToken = user.RefreshTokens.Single(rt => rt.Token == token);

            if (!refreshToken.IsActive)
                return new Result { Errors = new[] { "Invalid Token" } };

            refreshToken.RevokedOn = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new Result { IsSucceeded = true };
        }

        public async Task<AuthResult> Register(RegisterDTO dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return new AuthResult { Errors = new[] { "Email is already registered!" } };

            if (await _userManager.FindByNameAsync(dto.UserName) != null)
                return new AuthResult { Errors = new[] { "Email is already registered!" } };

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return new AuthResult { Errors = result.Errors.Select(e => e.Description) };

            await _userManager.AddToRoleAsync(user, Roles.User.ToString());

            var jwtToken = await _jwtService.GetJwtTokenAsync(await GetUserClaimsAsync(user));
            var refreshToken = await GetFirstActiveOrNewUserRefreshToken(user);

            return new AuthResult
            {
                IsSucceeded = true,
                JwtToken = jwtToken.Token,
                JwtTokenExpiration = jwtToken.ExpiresOn,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiresOn,
            };
        }

        private RefreshToken GenerateRefreshToken()
        {
            var token = new byte[32];

            using var generator = RandomNumberGenerator.Create();
            generator.GetBytes(token);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(token),
                ExpiresOn = DateTime.UtcNow.AddDays(10),
                CreatedOn = DateTime.UtcNow
            };
        }
        private async Task<IEnumerable<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.GivenName,$"{user.FirstName} {user.LastName}")
            }
            .Union(await _userManager.GetClaimsAsync(user))
            .Union((await _userManager.GetRolesAsync(user)).Select(r => new Claim(ClaimTypes.Role, r)));
        }

        private async Task<RefreshToken> GetFirstActiveOrNewUserRefreshToken(ApplicationUser user)
        {
            if (user.RefreshTokens.Any(t => t.IsActive))
                return user.RefreshTokens.FirstOrDefault(t => t.IsActive);

            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            return refreshToken;
        }
    }
}
