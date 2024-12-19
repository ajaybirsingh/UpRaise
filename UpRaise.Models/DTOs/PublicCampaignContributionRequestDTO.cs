using System;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicCampaignContributionRequestDTO
    {
        public int CampaignId { get; set; }
        public CampaignTypes CampaignType { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public ContributionSortFields SortField { get; set; }
        public SortDirections SortDirection { get; set; }
    }
}