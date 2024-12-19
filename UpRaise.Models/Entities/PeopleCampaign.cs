using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.Entities
{
    [Table("PeopleCampaigns")]
    public class PeopleCampaign
    {
        public PeopleCampaign()
        {
        }

        [Key]
        public int CampaignId { get; set; }

        [MaxLength(100)]
        public string BeneficiaryName { get; set; }

        [MaxLength(256)]
        public string BeneficiaryEmail { get; set; }

        [MaxLength(1000)]
        public string BeneficiaryMessage { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public virtual Campaign Campaign { get; set; }

    }
}