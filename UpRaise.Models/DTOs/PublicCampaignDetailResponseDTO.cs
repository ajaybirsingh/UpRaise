using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicCampaignDetailResponseDTO
    {
        public PublicCampaignDetailResponseDTO()
        {
        }

        public bool IsCurrentSignedInUserCreator { get; set; }
        public int Id { get; set; }
        public string TransactionId { get; set; }

        public CampaignTypes Type { get; set; }

        public string Category { get; set; }


        public int NumberOfFollowers { get; set; }
        public int NumberOfVisits { get; set; }
        public int NumberOfShares { get; set; }
        public int NumberOfDonors { get; set; }

        public string HeaderPictureURL { get; set; }
        public string CreatedByUserFullName { get; set; }
        public Guid CreatedByUserAliasId { get; set; }
        public bool ShowProfilePic { get; set; }
        public bool IsUserFollowing { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string BeneficiaryOrganizationName { get; set; }

        public string Location { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }


        public Decimal? FundraisingGoals { get; set; }

        public bool Featured { get; set; }

        public bool Completed { get; set; }

        public int? DaysLeft { get; set; }
        public int NumberOfContributions { get; set; }

        public bool FundedPreviously { get; set; }

        public int FundedPercentage { get; set; }

        public Decimal FundingAmountToDate { get; set; }

        public DateTimeOffset? FundingLastDateTime { get; set; }
        public string SEOFriendlyURL { get; set; }
        public IEnumerable<string> Photos { get; set; }
        public IEnumerable<string> Videos { get; set; }
    }

}