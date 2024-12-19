using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class GeocodingResponseDTO
    {
        public GeocodingResponseDTO()
        {
        }
        public string FormattedAddress { get; set; }
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

}