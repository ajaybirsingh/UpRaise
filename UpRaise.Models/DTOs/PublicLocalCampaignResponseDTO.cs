using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicLocalCampaignResponseDTO
    {
        public PublicLocalCampaignResponseDTO()
        {
            Campaigns = new List<PublicLocalCampaignDataDTO>();
        }

        public List<PublicLocalCampaignDataDTO> Campaigns { get; set; }

    }

    public class PublicLocalCampaignDataDTO
    {
        public PublicLocalCampaignDataDTO()
        {
        }
        public int Id { get; set; }

        public CampaignTypes TypeId { get; set; }

        public string PictureURL { get; set; }

        public int FundedPercentage { get; set; }

        public string Category { get; set; }
        public string Beneficiary { get; set; }

        public string Name { get; set; }

        public string Summary { get; set; }

        public Decimal? FundraisingGoals { get; set; }

        public string SEOFriendlyURL { get; set; }
        public int NumberOfDonors { get; set; }
        public string OrganizedByProfilePictureURL { get; set; }
        public string OrganizedBy { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }

}