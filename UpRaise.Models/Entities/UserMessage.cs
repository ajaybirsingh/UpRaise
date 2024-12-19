using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.Entities
{
    [Table("UserMessages")]
    public class UserMessage
    {
        public UserMessage()
        {
        }

        [Key]
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public int ToUserId { get; set; }
        public int? FromUserId { get; set; }

        [MaxLength(256)]
        public string FromSubject { get; set; }

        [MaxLength(2048)]
        public string FromMessage { get; set; }

        [MaxLength(256)]
        public string FromFirstName { get; set; }

        [MaxLength(256)]
        public string FromLastName { get; set; }

        [MaxLength(256)]
        public string FromEmail { get; set; }

        [MaxLength(32)]
        public string FromPhone { get; set; }

        public UserMessagesStatuses StatusId { get; set; }

        public bool Unread { get; set; }

        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ReadAt { get; set; }
        
        [MaxLength(16)]
        public byte[] IPAddress { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign Campaign { get; set; }

        [ForeignKey("ToUserId")]
        public virtual IDUser ToUser { get; set; }

        [ForeignKey("FromUserId")]
        public virtual IDUser FromUser { get; set; }
    }
}