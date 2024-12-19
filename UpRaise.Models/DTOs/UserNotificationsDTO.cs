using System;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class UserNotificationsDTO
    {
        public bool NotificationOnCampaignDonations { get; set; }
        public bool NotificationOnCampaignFollows { get; set; }
        public bool NotificationOnUpraiseEvents { get; set; }
    }
}

