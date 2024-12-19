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
    public class RedisUnitTests
    {
        private IServiceProvider _servicePrvider = null;

        public RedisUnitTests()
        {
            var nlogger = NLog.LogManager.GetLogger("UpRaise.Tests");
            var host = UpRaise.Program.CreateHostBuilder(nlogger, null).Build();
            _servicePrvider = host.Services;
        }


        [Fact]
        public async Task<bool> ReseedSEOFriendlyURLs()
        {
            try
            {
                var appDatabaseContext = _servicePrvider.GetService<AppDatabaseContext>();
                var cachingProvider = _servicePrvider.GetService<IEasyCachingProvider>();

                //var oldValues = await cachingProvider.GetByPrefixAsync<SEOFriendlyURLDTO>(UpRaise.Helpers.Constants.Cache_SEO_Prefix);
                await cachingProvider.RemoveByPrefixAsync(UpRaise.Helpers.Constants.Cache_SEO_Prefix);


                var campaignDatas = appDatabaseContext
                            .Campaigns
                            .Select(i => new
                            {
                              i.SEOFriendlyURL,
                              i.Id,
                              i.TypeId,
                            })
                            .AsNoTracking()
                            .Where(i => i.SEOFriendlyURL != null);

                if (campaignDatas.Any())
                {
                    var SEOFriendlyURLDTOs = new Dictionary<string, SEOFriendlyURLDTO>();
                    
                    foreach (var campaignData in campaignDatas)
                    {
                        if (campaignData.SEOFriendlyURL.Any())
                        {
                            var key = $"{UpRaise.Helpers.Constants.Cache_SEO_Prefix}{campaignData.SEOFriendlyURL}";
                            SEOFriendlyURLDTOs[key] = new SEOFriendlyURLDTO() { Id = campaignData.Id, TypeId = (byte)campaignData.TypeId };
                        }
                    }

                    await cachingProvider.SetAllAsync(SEOFriendlyURLDTOs, TimeSpan.FromDays( 365 * 10 ));
                }

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
