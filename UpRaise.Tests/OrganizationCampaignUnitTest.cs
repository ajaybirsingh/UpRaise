using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unsplasharp;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Models.Enums;
using UpRaise.Services;
using UpRaise.Services.EF;
using Xunit;

namespace UpRaise.Tests
{
    public class OrganizationCampaignUnitTest
    {
        private IServiceProvider _servicePrvider = null;
        private Random _rnd = new Random(DateTimeOffset.Now.Millisecond);
        private HttpClient _httpClient = new HttpClient();

        private string imagesPath = @"c:\temp\images\";
        private DateTimeOffset unsplashStart = DateTimeOffset.Now;

        public OrganizationCampaignUnitTest()
        {
            var nlogger = NLog.LogManager.GetLogger("UpRaise.Tests");
            var host = UpRaise.Program.CreateHostBuilder(nlogger, null).Build();
            _servicePrvider = host.Services;
        }


        [Fact]
        public async Task<bool> RecreateSearchServiceIndexAsync()
        {
            try
            {
                var searchService = _servicePrvider.GetService<SearchService>();
                if (searchService == null)
                    return false;

                var recreateIndex = await searchService.RecreateIndexAsync(SearchService._campaignIndexName, typeof(UpRaise.Models.Search.Campaign));
                return recreateIndex;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

            return false;
        }


        [Fact]
        public async Task<bool> DeleteAllDataAsync()
        {
            using (var dbContext = _servicePrvider.GetService<AppDatabaseContext>())
            {
                try
                {
                    int rows = 0;

                    rows = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.CampaignAnalytics");
                    rows = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.CampaignFiles");
                    rows = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.CampaignFollowers");
                    rows = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.CampaignUpdateFiles");
                    rows = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.CampaignUpdates");
                    rows = await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM dbo.Campaigns");


                    var searchService = _servicePrvider.GetService<SearchService>();
                    var recreateIndex = await searchService.RecreateIndexAsync(SearchService._campaignIndexName, typeof(UpRaise.Models.Search.Campaign));

                    return true;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.ToString());
                }
            }

            return false;
        }


        [Fact]
        public async Task GenerateOrganizationCampaignsAsync()
        {
            int maxNumberOfCampaignPictures = 4;
            int maxNumberOfCampaignContributions = 15;

            var searchService = _servicePrvider.GetService<SearchService>();
            var blobManager = _servicePrvider.GetService<IBlobManager>();

            using (var dbContext = _servicePrvider.GetService<AppDatabaseContext>())
            {
                try
                {
                    var userIds = await GetUserIdsAsync(dbContext);
                    var beneficiaryOrganizationIds = await GetBeneficiaryIdsAsync(dbContext);

                    var campaignTypes = Enum.GetValues(typeof(CampaignTypes));
                    var organizationCampaignCategories = Enum.GetValues(typeof(OrganizationCampaignCategories));

                    var startDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, 0, TimeSpan.FromSeconds(0));


                    int numberOfCampaigns = 0;

                    unsplashStart = DateTimeOffset.Now;
                    while (true)
                    {
                        System.Console.WriteLine($"Creating campaign #{++numberOfCampaigns}.");

                        var campaign = new Campaign();

                        campaign.TypeId = GetRandomBoolean(0.5) ? CampaignTypes.Organization : CampaignTypes.People;
                        campaign.StatusId = GetRandomBoolean(0.10) ? CampaignStatuses.Active : CampaignStatuses.Disabled;  //10% chance to be false

                        campaign.Name = Faker.Lorem.Sentence().Left(_rnd.Next(10, 100));
                        campaign.Summary = Faker.Lorem.Paragraph().Left(_rnd.Next(25, 128));

                        //[MaxLength(1000)]
                        //public string DistributionTerms { get; set; }


                        campaign.Deleted = !GetRandomBoolean(0.10); //10% chance to be false
                        campaign.CreatedAt = GetRandomDate(startDate, DateTimeOffset.Now);
                        campaign.UpdatedAt = GetRandomDate(campaign.CreatedAt, DateTimeOffset.Now);
                        campaign.CreatedByUserId = userIds[_rnd.Next(userIds.Count)];
                        campaign.TransactionId = Guid.NewGuid().ToString().ToLower();

                        campaign.StartDate = GetRandomDate(startDate, DateTimeOffset.Now);
                        campaign.EndDate = GetRandomDate(campaign.StartDate.Value, DateTimeOffset.Now.AddYears(2));

                        campaign.FundraisingGoals = (Decimal)(_rnd.NextDouble() * 1000000.0);
                        campaign.FeatureScore = GetRandomBoolean(0.9) ? 1 : null;



                        List<string> photos = null;

                        switch (campaign.TypeId)
                        {
                            case CampaignTypes.Organization:
                                {
                                    campaign.OrganizationCampaign = new OrganizationCampaign();
                                    campaign.OrganizationCampaign.CategoryId = (OrganizationCampaignCategories)organizationCampaignCategories.GetValue(_rnd.Next(organizationCampaignCategories.Length));
                                    campaign.OrganizationCampaign.BeneficiaryOrganizationId = beneficiaryOrganizationIds[_rnd.Next(beneficiaryOrganizationIds.Count)];
                                    campaign.OrganizationCampaign.Location = $"{Faker.Address.City()}, {Faker.Address.UsState()}";
                                    campaign.OrganizationCampaign.ContactName = Faker.Name.FullName();
                                    campaign.OrganizationCampaign.ContactEmail = Faker.Internet.Email(campaign.OrganizationCampaign.ContactName);
                                    campaign.OrganizationCampaign.ContactPhone = Faker.Phone.Number();
                                    campaign.OrganizationCampaign.BeneficiaryMessage = Faker.Lorem.Paragraph().Left(_rnd.Next(1000));
                                    campaign.OrganizationCampaign.UpdatedAt = GetRandomDate(campaign.CreatedAt, DateTimeOffset.Now);

                                    var numberOfPhotos = _rnd.Next(maxNumberOfCampaignPictures);

                                    var description = campaign.OrganizationCampaign.CategoryId.GetDescription();
                                    photos = await DownloadUnsplashImages(description, numberOfPhotos);
                                }
                                break;

                            case CampaignTypes.People:
                                {
                                    campaign.PeopleCampaign = new PeopleCampaign();
                                    campaign.PeopleCampaign.BeneficiaryName = Faker.Name.FullName();
                                    campaign.PeopleCampaign.BeneficiaryEmail = Faker.Internet.Email(campaign.PeopleCampaign.BeneficiaryName);
                                    campaign.PeopleCampaign.BeneficiaryMessage = Faker.Lorem.Paragraph().Left(_rnd.Next(1000));
                                    campaign.PeopleCampaign.UpdatedAt = GetRandomDate(campaign.CreatedAt, DateTimeOffset.Now);

                                    var numberOfPhotos = _rnd.Next(maxNumberOfCampaignPictures);
                                    photos = await DownloadUnsplashImages("peopl help", numberOfPhotos);
                                }
                                break;
                        }



                        short photoIdx = 0;
                        foreach (var photo in photos)
                        {
                            var campaignFile = new CampaignFile();

                            campaignFile.Filename = $"{Guid.NewGuid().ToString()}.png";
                            campaignFile.TypeId = CampaignFileTypes.Photo;
                            campaignFile.SizeInBytes = (int)(new FileInfo(photo)).Length;

                            campaignFile.SortOrder = photoIdx++;
                            campaignFile.UID = Guid.NewGuid().ToString().ToLower();

                            campaignFile.Deleted = !GetRandomBoolean(0.1);

                            campaignFile.CreatedAt = GetRandomDate(campaign.StartDate.Value, DateTimeOffset.Now);

                            campaignFile.UpdatedAt = GetRandomDate(campaignFile.CreatedAt, DateTimeOffset.Now);

                            campaignFile.CreatedByUserId = campaign.CreatedByUserId;


                            using (var fileStream = new FileStream(photo, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                            {
                                var tags = new Dictionary<string, string>
                                         {
                                          { "TransactionId", campaign.TransactionId },
                                          { "Status", "ok" },
                                          { "UID", "" },
                                          { "CreatedAt", $"{DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")}" },
                                         };

                                var fileName = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, campaignFile.UID, campaignFile.Filename);
                                var blockBlob = await blobManager.UploadFromStreamAsync(ContainerNames.Public, fileName, fileStream, contentDisposition: campaignFile.Filename, "image/png", tags);
                            }

                            campaign.CampaignFiles.Add(campaignFile);
                        }




                        //
                        //create contributions
                        //
                        var contributionTypes = Enum.GetValues(typeof(ContributionTypes));
                        var numberOfContributions = _rnd.Next(maxNumberOfCampaignContributions);
                        for (int j = 0; j < numberOfContributions; j++)
                        {
                            var contribution = new Contribution();

                            contribution.UserId = userIds[_rnd.Next(userIds.Count)];

                            contribution.Amount = (decimal)_rnd.NextDouble() * campaign.FundraisingGoals.Value // percentage of total goal
                                        * (decimal)0.15;//no contribution more than 15% of total goal
                            contribution.Note = Faker.Lorem.Paragraph().Left(_rnd.Next(25, 1000));

                            contribution.ContributionTypeId = (ContributionTypes)contributionTypes.GetValue(_rnd.Next(contributionTypes.Length)); ;

                            contribution.Anonymous = GetRandomBoolean(0.7);

                            contribution.Deleted = !GetRandomBoolean(0.1);

                            contribution.CreatedAt = GetRandomDate(campaign.StartDate.Value, DateTimeOffset.Now);
                            contribution.UpdatedAt = GetRandomDate(contribution.CreatedAt, DateTimeOffset.Now);

                            campaign.Contributions.Add(contribution);
                        }

                        await dbContext.AddAsync(campaign);
                        int numChanges = await dbContext.SaveChangesAsync();

                        await searchService.UpsertCampaignAsync(campaign);
                    }

                }
                catch (Exception ex)
                {
                }
            }
        }


        private async Task<List<int>> GetUserIdsAsync(AppDatabaseContext dbContext)
        {
            var userIds = await dbContext
                                .Users
                                .AsNoTracking()
                                .Select(i => i.Id)
                                .ToListAsync();

            return userIds;
        }

        private async Task<List<int>> GetBeneficiaryIdsAsync(AppDatabaseContext dbContext)
        {
            var userIds = await dbContext
                                .BeneficiaryOrganizations
                                .AsNoTracking()
                                .Select(i => i.Id)
                                .ToListAsync();

            return userIds;
        }

        private DateTimeOffset GetRandomDate(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, _rnd.Next(0, (int)timeSpan.TotalMinutes), 0);
            var newDate = startDate + newSpan;

            return newDate;
        }

        private bool GetRandomBoolean(double probFalse)
        {
            var prob = _rnd.NextDouble();
            var val = prob > probFalse;
            return val;
        }


        private async Task<bool> DelayUnsplashForRateLimitingAsync(UnsplasharpClient client)
        {
            if (client.RateLimitRemaining == 0)
            {
                var timePassedSinceStartFetch = DateTimeOffset.Now - unsplashStart;
                var numberOfMinutesToWait = 60 - timePassedSinceStartFetch.TotalMinutes;
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(numberOfMinutesToWait + 1));
                unsplashStart = DateTimeOffset.Now;

                return true;
            }

            return false;
        }

        private async Task<List<string>> DownloadUnsplashImages(string description, int numImages)
        {
            var client = new Unsplasharp.UnsplasharpClient("WkGrhhLwF_c9n0g3RvmKsT_CNnyvnyHmmwIxkEC1x4o", "1fVUISuU2FbTp5SAaoHWJKLdHVG1m795g7q_YsoypUM");
            var files = new List<string>();

            do
            {
                //var photosFound = await client.SearchPhotos(description, page: 1, perPage : numImages);
                var photosFound = await client.GetRandomPhoto(numImages, query: description);
                await DelayUnsplashForRateLimitingAsync(client);

                numImages = Math.Min(numImages, client.RateLimitRemaining);
                foreach (var photo in photosFound)
                {
                    var filename = System.IO.Path.GetTempFileName();

                    var bytes = await _httpClient.GetByteArrayAsync(photo.Urls.Regular);
                    await System.IO.File.WriteAllBytesAsync(filename, bytes);

                    files.Add(filename);

                    if (files.Count >= numImages)
                        break;
                }

            } while (files.Count < numImages);

            return files;
        }



    }
}
