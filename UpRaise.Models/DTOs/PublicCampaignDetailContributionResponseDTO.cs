using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{

    public class PublicCampaignDetailContributionResponseDTO
    {
        public string UserAliasId { get; set; }
        public string UserName { get; set; }

        public DateTimeOffset Date { get; set; }

        public Decimal Amount { get; set; }

        public string Note { get; set; }

        public ContributionStatuses Status { get; set; }
    }

}