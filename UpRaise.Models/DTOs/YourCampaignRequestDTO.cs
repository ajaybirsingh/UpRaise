using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class YourCampaignRequestDTO
    {
        public YourCampaignRequestDTO()
        {
        }
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public YourCampaignSortOrders SortOrder { get; set; }
    }

}

