using EasyCaching.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Unsplasharp;
using UpRaise.DTOs.Redis;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Models.Enums;
using UpRaise.Services;
using UpRaise.Services.EF;
using Xunit;

namespace UpRaise.Tests
{
    public class CampaignUnitTests
    {
        private IServiceProvider _servicePrvider = null;

        public CampaignUnitTests()
        {
            var nlogger = NLog.LogManager.GetLogger("UpRaise.Tests");
            var host = UpRaise.Program.CreateHostBuilder(nlogger, null).Build();
            _servicePrvider = host.Services;
        }


        [Fact]
        public async Task<bool> FillSEOFriendlyURL()
        {
            try
            {
                var appDatabaseContext = _servicePrvider.GetService<AppDatabaseContext>();
                var cachingProvider = _servicePrvider.GetService<IEasyCachingProvider>();


                Campaign campaign = null;
                do
                {
                    campaign = await appDatabaseContext.Campaigns
                                    .FirstOrDefaultAsync(i => i.SEOFriendlyURL == null);

                    if (campaign != null && !string.IsNullOrWhiteSpace(campaign.Name))
                    {
                        var friendlyURl = UrlHelper.URLFriendly(campaign.Name, 120);

                        //campaign = await appDatabaseContext.Campaigns
                                        //.FirstOrDefaultAsync(i => i.SEOFriendlyURL == null);

                        var seoExists = await appDatabaseContext.Campaigns
                                            .AnyAsync(i => i.SEOFriendlyURL == friendlyURl);
                        if (!seoExists)
                        {
                            campaign.SEOFriendlyURL = friendlyURl;
                        }
                        else
                        {
                            var existingSEOs = appDatabaseContext.Campaigns
                                                .Where(i => EF.Functions.Like(i.SEOFriendlyURL, $"{friendlyURl}-%"));

                            int maxNumber = 0;
                            foreach (var existingSEO in existingSEOs)
                            {
                                var numberPart = existingSEO.SEOFriendlyURL.Substring(friendlyURl.Length + 1);

                                if (!numberPart.Contains('-'))
                                {
                                    int numericValue;
                                    bool isNumber = int.TryParse(numberPart, out numericValue);
                                    if (isNumber && numericValue > maxNumber)
                                        maxNumber = numericValue;

                                }
                            }

                            campaign.SEOFriendlyURL = $"{friendlyURl}-{maxNumber+1}";
                        }

                        int numChanges = await appDatabaseContext.SaveChangesAsync();
                    }

                } while (campaign != null);


                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.ToString());
            }

            return false;
        }

    }
}
