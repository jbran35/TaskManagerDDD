namespace TaskManager.Presentation.Services
{
    public class TokenProviderService
    {
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsInitialised => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(UserId);
    }
}
