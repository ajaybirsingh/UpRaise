using System;
using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs.Entities
{
    public class CampaignFileDTO
    {
        public int? Id { get; set; }
        public string UID { get; set; }
        public string Filename { get; set; }
        public int FileSize { get; set; }
        public Int16 SortOrder { get; set; }
    }
}