using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class DashboardCampaignResponseDTO
    {
        public DashboardCampaignResponseDTO()
        {
            Contributions = new List<DashboardCampaignContributionDTO>();
        }
        public int Id { get; set; }

        public CampaignTypes Type { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string BeneficiaryName { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        public int? DaysLeft { get; set; }

        public int NumberOfTotalContributors { get; set; }
        public int NumberOfUniqueContributors { get; set; }

        public decimal? FundraisingGoal { get; set; }

        public decimal AmountRaised { get; set; }

        public List<DashboardCampaignContributionDTO> Contributions { get; set; }
    }

    public class DashboardCampaignContributionDTO
    { 
        public int Id { get; set; }

        public DateTimeOffset Date { get; set; }

        public Decimal Amount { get; set; }

        public ContributionTypes ContributionTypeId { get; set; }
    }

}