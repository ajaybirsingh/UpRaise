using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class GeocodingRequestDTO
    {
        public GeocodingRequestDTO()
        {
        }
        //public Countries CountryId { get; set; }
        public string Location { get; set; }
    }

}