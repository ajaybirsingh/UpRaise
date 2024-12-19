using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.Entities
{
    [Table("CampaignRedlineEvents")]
    public class CampaignRedlineEvent
    {
        public CampaignRedlineEvent()
        {
        }

        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }
        public int UserId { get; set; }

        [MaxLength(256)]
        public string Note { get; set; }

        public CampaignRedlineEventTypes EventType { get; set; }

        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }


        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("UserId")]
        public virtual IDUser User { get; set; }
    }
}