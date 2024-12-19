using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UpRaise.Entities;
using UpRaise.Helpers;

namespace UpRaise.Services.EF
{
    public class AppDatabaseContext : IdentityDbContext<IDUser, IDRole, int, IDUserClaim, IDUserRole, IDUserLogin, IDRoleClaim, IDUserToken> //
    {
        private readonly ILogger<AppDatabaseContext> _logger;


        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options)
        {
            _logger = this.GetService<ILogger<AppDatabaseContext>>();
        }

        
        public DbSet<Campaign> Campaigns { get; set; }

        public DbSet<BeneficiaryOrganization> BeneficiaryOrganizations { get; set; }

        public DbSet<OrganizationCampaign> OrganizationCampaigns { get; set; }

        public DbSet<PeopleCampaign> PeopleCampaigns { get; set; }

        public DbSet<CampaignFile> CampaignFiles { get; set; }

        public DbSet<UserMessage> UserMessages { get; set; }

        public DbSet<CampaignAnalytic> CampaignAnalytics { get; set; }
        public DbSet<CampaignFollower> CampaignFollowers { get; set; }

        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }

        public DbSet<Contribution> Contributions { get; set; }

        public DbSet<CampaignRedlineComment> CampaignRedlineComments { get; set; }
        public DbSet<CampaignRedlineEvent> CampaignRedlineEvents { get; set; }


        public async Task<int> SaveChangesAsyncWithValidation(CancellationToken ct = default(CancellationToken))
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                var entities = from e in ChangeTracker.Entries()
                               where e.State == EntityState.Added
                                   || e.State == EntityState.Modified
                               select e.Entity;

                bool validationErrors = false;

                foreach (var entity in entities)
                {
                    try
                    {
                        var validationContext = new ValidationContext(entity);
                        Validator.ValidateObject(entity, validationContext, true);
                    }
                    catch (ValidationException validationException)
                    {
                        wrappedLogger.LogError(validationException);

                        validationErrors = true;
                    }
                    catch (Exception ex)
                    {
                        wrappedLogger.LogError(ex);
                    }
                }

                if (validationErrors)
                    throw new Exception("Errors validating objects");

                return await base.SaveChangesAsync(ct);
            }
        }

        public new async Task<int?> SaveChangesAsync(bool throwException, CancellationToken ct = default(CancellationToken))
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                if (throwException)
                    return await base.SaveChangesAsync(ct);
                else
                {
                    try
                    {
                        return await base.SaveChangesAsync(ct);
                    }
                    catch (Exception ex)
                    {
                        wrappedLogger.LogError(ex.Message);
                    }

                    return null;
                }
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Write Fluent API configurations here

            //Property Configurations
            //modelBuilder.Entity<Claim>(e =>
            //{
            //e.HasOne(r => r.User).WithMany(u => u.ClaimUsers).HasForeignKey(r => r.AccountManagerId);
            //e.HasOne(r => r.ApprovedByUser).WithMany(u => u.ApprovedByUsers).HasForeignKey(r => r.SalesManagerId);
            //});

            //modelBuilder.Entity<OrganizationCampaign>(b =>
            //{
            //b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
            //b.Property(r => r.Version).IsRowVersion();

            //b.HasOne(t => t.BeneficiaryOrganization)
            //.WithMany(a => a.OrganizationCampaigns)
            //.HasForeignKey(t => t.BeneficiaryOrganizationId);

            //b.HasOne(t => t.User)
            //.WithMany(a => a.OrganizationCampaigns)
            //.HasForeignKey(t => t.CreatedByUserId);

            //});


            //modelBuilder.Entity<BeneficiaryOrganization>(b =>
            //{
            //b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
            //b.Property(r => r.Version).IsRowVersion();
            //});
            /*
            modelBuilder.Entity<Campaign>()
                .HasOne(b => b.OrganizationCampaign)
                .WithOne(i => i.Campaign)
                .HasForeignKey<OrganizationCampaign>(b => b.CampaignId);

            modelBuilder.Entity<Campaign>()
                .HasOne(b => b.PeopleCampaign)
                .WithOne(i => i.Campaign)
                .HasForeignKey<PeopleCampaign>(b => b.CampaignId);
            */

            modelBuilder.Entity<UserMessage>()
                .HasOne(m => m.FromUser)
                .WithMany(i => i.FromUserMessages)
                .HasForeignKey(i => i.FromUserId);

            modelBuilder.Entity<UserMessage>()
                .HasOne(m => m.ToUser)
                .WithMany(i => i.ToUserMessages)
                .HasForeignKey(i => i.ToUserId);



            modelBuilder.Entity<IDRole>(b =>
            {
                b.ToTable("IdentityRoles");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


            modelBuilder.Entity<IDRoleClaim>(b =>
            {
                b.ToTable("IdentityRoleClaims");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


            modelBuilder.Entity<IDUser>(b =>
            {
                b.ToTable("IdentityUsers");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


            modelBuilder.Entity<IDUserClaim>(b =>
            {
                b.ToTable("IdentityUserClaims");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


            modelBuilder.Entity<IDUserLogin>(b =>
            {
                b.ToTable("IdentityUserLogins");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


            modelBuilder.Entity<IDUserRole>(b =>
            {
                b.ToTable("IdentityUserRoles");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


            modelBuilder.Entity<IDUserToken>(b =>
            {
                b.ToTable("IdentityUserTokens");

                b.Property(r => r.CreatedAt).ValueGeneratedOnAdd();
                b.Property(r => r.Version).IsRowVersion();
            });


        }

    }

}