using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.DTOs.Entities
{
    public class PeopleCampaignDTO
    {
        public string BeneficiaryName { get; set; }
        public string BeneficiaryEmail { get; set; }
        public string BeneficiaryMessage { get; set; }
    }
}