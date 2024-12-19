using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicClosestLocationDTO
    {
        public PublicClosestLocationDTO()
        {
        }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

}