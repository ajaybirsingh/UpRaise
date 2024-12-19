using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum CampaignTypes : byte
    {
        Any = 0, //used for filtering
        Organization = 1,
        People = 2,
    }

}
