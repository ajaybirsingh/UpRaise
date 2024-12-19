using System;
using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class StripeCreateRequestDTO
    {
        public int CampaignId { get; set; }
        public float Amount { get; set; }
        public string Token {get;set;}
    }
}