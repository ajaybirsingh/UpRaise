using System;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicCampaignRequestDTO
    {
        public int NumberOfCampaigns { get; set; }

        public CampaignTypes FilterCampaignType { get; set; }

        public int FilterCategoryId { get; set; }

        public string FilterTitleOrDescription { get; set; }

        public bool FilterHideCompleted { get; set; }

        public ExploreViewTypes ExploreViewType { get; set; }

        public float[] MapBounds { get; set; }
    }

}