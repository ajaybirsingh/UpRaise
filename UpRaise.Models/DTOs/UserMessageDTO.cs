using System;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class UserMessageDTO
    {
        public CampaignTypes CampaignType { get; set; }
        public int CampaignId { get; set; }
        public string ToUserAliasId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}

