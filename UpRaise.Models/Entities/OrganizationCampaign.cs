using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.Entities
{
    [Table("OrganizationCampaigns")]
    public class OrganizationCampaign
    {
        public OrganizationCampaign()
        {
        }

        [Key, ForeignKey("Campaign")]
        public int CampaignId { get; set; }


        public Models.Enums.OrganizationCampaignCategories CategoryId { get; set; }

        public int? BeneficiaryOrganizationId { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(100)]
        public string ContactName { get; set; }

        [MaxLength(100)]
        public string ContactEmail { get; set; }

        [MaxLength(100)]
        public string ContactPhone { get; set; }

        [MaxLength(1000)]
        public string Conditions { get; set; }

        [MaxLength(1000)]
        public string BeneficiaryMessage { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        [ForeignKey("BeneficiaryOrganizationId")]
        public virtual BeneficiaryOrganization BeneficiaryOrganization { get; set; }

        public virtual Campaign Campaign { get; set; }
    }
}