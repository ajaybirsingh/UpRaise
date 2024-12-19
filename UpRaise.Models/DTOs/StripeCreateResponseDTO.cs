using System;
using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class StripeCreateResponseDTO
    {
        public string ClientSecret { get; set; }
    }
}