namespace JWTToken.Common.Models
{
    public class TokenResult
    {
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
