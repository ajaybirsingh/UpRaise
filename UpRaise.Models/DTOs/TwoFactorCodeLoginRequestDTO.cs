using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class TwoFactorCodeLoginRequestDTO
    {
        public string Username { get; set; }

        public string Password { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Two Factor Code")]
        public string TwoFactorCode { get; set; }

        public bool UseRecoveryCode { get; set; }
        public bool RememberMachine { get; set; }

    }
}

