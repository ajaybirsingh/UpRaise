using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum DateRanges: byte
    {
        All = 0,
        LastWeek = 1,
        LastMonth = 2,
        LastYear = 3,
        YTD = 4
    }

}
