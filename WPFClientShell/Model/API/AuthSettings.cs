namespace WPFClientShell.Model.API
{
    public class AuthSettings
    {
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? JWT { get; set; }
        public string? RefreshToken { get; set; }
    }
}