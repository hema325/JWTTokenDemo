namespace JWTToken.Common.Models
{
    public class AuthResult
    {
        public bool IsSucceeded { get; init; }
        public IEnumerable<string> Errors { get; init; }

        public string JwtToken { get; init; }
        public DateTime JwtTokenExpiration { get; init; }

        public string RefreshToken { get; init; }
        public DateTime RefreshTokenExpiration { get; init; }
    }
}
