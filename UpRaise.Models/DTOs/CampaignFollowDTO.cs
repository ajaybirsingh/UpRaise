using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class CampaignFollowDTO
    {
        public CampaignFollowDTO()
        {
        }
        public int Id { get; set; }
        public CampaignTypes CampaignType { get; set; }
        public bool IsFollowing { get; set; }
    }

}