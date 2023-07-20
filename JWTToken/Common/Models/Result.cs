namespace JWTToken.Common.Models
{
    public class Result
    {
        public bool IsSucceeded { get; init; }
        public IEnumerable<string> Errors { get; init; }
    }
}
