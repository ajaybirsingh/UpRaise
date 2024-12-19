using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs.Redis
{
    [Serializable]
    public class SEOFriendlyURLDTO
    {
        public byte TypeId { get; set; }
        public int Id { get; set; }
    }
}