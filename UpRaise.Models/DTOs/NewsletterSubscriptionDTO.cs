using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class NewsletterSubscriptionDTO
    {
        public NewsletterSubscriptionDTO()
        {
        }
        public string Email { get; set; }
    }

}