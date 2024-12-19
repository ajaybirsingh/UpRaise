using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs.Entities
{
    public class EditCampaignDTO
    {
        public int? Id { get; set; }

        public string TransactionId { get; set; }

        public CampaignTypes TypeId { get; set; }

        public Models.Enums.Countries? GeoLocationCountryId { get; set; }

        public string GeoLocationAddress { get; set; }

        public EditOrganizationCampaignDTO Organization { get; set; }
        public EditPeopleCampaignDTO People { get; set; }

        public decimal? FundraisingGoals { get; set; }

        public Currencies CurrencyId { get; set; }

        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public EditCampaignFileDTO HeaderPhoto { get; set; }
        public IEnumerable<EditCampaignFileDTO> Photos { get; set; }
        public IEnumerable<EditCampaignFileDTO> Videos { get; set; }

        public bool AcceptTermsAndConditions { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<string> DistributionTerms { get; set; }
    }

    public class EditOrganizationCampaignDTO
    {
        public OrganizationCampaignCategories CategoryId { get; set; }

        public string BeneficiaryOrganization { get; set; }
        public string Location { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public IEnumerable<string> CampaignConditions { get; set; }


        public string BeneficiaryMessage { get; set; }

    }


    public class EditPeopleCampaignDTO
    {
        public string BeneficiaryName { get; set; }
        public string BeneficiaryEmail { get; set; }
        public string BeneficiaryMessage { get; set; }

    }

    public class EditCampaignFileDTO
    {
        public int Id { get; set; }
        public string url { get; set; }
    }



}