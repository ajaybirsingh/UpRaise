using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum Countries : byte
    {
        [Description("Canada")]
        Canada = 1,

        [Description("United States")]
        USA = 2,
    }
}