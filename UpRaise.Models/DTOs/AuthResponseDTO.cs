using System;
using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class AuthResponseDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
    }
}