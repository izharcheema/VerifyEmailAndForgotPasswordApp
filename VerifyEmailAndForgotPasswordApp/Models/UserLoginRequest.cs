namespace VerifyEmailAndForgotPasswordApp.Models
{
    public class UserLoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Hey Dude!!! Enter Atleast 6 Characters!")]
        public string Password { get; set; } = string.Empty;
    }
}
