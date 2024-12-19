using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.DTOs.Entities;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class YourCampaignResponseDTO
    {
        public YourCampaignResponseDTO()
        {
        }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalPages { get; set; }
        public int TotalItems { get; set; }


        public List<YourCampaignDTO> YourCampaigns { get; set; }
    }


  

}