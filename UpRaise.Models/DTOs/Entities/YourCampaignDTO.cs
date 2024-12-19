using System;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs.Entities
{
    public class YourCampaignDTO
    {
        public int CampaignId { get; set; }
        public string TransactionId { get; set; }
        public CampaignStatuses Status { get; set; }
        public CampaignTypes Type { get; set; }
        public string HeaderPictureURL { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

    }
}