using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum CampaignStatuses : byte
    {
        Disabled = 0, 
        Active = 1,
        PendingAcceptance = 2,
    }

}
