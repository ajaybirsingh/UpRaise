using System;
using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class CryptoCreateRequestDTO
    {
        public int CampaignType { get; set; }
        public int CampaignId { get; set; }
        public string Comment { get; set; }
        public float Amount { get; set; }

        public int UserId { get; set; }

    }
}