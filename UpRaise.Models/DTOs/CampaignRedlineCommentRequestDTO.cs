using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class CampaignRedlineCommentRequestDTO
    {
        public CampaignRedlineCommentRequestDTO()
        {
        }
        public int CampaignId { get; set; }

        public string Comment { get; set; }
    }

}