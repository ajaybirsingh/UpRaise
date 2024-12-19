using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UpRaise.Entities
{
    [Table("BeneficiaryOrganizations")]
    public class BeneficiaryOrganization
    {
        public BeneficiaryOrganization()
        {
            this.OrganizationCampaigns = new HashSet<OrganizationCampaign>();
        }

        [Key]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        [MaxLength(8)]
        public string Language { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public Decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public Decimal? Longitude { get; set; }

        [MaxLength(256)]
        public string PictureURL { get; set; }

        [MaxLength(32)]
        public string PhoneNumber { get; set; }

        [MaxLength(256)]
        public string EmailAddress { get; set; }

        [MaxLength(128)]
        public string AddressStreet { get; set; }

        [MaxLength(128)]
        public string AddressCity { get; set; }

        [MaxLength(32)]
        public string AddressZipPostalCode { get; set; }

        [MaxLength(128)]
        public string AddressStateProvinceCounty { get; set; }

        [MaxLength(128)]
        public string AddressCountry { get; set; }

        [MaxLength(256)]
        public string Website { get; set; }

        [MaxLength(256)]
        public string MainContact { get; set; }

        [MaxLength(32)]
        public string MainContactPhoneNumber { get; set; }

        [MaxLength(32)]
        public string MainContactEmailAddress { get; set; }

        [MaxLength(32)]
        public string TimeZone { get; set; }

        [MaxLength(256)]
        public string EmailDomain { get; set; }


        public bool Deleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public virtual ICollection<OrganizationCampaign> OrganizationCampaigns { get; set; }
    }
}