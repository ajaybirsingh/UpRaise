using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class DashboardActivityResponseDTO
    {
        public DashboardActivityResponseDTO()
        {
        }
        public int Id { get; set; }

        public string Icon { get; set; }

        public string UserAliasId { get; set; }

        public string Image { get; set; }

        public string Description { get; set; }

        public DateTimeOffset Date { get; set; }

        public string ExtraContent { get; set; }

        public string LinkedContent { get; set; }

        public string Link { get; set; }

        public bool? UseRouter { get; set; }
    }

}