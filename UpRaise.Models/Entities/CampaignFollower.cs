using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.Entities
{
    [Table("CampaignFollowers")]
    public class CampaignFollower
    {
        public CampaignFollower()
        {
        }

        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }
        public int? UserId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("UserId")]
        public virtual IDUser User { get; set; }
    }
}