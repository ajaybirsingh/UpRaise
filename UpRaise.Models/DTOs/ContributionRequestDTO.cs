using System;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class ContributionRequestDTO
    {
        public ContributionTypes ContributionType { get; set; }

        public int CampaignType { get; set; }
        public int CampaignId { get; set; }

        public float Amount { get; set; }
        public string Comment {get;set;}
    }
}