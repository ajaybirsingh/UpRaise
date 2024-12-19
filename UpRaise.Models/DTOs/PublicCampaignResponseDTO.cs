using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicCampaignResponseDTO
    {
        public PublicCampaignResponseDTO()
        {
        }
        public int Id { get; set; }

        public CampaignTypes TypeId { get; set; }

        public OrganizationCampaignCategories? CategoryId { get; set; }

        public string Name { get; set; }

        public string Summary { get; set; }
        public string Description { get; set; }
        public string HeaderPictureURL { get; set; }
        public string BeneficiaryOrganizationName { get; set; }

        public string Category { get; set; }

        public string Beneficiary { get; set; }

        public int NumberOfDonors { get; set; }


        public string Location { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }


        public Decimal? FundraisingGoals { get; set; }

        public bool Featured{ get; set; }

        public bool Completed { get; set; }

        public int? DaysLeft { get; set; }

        public bool FundedPreviously { get; set; }

        public int FundedPercentage { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }


        public Decimal FundingAmountToDate { get; set; }

        public DateTimeOffset? FundingLastDateTime { get; set; }

        public string OrganizedBy { get; set; }
        public string OrganizedByProfilePictureURL { get; set; }

        public string SEOFriendlyURL { get; set; }

        public IEnumerable<string> Photos { get; set; }
        public IEnumerable<string> Videos { get; set; }
    }

}