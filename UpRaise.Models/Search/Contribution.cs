using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpRaise.Models.Enums;

namespace UpRaise.Models.Search
{
    public class Contribution
    {
        [SearchableField(IsFilterable = true)]
        public string Id { get; set; }


        [SearchableField(IsFilterable = true)]
        public string ContributionTypeId { get; set; }

        [SearchableField(IsFilterable = true)]
        public string UserId { get; set; }

        public double Amount { get; set; }
        

        public bool Deleted { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

    }
}
