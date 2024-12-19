using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class CampaignRedlineEventDTO
    {
        public CampaignRedlineEventDTO()
        {
        }
        public int CampaignId { get; set; }

        public string UserAliasId { get; set; }
        public string UserFullName { get; set; }

        public string Note { get; set; }

        public CampaignRedlineEventTypes EventType { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }

}