using System;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs.Entities
{
    public class OrganizationCampaignDTO
    {
        public OrganizationCampaignCategories CategoryId { get; set; }
        public int? BeneficiaryOrganizationId { get; set; }
        public string Location { get; set; }

        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string[] CampaignConditions { get; set; }

        public string BeneficiaryMessage { get; set; }
    }
}