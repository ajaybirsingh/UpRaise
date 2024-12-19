using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.Entities
{
    [Table("CampaignAnalytics")]
    public class CampaignAnalytic
    {
        public CampaignAnalytic()
        {
        }

        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }

        [MaxLength(16)]
        public byte[] IPAddress { get; set; }

        [MaxLength(256)]
        public string UserAgent { get; set; }

        public int? UserId { get; set; }

        public CampaignAnalyticTypes TypeId { get; set; }

        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }


        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("UserId")]
        public virtual IDUser User { get; set; }
    }
}