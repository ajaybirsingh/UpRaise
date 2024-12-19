using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum OrganizationCampaignCategories : byte
    {
        [Description("Arts & Culture")]
        ArtsAndCulture = 1,

        [Description("Education")]
        Education = 2,

        [Description("Environment")]
        Environment = 3,

        [Description("Animal & Humane")]
        AnimalAndHumane = 4,

        [Description("Disaster Relief")]
        DisasterRelief = 5,

        [Description("Health & Medical")]
        HealthAndMedical = 6,

        [Description("Active Duty & Veterans")]
        ActiveDutyAndVeterans = 7,

        [Description("Human Services")]
        HumanServices = 8,

        [Description("Global")]
        Global = 9,

        [Description("Social Action")]
        SocialAction = 10,

        [Description("Community & Family")]
        CommunityAndFamily = 11,

        [Description("Faith & Missions")]
        FaithAndMissions = 12,
    }
}