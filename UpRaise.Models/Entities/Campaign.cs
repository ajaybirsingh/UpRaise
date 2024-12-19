using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

//https://www.markopapic.com/finding-nearby-users-using-ef-core-spatial-data/
namespace UpRaise.Entities
{
    [Table("Campaigns")]
    public class Campaign
    {
        public Campaign()
        {
            this.CampaignFiles = new HashSet<CampaignFile>();
            this.CampaignAnalytics = new HashSet<CampaignAnalytic>();
            this.CampaignFollowers = new HashSet<CampaignFollower>();
            this.Contributions = new HashSet<Contribution>();

            this.CampaignRedlineComments = new HashSet<CampaignRedlineComment>();
            this.CampaignRedlineEvents = new HashSet<CampaignRedlineEvent>();

            this.UserMessages = new HashSet<UserMessage>();
        }
        
        [Key]
        public int Id { get; set; }

        public Models.Enums.CampaignTypes TypeId { get; set; }

        public Models.Enums.CampaignStatuses StatusId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string Summary { get; set; }

        [MaxLength(1000)]
        public string DistributionTerms { get; set; }

        [MaxLength(256)]
        public string HeaderPictureURL { get; set; }

        [Column(TypeName = "decimal(16, 4)")]
        public Decimal? FundraisingGoals { get; set; }

        public Int16? FeatureScore { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? EndDate { get; set; }

        [MaxLength(36)]
        public string TransactionId { get; set; }

        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public int CreatedByUserId { get; set; }

        public bool AcceptedTermsAndConditions { get; set; }

        public NetTopologySuite.Geometries.Point GeoLocation { get; set; }

        public Models.Enums.Countries? GeoLocationCountryId { get; set; }

        public Models.Enums.Currencies CurrencyId { get; set; }

        [MaxLength(128)]
        public string SEOFriendlyURL { get; set; }

        [MaxLength(160)]
        public string GeoLocationAddress { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual IDUser User { get; set; }

        public virtual OrganizationCampaign OrganizationCampaign { get; set; }

        public virtual PeopleCampaign PeopleCampaign { get; set; }

        public virtual ICollection<CampaignFile> CampaignFiles { get; set; }
        public virtual ICollection<CampaignFollower> CampaignFollowers { get; set; }
        public virtual ICollection<CampaignAnalytic> CampaignAnalytics { get; set; }
        public virtual ICollection<CampaignRedlineComment> CampaignRedlineComments { get; set; }
        public virtual ICollection<CampaignRedlineEvent> CampaignRedlineEvents { get; set; }

        public virtual ICollection<Contribution> Contributions { get; set; }
        public virtual ICollection<UserMessage> UserMessages { get; set; }
    }
}