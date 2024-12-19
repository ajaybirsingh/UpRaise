using System;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UpRaise.Models.Enums;

//
//https://github.com/aspnet/AspNetCore
//https://chsakell.com/2018/05/11/asp-net-core-identity-series-integrating-entity-framework/
//https://gist.github.com/akatakritos/96b0c3136f8498246fa810d393927f04
//https://stackoverflow.com/questions/29006589/how-can-i-change-asp-net-identity-2-on-sql-server-to-create-a-newsequentialid-pr
namespace UpRaise.Entities
{
    public class IDUser: IdentityUser<int>
    {
        public IDUser()
        {
            this.Campaigns = new HashSet<Campaign>();

            this.CampaignFiles = new HashSet<CampaignFile>();
            this.CampaignAnalytics = new HashSet<CampaignAnalytic>();
            this.CampaignFollowers = new HashSet<CampaignFollower>();

            this.CampaignRedlineComments = new HashSet<CampaignRedlineComment>();
            this.CampaignRedlineEvents = new HashSet<CampaignRedlineEvent>();

            this.Contributions = new HashSet<Contribution>();

            this.FromUserMessages = new HashSet<UserMessage>();
            this.ToUserMessages = new HashSet<UserMessage>();
        }

        public Guid AliasId { get; set; }

        [MaxLength(128)]
        public string FirstName { get; set; }

        [MaxLength(128)]
        public string LastName { get; set; }

        [MaxLength(256)]
        public string PictureURL { get; set; }

        //public string Token { get; set; }
        public bool Deleted { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public byte[] Version { get; set; }

        [MaxLength(128)]
        public string AddressStreet { get; set; }

        [MaxLength(64)]
        public string AddressCity { get; set; }

        [MaxLength(32)]
        public string AddressStateProvince { get; set; }

        [MaxLength(16)]
        public string AddressZipPostal { get; set; }

        [MaxLength(32)]
        public string AddressCountry { get; set; }

        public bool NotificationOnCampaignDonations { get; set; }
        public bool NotificationOnCampaignFollows { get; set; }
        public bool NotificationOnUpraiseEvents { get; set; }

        public NetTopologySuite.Geometries.Point GeoLocation { get; set; }

        public Currencies DefaultCurrencyId { get; set; }


        //
        //vice president, board of directors, default
        //
        //[ForeignKey("ClaimRoleId")]
        //public virtual ClaimRole ClaimRole { get; set; }

        //public virtual ICollection<Claim> Claims { get; set; }


        public virtual ICollection<Campaign> Campaigns { get; set; }

        public virtual ICollection<CampaignFile> CampaignFiles { get; set; }
        public virtual ICollection<CampaignAnalytic> CampaignAnalytics { get; set; }
        public virtual ICollection<CampaignFollower> CampaignFollowers { get; set; }
        public virtual ICollection<Contribution> Contributions { get; set; }

        public virtual ICollection<CampaignRedlineComment> CampaignRedlineComments { get; set; }
        public virtual ICollection<CampaignRedlineEvent> CampaignRedlineEvents { get; set; }

        public virtual ICollection<UserMessage> FromUserMessages { get; set; }
        public virtual ICollection<UserMessage> ToUserMessages { get; set; }

        public virtual NewsletterSubscription NewsletterSubscription { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
            /*
        {
            get
            {
                var fullName = $"{FirstName} {LastName}";
                return fullName;
            }
        }
            */

 
        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, Guid> manager)
        //{
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            //var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            //return userIdentity;
        //}

    }
}