using System;
using System.ComponentModel.DataAnnotations;

namespace UpRaise.DTOs
{
    public class EnumDTO
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}