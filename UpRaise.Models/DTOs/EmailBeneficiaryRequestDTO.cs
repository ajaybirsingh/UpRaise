using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class EmailBeneficiaryRequestDTO
    {
        public EmailBeneficiaryRequestDTO()
        {
        }
        public int CampaignId { get; set; }
    }

}