using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.Entities
{
    [Table("CampaignFiles")]
    public class CampaignFile
    {
        public CampaignFile()
        {
        }

        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }

        [MaxLength(36)]
        public string UID { get; set; } //unique file id to allow saving of files before db entry exists and prevent name clashing

        [MaxLength(256)]
        public string Filename { get; set; }

        public CampaignFileTypes TypeId { get; set; }

        public int SizeInBytes { get; set; }

        public Int16? SortOrder { get; set; }

        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public int CreatedByUserId { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }


        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual IDUser User { get; set; }
    }
}