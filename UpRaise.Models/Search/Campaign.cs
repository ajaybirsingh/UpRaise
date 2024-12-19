using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UpRaise.Models.Search
{
    public class Campaign
    {
        [SearchableField(IsFilterable = true, IsKey = true)]
        public string Id { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string Name { get; set; }


        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string Description { get; set; }


        public DateTimeOffset? StartDate { get; set; }


        public DateTimeOffset? EndDate { get; set; }


        [SearchableField(IsFilterable = true, IsSortable = true, IsFacetable = true)]
        public string DistributionTerms { get; set; }

        public int CreatedByUserId { get; set; }
        public bool Deleted { get; set; }

        
        public DateTimeOffset CreatedAt { get; set; }


        public DateTimeOffset? UpdatedAt { get; set; }


        public IEnumerable<Contribution> Contributions { get; set; }

        public OrganizationCampaign OrganizationCampaign { get; set; }
        public PeopleCampaign PeopleCampaign { get; set; }
    }
}
