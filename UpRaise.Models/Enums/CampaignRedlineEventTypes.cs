using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum CampaignRedlineEventTypes : byte
    {
        Created = 1,
        LookedAtBy = 2,
        ChangeRequest = 3,
        Accepted = 4,
        Rejected = 5,
    }

}
