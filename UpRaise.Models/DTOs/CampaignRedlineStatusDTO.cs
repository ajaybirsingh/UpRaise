using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class CampaignRedlineStatusDTO
    {
        public CampaignRedlineStatusDTO()
        {
        }


        public string transactionId { get; set; }
        public bool approved { get; set; }
    }

}