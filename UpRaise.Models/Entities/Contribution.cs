using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.Entities
{
    [Table("Contributions")]
    public class Contribution
    {
        public Contribution()
        {
        }

        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public int UserId { get; set; }

        public ContributionTypes ContributionTypeId { get; set; }

        [Column(TypeName = "decimal(10, 4)")]
        public Decimal Amount { get; set; }

        [MaxLength(1000)]
        public string Note { get; set; }

        public bool Anonymous { get; set; }

        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }


        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("UserId")]
        public virtual IDUser User { get; set; }
    }
}