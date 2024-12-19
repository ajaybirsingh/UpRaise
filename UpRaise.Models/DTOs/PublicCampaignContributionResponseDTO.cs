using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicCampaignContributionResponseDTO
    {
        public PublicCampaignContributionResponseDTO()
        {
        }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalNumberOfContributions { get; set; }
        public int TotalPages { get; set; }

        public IEnumerable<PublicCampaignDetailContributionResponseDTO> Contributions { get; set; }
    }

}