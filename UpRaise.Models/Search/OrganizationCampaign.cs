using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpRaise.Models.Search
{
    public class OrganizationCampaign
    {
        [SearchableField(IsFilterable = true)]
        public string CampaignId { get; set; }


        [SearchableField(IsFilterable = true)]
        public string CategoryId { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
