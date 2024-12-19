using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    //[Flags]
    public enum ContributionStatuses : byte
    {
        RecentDonation = 1,
        TopDonation = 2,
        FirstDonation = 3,
    }

}
