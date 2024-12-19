using System.Collections.Generic;

namespace UpRaise.DTOs
{
    public class TwoFactorAuthAuthenticatorDetailDTO
    {
        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }
    }
}