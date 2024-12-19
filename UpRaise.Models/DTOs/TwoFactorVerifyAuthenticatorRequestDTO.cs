using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class TwoFactorVerifyAuthenticatorRequestDTO
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string VerificationCode { get; set; }
    }
}