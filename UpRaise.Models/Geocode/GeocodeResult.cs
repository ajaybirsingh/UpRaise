using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public class GeocodeResult
    {
        public GeocodeInfo[] data { get; set; }
    }

    public class GeocodeInfo
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string number { get; set; }
        public string street { get; set; }
        public string postal_code { get; set; }
        public double confidence { get; set; }
        public string region { get; set; }
        public string region_code { get; set; }
        public string administrative_area { get; set; }
        public string neighbourhood { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string map_url { get; set; }
    }

}
