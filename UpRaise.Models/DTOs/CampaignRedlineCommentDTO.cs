using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class CampaignRedlineCommentDTO
    {
        public CampaignRedlineCommentDTO()
        {
        }

        public int CampaignId { get; set; }
        public string UserAliasId { get; set; }
        public string UserFullName { get; set; }

        public string Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

}