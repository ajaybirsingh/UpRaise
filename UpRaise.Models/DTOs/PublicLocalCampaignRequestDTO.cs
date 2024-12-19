using System;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class PublicLocalCampaignRequestDTO
    {
        public float east { get; set; }
        public float north { get; set; }
        public float west { get; set; }
        public float south { get; set; }
    }
}