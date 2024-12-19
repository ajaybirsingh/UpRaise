using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.Entities
{
    [Table("NewsletterSubscriptions")]
    public class NewsletterSubscription
    {
        public NewsletterSubscription()
        {
        }

        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual IDUser User { get; set; }
    }
}