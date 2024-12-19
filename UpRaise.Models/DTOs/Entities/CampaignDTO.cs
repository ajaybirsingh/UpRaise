using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs.Entities
{
    public class CampaignDTO
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public CampaignTypes TypeId { get; set; }

        public string Description { get; set; }

        public string HeaderPictureURL { get; set; }

        public string[] DistributionTerms { get; set; }
        public string TransactionId { get; set; }
        public Decimal FundraisingGoals { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public int CreatedByUserId { get; set; }

        public Countries? GeoLocationCountryId { get; set; }

        public Currencies CurrencyId { get; set; }

        public string GeoLocationAddress { get; set; }

        public double? GeoLocationLatitude { get; set; }
        public double? GeoLocationLongitude { get; set; }


        public CampaignFileDTO HeaderPhoto { get; set; }
        public CampaignFileDTO[] Photos { get; set; }
        public CampaignFileDTO[] Videos { get; set; }

        public OrganizationCampaignDTO OrganizationCampaignDTO { get; set; }
        public PeopleCampaignDTO PeopleCampaignDTO { get; set; }

    }
}