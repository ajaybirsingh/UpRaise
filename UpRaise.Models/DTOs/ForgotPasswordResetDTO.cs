namespace UpRaise.DTOs
{
    public class ForgotPasswordResetDTO
    { 
        public string Username { get; set; }
        public string ResetToken { get; set; }
        public string Password { get; set; }
    }
}