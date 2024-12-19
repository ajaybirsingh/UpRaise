using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace UpRaise.Models.Search
{
    public class PeopleCampaign
    {
        [SearchableField(IsFilterable = true)]
        public string CampaignId { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
