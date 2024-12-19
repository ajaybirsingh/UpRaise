using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class CampaignAnalyticDTO
    {
        public CampaignAnalyticDTO()
        {
        }
        public int Id { get; set; }
        public CampaignTypes CampaignType { get; set; }
        public CampaignAnalyticTypes AnalyticType { get; set; }
    }

}