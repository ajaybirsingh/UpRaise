using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class DashboardCampaignRequestDTO
    {
        public DashboardCampaignRequestDTO()
        {
        }
        public int Id { get; set; }

        public CampaignTypes Type { get; set; }

        public DateRanges DateRange { get; set; }

    }

}