using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Models.Enums;
using UpRaise.Services;
using UpRaise.Services.EF;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace UpRaise.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/public")]
    public class PublicController : Controller
    {
        private readonly string _cachePopularCampaigns = "popular_campaigns";
        private readonly string _cacheLocalCampaigns = "local_campaigns";
        private readonly string _cacheClosestLocation = "closest_location";

        private readonly IHttpClientFactory _clientFactory;

        private readonly ILogger<PublicController> _logger;
        private readonly IMapper _mapper;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly Random _random = new Random(Guid.NewGuid().GetHashCode());
        private readonly IBlobManager _blobManager;
        private readonly EmailHelper _emailHelper;
        private readonly IDistributedCache _cache;
        private readonly IpStack.IpStackClient _ipStackClient = null;


        public PublicController(
            ILogger<PublicController> logger,
            IMapper mapper,
            AppDatabaseContext appDatabaseContext,
            IBlobManager blobManager,
            EmailHelper emailHelper,
            IDistributedCache cache,
            IHttpClientFactory clientFactory
            )
        {
            _logger = logger;
            _mapper = mapper;
            _appDatabaseContext = appDatabaseContext;
            _blobManager = blobManager;
            _emailHelper = emailHelper;
            _cache = cache;

            _ipStackClient = new IpStack.IpStackClient("48634c7dc8bf40ae9ca4404e8c2bfc8a", false);
            _clientFactory = clientFactory;
        }


        [HttpPost("campaigns")]
        public async Task<IActionResult> Campaigns([FromBody] PublicCampaignRequestDTO publicCampaignRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        var publicCampaignResponseDTOs = new List<PublicCampaignResponseDTO>();

                        var qry = _appDatabaseContext.Campaigns
                                    .Include(i => i.CampaignFiles)
                                    .Include(i => i.Contributions)
                                    .Select(i => new
                                    {
                                        i.Id,
                                        i.StatusId,
                                        i.EndDate,
                                        i.TypeId,
                                        i.OrganizationCampaign,
                                        i.Name,
                                        i.Summary,
                                        i.CreatedAt,
                                        i.HeaderPictureURL,
                                        i.UpdatedAt,
                                        i.FundraisingGoals,
                                        i.FeatureScore,
                                        i.Contributions,
                                        i.CampaignFiles,
                                        i.TransactionId,
                                        i.GeoLocation,
                                        i.SEOFriendlyURL,
                                        User = (i.User != null) ? new { PictureURL = i.User.PictureURL, FirstName = i.User.FirstName, LastName = i.User.LastName, AliasId = i.User.AliasId } : null
                                    })
                                    .AsSplitQuery()
                                    .AsNoTracking()
                                    //.Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                    .Where(i => i.StatusId == CampaignStatuses.Active);

                        if (publicCampaignRequestDTO.FilterHideCompleted)
                            qry = qry.Where(i => !i.EndDate.HasValue || i.EndDate > DateTimeOffset.UtcNow);

                        if (publicCampaignRequestDTO.FilterCampaignType != CampaignTypes.Any)
                            qry = qry.Where(i => i.TypeId == publicCampaignRequestDTO.FilterCampaignType);

                        if (publicCampaignRequestDTO.FilterCampaignType == CampaignTypes.Organization &&
                            publicCampaignRequestDTO.FilterCategoryId != 0)
                            qry = qry.Where(i => i.OrganizationCampaign.CategoryId == (OrganizationCampaignCategories)publicCampaignRequestDTO.FilterCategoryId);

                        if (!string.IsNullOrWhiteSpace(publicCampaignRequestDTO.FilterTitleOrDescription))
                        {
                            qry = qry.Where(i => Microsoft.EntityFrameworkCore.EF.Functions.Like(i.Name, $"%{publicCampaignRequestDTO.FilterTitleOrDescription}%") ||
                                                 Microsoft.EntityFrameworkCore.EF.Functions.Like(i.Summary, $"%{publicCampaignRequestDTO.FilterTitleOrDescription}%"));
                        }

                        if (publicCampaignRequestDTO.ExploreViewType == ExploreViewTypes.Map)
                        {
                            if (publicCampaignRequestDTO.MapBounds != null &&
                                publicCampaignRequestDTO.MapBounds.Count() == 4)
                            {

                                var polygonCoords = new Coordinate[]
                                {
                                        new Coordinate(publicCampaignRequestDTO.MapBounds[3], publicCampaignRequestDTO.MapBounds[2]), //NE
                                        new Coordinate(publicCampaignRequestDTO.MapBounds[1], publicCampaignRequestDTO.MapBounds[2]), //NW
                                        new Coordinate(publicCampaignRequestDTO.MapBounds[1], publicCampaignRequestDTO.MapBounds[0]), //SW
                                        new Coordinate(publicCampaignRequestDTO.MapBounds[3], publicCampaignRequestDTO.MapBounds[0]), //SE

                                        new Coordinate(publicCampaignRequestDTO.MapBounds[3], publicCampaignRequestDTO.MapBounds[2]), //NE

                                };

                                var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                                var polygon = geometryFactory.CreatePolygon(polygonCoords);
                                var isValid = polygon.IsValid;

                                //var polygon = NetTopologySuite.Geometries.Polygon.DefaultFactory.CreatePolygon(polygonCoords);
                                qry = qry.Where(i => i.GeoLocation != null &&
                                                    i.GeoLocation.Within(polygon)
                                                 );
                            }
                            //else
                            //qry = qry.Where(i => i.GeoLocation != null);
                        }


                        qry = qry
                                .OrderByDescending(i => i.CreatedAt)
                                .Take(publicCampaignRequestDTO.NumberOfCampaigns);

                        var campaigns = await qry.ToListAsync();


                        foreach (var campaign in campaigns)
                        {
                            try
                            {
                                var publicCampaignResponseDTO = new PublicCampaignResponseDTO();

                                publicCampaignResponseDTO.Id = campaign.Id;
                                publicCampaignResponseDTO.TypeId = campaign.TypeId;
                                publicCampaignResponseDTO.SEOFriendlyURL = campaign.SEOFriendlyURL;

                                if (campaign.TypeId == CampaignTypes.Organization)
                                {
                                    var organizationCampaign = await _appDatabaseContext.OrganizationCampaigns
                                    .AsNoTracking()
                                    .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                    .Select(i => new { i.CampaignId, i.CategoryId })
                                    .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                    publicCampaignResponseDTO.CategoryId = organizationCampaign.CategoryId;
                                }

                                publicCampaignResponseDTO.Name = campaign.Name;

                                publicCampaignResponseDTO.Summary = campaign.Summary.LeftTrim(50, true);


                                switch (campaign.TypeId)
                                {
                                    case CampaignTypes.Organization:
                                        {
                                            var organizationCampaign = await _appDatabaseContext
                                                              .OrganizationCampaigns
                                                              .Include(i => i.BeneficiaryOrganization)
                                                              .AsNoTracking()
                                                              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                              .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                            publicCampaignResponseDTO.Category = $"Organization - {organizationCampaign.CategoryId.GetDescription()}";

                                            if (organizationCampaign.BeneficiaryOrganization != null)
                                                publicCampaignResponseDTO.Beneficiary = $"{organizationCampaign.BeneficiaryOrganization.Name}";
                                        }
                                        break;

                                    case CampaignTypes.People:
                                        {
                                            var peopleCampaign = await _appDatabaseContext
                                                              .PeopleCampaigns
                                                              .AsNoTracking()
                                                              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                              .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                            publicCampaignResponseDTO.Category = $"Beneficiary";

                                            publicCampaignResponseDTO.Beneficiary = $"{peopleCampaign.BeneficiaryName}";
                                        }
                                        break;
                                }


                                //if (campaign.BeneficiaryOrganization != null)
                                //{
                                //publicCampaignResponseDTO.BeneficiaryOrganizationName = campaign.BeneficiaryOrganization.Name;
                                //publicCampaignResponseDTO.Location = $"{campaign.BeneficiaryOrganization.AddressCity}, {campaign.BeneficiaryOrganization.AddressStateProvinceCounty}";
                                //}
                                publicCampaignResponseDTO.CreatedAt = campaign.CreatedAt;

                                if (campaign.GeoLocation != null)
                                {
                                    publicCampaignResponseDTO.Longitude = campaign.GeoLocation.X;
                                    publicCampaignResponseDTO.Latitude = campaign.GeoLocation.Y;
                                }

                                if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                                    publicCampaignResponseDTO.HeaderPictureURL = $"{BlobPathHelper.DomainPrefix}/public-md/{campaign.HeaderPictureURL}";

                                if (string.IsNullOrWhiteSpace(publicCampaignResponseDTO.HeaderPictureURL))
                                    publicCampaignResponseDTO.HeaderPictureURL = $"/assets/images/campaigns/default_{(campaign.TypeId == CampaignTypes.Organization ? "organization" : "people")}.jpg";


                                publicCampaignResponseDTO.UpdatedAt = campaign.UpdatedAt;


                                publicCampaignResponseDTO.FundraisingGoals = campaign.FundraisingGoals;

                                publicCampaignResponseDTO.Featured = campaign.FeatureScore.HasValue && campaign.FeatureScore.Value > 0;

                                if (campaign.EndDate.HasValue)
                                {
                                    publicCampaignResponseDTO.Completed = campaign.EndDate < DateTimeOffset.Now;

                                    if (!publicCampaignResponseDTO.Completed)
                                        publicCampaignResponseDTO.DaysLeft = (campaign.EndDate.Value - DateTimeOffset.UtcNow).Days;
                                }


                                if (campaign.Contributions.Any())
                                {
                                    var numberOfDonors = await _appDatabaseContext
                                       .Contributions
                                       .AsNoTracking()
                                       .Select(i => new
                                       {
                                           i.CampaignId,
                                           i.UserId
                                       })
                                       .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                       .Where(i => i.CampaignId == campaign.Id)
                                       .Distinct()
                                       .CountAsync();

                                    publicCampaignResponseDTO.NumberOfDonors = numberOfDonors;


                                    publicCampaignResponseDTO.FundedPreviously = true;

                                    var totalAmount = campaign.Contributions.Sum(i => i.Amount);
                                    publicCampaignResponseDTO.FundedPercentage = (int)((totalAmount / publicCampaignResponseDTO.FundraisingGoals) * 100);

                                    publicCampaignResponseDTO.FundingAmountToDate = totalAmount;

                                    publicCampaignResponseDTO.FundingLastDateTime = campaign.Contributions.Select(i => i.CreatedAt).Max();
                                }

                                publicCampaignResponseDTO.Photos = campaign.CampaignFiles.Where(i => i.TypeId == CampaignFileTypes.Photo)
                                                                                .Select(cFile => $"{BlobPathHelper.DomainPrefix}/public-md/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}");

                                publicCampaignResponseDTO.Videos = campaign.CampaignFiles.Where(i => i.TypeId == CampaignFileTypes.Video)
                                                                                .Select(cFile => $"{BlobPathHelper.DomainPrefix}/public-md/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}");

                                if (campaign.User != null)
                                {
                                    publicCampaignResponseDTO.OrganizedBy = $"{campaign.User.FirstName} {campaign.User.LastName}".Trim();

                                    if (!string.IsNullOrWhiteSpace(campaign.User.PictureURL))
                                        publicCampaignResponseDTO.OrganizedByProfilePictureURL = $"https://app.upraise.fund/api/file/getprofilepicture/{campaign.User.AliasId}";
                                }


                                publicCampaignResponseDTOs.Add(publicCampaignResponseDTO);
                            }
                            catch (Exception ex)
                            {
                                wrappedLogger.LogError($"Unable to fetch Organization Campaign Id {campaign.Id} {ex.Message}");
                            }

                        }

                        return Ok(publicCampaignResponseDTOs);
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to fetch campaigns."));

            }
        }

        [HttpGet("campaignByFriendlyURL")]
        public async Task<IActionResult> CampaignByFriendlyURL(string campaignFriendlyUrl)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var publicCampaignDetailResponseDTO = await GetCampaignAsync(null, campaignFriendlyUrl);
                    return Ok(publicCampaignDetailResponseDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get campaign."));

            }
        }


        private async Task<PublicCampaignDetailResponseDTO> GetCampaignAsync(int? id, string friendlyURL)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var publicCampaignDetailResponseDTO = new PublicCampaignDetailResponseDTO();

                    var qry = _appDatabaseContext.Campaigns
                            .Include(i => i.User)
                            .Include(i => i.CampaignFiles)
                            .Include(i => i.Contributions)
                            .AsSplitQuery()
                            .AsNoTracking()
                            .Select(i => new Campaign()
                            {
                                Id = i.Id,
                                HeaderPictureURL = i.HeaderPictureURL,
                                Name = i.Name,
                                TypeId = i.TypeId,
                                CreatedAt = i.CreatedAt,
                                UpdatedAt = i.UpdatedAt,
                                FundraisingGoals = i.FundraisingGoals,
                                FeatureScore = i.FeatureScore,
                                TransactionId = i.TransactionId,
                                CreatedByUserId = i.CreatedByUserId,
                                StartDate = i.StartDate,
                                EndDate = i.EndDate,
                                CampaignFiles = i.CampaignFiles,
                                Contributions = i.Contributions,
                                GeoLocationAddress = i.GeoLocationAddress,
                                SEOFriendlyURL = i.SEOFriendlyURL,
                                User = (i.User != null) ? new IDUser() { Id = i.User.Id, FirstName = i.User.FirstName, LastName = i.User.LastName, AliasId = i.User.AliasId, PictureURL = i.User.PictureURL } : null
                            })
                            .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5));

                    Campaign campaign = null;
                    if (id.HasValue)
                        campaign = await qry.FirstOrDefaultAsync(i => i.Id == id.Value);
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(friendlyURL))
                            campaign = await qry.FirstOrDefaultAsync(i => i.SEOFriendlyURL == friendlyURL);
                        else
                            return null;
                    }

                    if (campaign == null)
                    {
                        wrappedLogger.LogError($"Could not find campaign");
                        return null;
                    }

                    publicCampaignDetailResponseDTO.Id = campaign.Id;
                    publicCampaignDetailResponseDTO.TransactionId = campaign.TransactionId;
                    publicCampaignDetailResponseDTO.Type = CampaignTypes.Organization;

                    publicCampaignDetailResponseDTO.CreatedByUserFullName = $"{campaign.User.FirstName} {campaign.User.LastName}".Trim();

                    if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                        publicCampaignDetailResponseDTO.HeaderPictureURL = $"{BlobPathHelper.DomainPrefix}/public-md/{campaign.HeaderPictureURL}";

                    if (string.IsNullOrWhiteSpace(publicCampaignDetailResponseDTO.HeaderPictureURL))
                        publicCampaignDetailResponseDTO.HeaderPictureURL = $"/assets/images/campaigns/default_{(campaign.TypeId == CampaignTypes.Organization ? "organization" : "people")}.jpg";



                    publicCampaignDetailResponseDTO.CreatedByUserAliasId = campaign.User.AliasId;

                    publicCampaignDetailResponseDTO.SEOFriendlyURL = campaign.SEOFriendlyURL;

                    publicCampaignDetailResponseDTO.ShowProfilePic = !string.IsNullOrWhiteSpace(campaign.User.PictureURL);

                    //publicCampaignDetailResponseDTO.Category = campaign.CategoryId.GetDescription();

                    publicCampaignDetailResponseDTO.Name = campaign.Name;

                    var descriptionBlobPath = BlobPathHelper.GetCampaignDescriptionFilename(campaign.TypeId, campaign.Id, null);
                    publicCampaignDetailResponseDTO.Description = await _blobManager.ReadBlobStringAsync(ContainerNames.Data, descriptionBlobPath);

                    //if (organizationCampaign.BeneficiaryOrganization != null)
                    //{
                    //publicCampaignDetailResponseDTO.BeneficiaryOrganizationName = organizationCampaign.BeneficiaryOrganization.Name;
                    //publicCampaignDetailResponseDTO.Location = $"{organizationCampaign.BeneficiaryOrganization.AddressCity}, {organizationCampaign.BeneficiaryOrganization.AddressStateProvinceCounty}";
                    //}
                    publicCampaignDetailResponseDTO.CreatedAt = campaign.CreatedAt;

                    publicCampaignDetailResponseDTO.UpdatedAt = campaign.UpdatedAt;

                    publicCampaignDetailResponseDTO.Location = campaign.GeoLocationAddress;


                    publicCampaignDetailResponseDTO.FundraisingGoals = campaign.FundraisingGoals;

                    publicCampaignDetailResponseDTO.Featured = campaign.FeatureScore.HasValue && campaign.FeatureScore.Value > 0;

                    if (campaign.EndDate.HasValue)
                    {
                        publicCampaignDetailResponseDTO.Completed = campaign.EndDate < DateTimeOffset.Now;

                        if (!publicCampaignDetailResponseDTO.Completed)
                            publicCampaignDetailResponseDTO.DaysLeft = (campaign.EndDate.Value - DateTimeOffset.UtcNow).Days;
                    }


                    if (campaign.Contributions.Any())
                    {

                        publicCampaignDetailResponseDTO.NumberOfContributions = campaign.Contributions.Count;


                        var numberOfDonors = await _appDatabaseContext
                                        .Contributions
                                        .AsNoTracking()
                                        .Select(i => new
                                        {
                                            i.CampaignId,
                                            i.UserId
                                        })
                                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                        .Where(i => i.CampaignId == campaign.Id)
                                        .Distinct()
                                        .CountAsync();

                        publicCampaignDetailResponseDTO.NumberOfDonors = numberOfDonors;

                        publicCampaignDetailResponseDTO.FundedPreviously = true;

                        var totalAmount = campaign.Contributions.Sum(i => i.Amount);
                        publicCampaignDetailResponseDTO.FundedPercentage = (int)((totalAmount / publicCampaignDetailResponseDTO.FundraisingGoals) * 100);

                        publicCampaignDetailResponseDTO.FundingAmountToDate = totalAmount;

                        publicCampaignDetailResponseDTO.FundingLastDateTime = campaign.Contributions.Select(i => i.CreatedAt).Max();
                    }

                    publicCampaignDetailResponseDTO.Photos = campaign.CampaignFiles.Where(i => i.TypeId == CampaignFileTypes.Photo)
                                                                    .Select(cFile => $"{BlobPathHelper.DomainPrefix}/public-md/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}");

                    publicCampaignDetailResponseDTO.Videos = campaign.CampaignFiles.Where(i => i.TypeId == CampaignFileTypes.Video)
                                                                    .Select(cFile => $"{BlobPathHelper.DomainPrefix}/public-md/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}");


                    publicCampaignDetailResponseDTO.NumberOfShares = await _appDatabaseContext
                                                                            .CampaignAnalytics
                                                                            .CountAsync(i => i.CampaignId == campaign.Id && i.TypeId == CampaignAnalyticTypes.Share);

                    publicCampaignDetailResponseDTO.NumberOfVisits = await _appDatabaseContext
                                                                            .CampaignAnalytics
                                                                            .CountAsync(i => i.CampaignId == campaign.Id && i.TypeId == CampaignAnalyticTypes.Visit);

                    publicCampaignDetailResponseDTO.NumberOfFollowers = await _appDatabaseContext
                                                                            .CampaignFollowers
                                                                            .CountAsync(i => i.CampaignId == campaign.Id);

                    if (this.User != null)
                    {
                        var userId = this.User.Identity.GetUserId();
                        if (userId.HasValue)
                        {
                            publicCampaignDetailResponseDTO.IsUserFollowing = await _appDatabaseContext
                                                                            .CampaignFollowers
                                                                            .AnyAsync(i => i.UserId == userId.Value && i.CampaignId == campaign.Id);

                            publicCampaignDetailResponseDTO.IsCurrentSignedInUserCreator = campaign.CreatedByUserId == userId.Value;
                        }
                    }

                    return publicCampaignDetailResponseDTO;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return null;

            }
        }

        [HttpGet("campaign")]
        public async Task<IActionResult> Campaign(CampaignTypes campaignTypeId, int campaignId)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var publicCampaignDetailResponseDTO = await GetCampaignAsync(campaignId, null);
                    return Ok(publicCampaignDetailResponseDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get campaign."));

            }
        }

        //https://localhost:5002/api/public/closestlocation
        [HttpGet("categories")]
        public IActionResult GetEnums()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var resultDTO = new ResultDTO();
                    resultDTO.Status = ResultDTOStatuses.Success;

                    var enumDTOs = new List<EnumDTO>();

                    var enums = Enum.GetValues(typeof(OrganizationCampaignCategories));

                    foreach (var e in enums)
                    {
                        var enumDTO = new EnumDTO();
                        enumDTO.Value = Convert.ToInt32(e);
                        enumDTO.Name = e.ToString();

                        enumDTO.Description = ((Enum)e).GetDescription();

                        enumDTOs.Add(enumDTO);
                    }

                    resultDTO.Data = enumDTOs;

                    return Ok(resultDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }


        [HttpPost("analytic")]
        public async Task<IActionResult> Analytic([FromBody] CampaignAnalyticDTO campaignAnalyticDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        var campaignAnalytic = new CampaignAnalytic();

                        campaignAnalytic.CampaignId = campaignAnalyticDTO.Id;
                        campaignAnalytic.UserAgent = this.Request.Headers[Microsoft.Net.Http.Headers.HeaderNames.UserAgent].ToString().Left(256);
                        campaignAnalytic.IPAddress = this.Request.HttpContext.Connection.RemoteIpAddress.GetAddressBytes();

                        if (this.User != null)
                            campaignAnalytic.UserId = this.User.Identity.GetUserId();

                        campaignAnalytic.TypeId = campaignAnalyticDTO.AnalyticType;

                        await _appDatabaseContext.CampaignAnalytics.AddAsync(campaignAnalytic);
                        await _appDatabaseContext.SaveChangesAsync();

                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to save analytics."));
            }
        }


        [HttpPost("campaigncontributions")]
        public async Task<IActionResult> CampaignContributions([FromBody] PublicCampaignContributionRequestDTO publicCampaignContributionRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var publicCampaignContributionResponseDTO = new PublicCampaignContributionResponseDTO();

                    publicCampaignContributionResponseDTO.PageNumber = publicCampaignContributionRequestDTO.PageNumber;
                    publicCampaignContributionResponseDTO.PageSize = publicCampaignContributionRequestDTO.PageSize;

                    var contributionsQry = _appDatabaseContext
                                            .Contributions
                                            .Include(i => i.User)
                                            .AsNoTracking()
                                             .Select(i => new
                                             {
                                                 i.CampaignId,
                                                 i.CreatedAt,
                                                 i.Amount,
                                                 i.Note,
                                                 i.Anonymous,
                                                 User = new { i.User.FirstName, i.User.LastName, i.User.AliasId }
                                             })
                                            .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                            .Where(i => i.CampaignId == publicCampaignContributionRequestDTO.CampaignId);

                    publicCampaignContributionResponseDTO.TotalNumberOfContributions = contributionsQry.Count();
                    publicCampaignContributionResponseDTO.TotalPages = (int)Math.Ceiling(publicCampaignContributionResponseDTO.TotalNumberOfContributions / (float)publicCampaignContributionResponseDTO.PageSize);

                    switch (publicCampaignContributionRequestDTO.SortField)
                    {
                        case ContributionSortFields.Date:
                            switch (publicCampaignContributionRequestDTO.SortDirection)
                            {
                                case SortDirections.Ascending:
                                    contributionsQry = contributionsQry
                                                            .OrderBy(j => j.CreatedAt);
                                    break;

                                case SortDirections.Descending:
                                    contributionsQry = contributionsQry
                                                            .OrderByDescending(j => j.CreatedAt);
                                    break;
                            }
                            break;

                        case ContributionSortFields.TopContributions:
                            switch (publicCampaignContributionRequestDTO.SortDirection)
                            {
                                case SortDirections.Ascending:
                                    contributionsQry = contributionsQry
                                                            .OrderBy(j => j.Amount);
                                    break;

                                case SortDirections.Descending:
                                    contributionsQry = contributionsQry
                                                            .OrderByDescending(j => j.Amount);
                                    break;
                            }
                            break;
                    }


                    publicCampaignContributionResponseDTO.Contributions = await contributionsQry.Select(i => new PublicCampaignDetailContributionResponseDTO()
                    {
                        Amount = i.Amount,
                        Date = i.CreatedAt,
                        Note = i.Note,
                        UserName = i.Anonymous ? "Anonymous" : $"{i.User.FirstName} {i.User.LastName}".Trim(),
                        UserAliasId = i.Anonymous ? "" : $"{i.User.AliasId.ToString()}",
                    })
                                                                                   .ToListAsync();



                    return Ok(publicCampaignContributionResponseDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get campaign contributions."));

            }
        }


        [HttpPost("newsletter")]
        public async Task<IActionResult> Newsletter([FromBody] NewsletterSubscriptionDTO newsletterSubscriptionDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        //
                        //make sure user is not already subscribed
                        //
                        var emailAlreadySubscribed = await _appDatabaseContext
                                                        .NewsletterSubscriptions
                                                        .AnyAsync(i => Microsoft.EntityFrameworkCore.EF.Functions.Like(i.Email, newsletterSubscriptionDTO.Email));

                        bool userAlreadySubscribed = emailAlreadySubscribed;

                        if (!userAlreadySubscribed)
                        {
                            var newsletterSubscription = new NewsletterSubscription();

                            if (this.User != null)
                                newsletterSubscription.UserId = this.User.Identity.GetUserId();

                            newsletterSubscription.Email = newsletterSubscriptionDTO.Email;

                            await _appDatabaseContext
                                        .NewsletterSubscriptions
                                        .AddAsync(newsletterSubscription);

                            await _appDatabaseContext.SaveChangesAsync();

                        }

                        await _emailHelper.SendNewsletterSubscriptionEmailAsync(newsletterSubscriptionDTO.Email);

                        return Ok(new ResultDTO(ResultDTOStatuses.Success, $"Subscribed"));

                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to add subscription."));
            }
        }

        //curl -i -X POST -H "Content-Type: application/json" -d "{\"NumberOfCampaigns\": 9}" https://localhost:5001/api/public/popularcampaigns
        [HttpPost("popularcampaigns")]
        public async Task<IActionResult> PopularCampaigns([FromBody] PublicPopularCampaignRequestDTO publicPopularCampaignRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        //
                        //check if value is already in cache
                        //
                        var jsonData = await _cache.GetStringAsync(_cachePopularCampaigns);
                        if (!string.IsNullOrWhiteSpace(jsonData))
                        {
                            var jsonDeserializedData = JsonConvert.DeserializeObject<IEnumerable<PublicPopularCampaignResponseDTO>>(jsonData);
                            if (jsonDeserializedData != null && jsonDeserializedData.Any())
                                return Ok(jsonDeserializedData);
                        }


                        var publicPopularCampaignResponseDTOs = new List<PublicPopularCampaignResponseDTO>();

                        var qry = _appDatabaseContext.Campaigns
                                    .OrderBy(i => i.CreatedAt)
                                    .Include(i => i.Contributions)
                                    .AsSplitQuery()
                                    .AsNoTracking()
                                    .Select(i => new
                                    {
                                        i.StatusId,
                                        i.CreatedAt,
                                        i.Id,
                                        i.TypeId,
                                        i.HeaderPictureURL,
                                        i.Name,
                                        i.Summary,
                                        i.TransactionId,
                                        i.FundraisingGoals,
                                        i.Contributions,
                                        i.CreatedByUserId,
                                        i.SEOFriendlyURL,
                                        User = (i.User != null) ? new { i.User.Id, i.User.FirstName, i.User.LastName, i.User.AliasId, i.User.PictureURL } : null
                                    })
                                    .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                    .Where(i => i.StatusId == CampaignStatuses.Active)
                                    .Take(publicPopularCampaignRequestDTO.NumberOfCampaigns);

                        var campaigns = await qry.ToListAsync();

                        foreach (var campaign in campaigns)
                        {
                            try
                            {
                                var publicPopularCampaignDTO = new PublicPopularCampaignResponseDTO();

                                publicPopularCampaignDTO.Id = campaign.Id;
                                publicPopularCampaignDTO.TypeId = campaign.TypeId;

                                switch (campaign.TypeId)
                                {
                                    case CampaignTypes.Organization:
                                        {
                                            var organizationCampaign = await _appDatabaseContext
                                                              .OrganizationCampaigns
                                                              .Include(i => i.BeneficiaryOrganization)
                                                              .AsNoTracking()
                                                              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                              .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                            publicPopularCampaignDTO.Category = $"Organization - {organizationCampaign.CategoryId.GetDescription()}";

                                            if (organizationCampaign.BeneficiaryOrganization != null)
                                                publicPopularCampaignDTO.Beneficiary = $"{organizationCampaign.BeneficiaryOrganization.Name}";
                                        }
                                        break;

                                    case CampaignTypes.People:
                                        {
                                            var peopleCampaign = await _appDatabaseContext
                                                              .PeopleCampaigns
                                                              .AsNoTracking()
                                                              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                              .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                            publicPopularCampaignDTO.Category = $"Beneficiary";

                                            publicPopularCampaignDTO.Beneficiary = $"{peopleCampaign.BeneficiaryName}";
                                        }
                                        break;
                                }

                                if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                                {
                                    if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                                        publicPopularCampaignDTO.PictureURL = $"{BlobPathHelper.DomainPrefix}/public-md/{campaign.HeaderPictureURL}";
                                }
                                else
                                {
                                    var campaignFile = await _appDatabaseContext
                                                                .CampaignFiles
                                                                .AsNoTracking()
                                                                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                                .Select(i => new { i.CampaignId, i.UID, i.Filename })
                                                                .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                    if (campaignFile != null)
                                        publicPopularCampaignDTO.PictureURL = $"{BlobPathHelper.DomainPrefix}/public-md/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, campaignFile.UID, campaignFile.Filename)}";
                                }

                                if (string.IsNullOrWhiteSpace(publicPopularCampaignDTO.PictureURL))
                                    publicPopularCampaignDTO.PictureURL = $"/assets/images/campaigns/default_{(campaign.TypeId == CampaignTypes.Organization ? "organization" : "people")}.jpg";

                                publicPopularCampaignDTO.Name = campaign.Name;
                                publicPopularCampaignDTO.Summary = campaign.Summary;
                                publicPopularCampaignDTO.FundraisingGoals = campaign.FundraisingGoals;
                                publicPopularCampaignDTO.SEOFriendlyURL = campaign.SEOFriendlyURL;

                                if (campaign.Contributions.Any())
                                {
                                    var totalAmount = campaign.Contributions.Sum(i => i.Amount);
                                    publicPopularCampaignDTO.FundedPercentage = (int)((totalAmount / publicPopularCampaignDTO.FundraisingGoals) * 100);
                                    publicPopularCampaignDTO.NumberOfDonors = campaign.Contributions.Select(i => i.UserId).Distinct().Count();
                                }

                                /*
                                var user = await _appDatabaseContext.Users
                                        .AsNoTracking()
                                        .Select(i => new { i.Id, i.FirstName, i.LastName, i.AliasId, i.PictureURL })
                                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                        .FirstOrDefaultAsync(i => i.Id == campaign.CreatedByUserId);
                                */

                                if (campaign.User != null)
                                {
                                    publicPopularCampaignDTO.OrganizedBy = $"{campaign.User.FirstName} {campaign.User.LastName}".Trim();

                                    if (!string.IsNullOrWhiteSpace(campaign.User.PictureURL))
                                        publicPopularCampaignDTO.OrganizedByProfilePictureURL = $"https://app.upraise.fund/api/file/getprofilepicture/{campaign.User.AliasId}";
                                }
                                publicPopularCampaignResponseDTOs.Add(publicPopularCampaignDTO);

                            }
                            catch (Exception ex)
                            {
                                wrappedLogger.LogError($"Unable to fetch Organization Campaign Id {campaign.Id} {ex.Message}");
                            }

                        }

                        var jsonSerializedData = JsonConvert.SerializeObject(publicPopularCampaignResponseDTOs);
                        var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                        await _cache.SetStringAsync(_cachePopularCampaigns, jsonSerializedData, options);

                        return Ok(publicPopularCampaignResponseDTOs);
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get popular campaigns."));

            }
        }


        //curl -i -X POST -H "Content-Type: application/json" -d "{\"east\": -113.83555757626955,\"north\": 51.33706769269358,\"south\": 50.992603381333666,\"west\": -114.09579622373049}" https://localhost:5001/api/public/localcampaigns
        [HttpPost("localcampaigns")]
        public async Task<IActionResult> LocalCampaigns([FromBody] PublicLocalCampaignRequestDTO publicLocalCampaignRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (publicLocalCampaignRequestDTO == null)
                            return BadRequest();

                        var publicLocalCampaignResponseDTO = new PublicLocalCampaignResponseDTO();

                        var qry = _appDatabaseContext.Campaigns
                                    .Include(i => i.Contributions)
                                    .AsSplitQuery()
                                    .AsNoTracking()
                                    .Select(i => new
                                    {
                                        i.StatusId,
                                        i.CreatedAt,
                                        i.Id,
                                        i.TypeId,
                                        i.HeaderPictureURL,
                                        i.Name,
                                        i.Summary,
                                        i.TransactionId,
                                        i.FundraisingGoals,
                                        i.Contributions,
                                        i.CreatedByUserId,
                                        i.GeoLocation,
                                        i.SEOFriendlyURL,
                                        User = (i.User != null) ? new { i.User.Id, i.User.FirstName, i.User.LastName, i.User.AliasId, i.User.PictureURL } : null
                                    })
                                    .Where(i => i.StatusId == CampaignStatuses.Active &&
                                                i.GeoLocation != null);

                        var polygonCoords = new Coordinate[]
                        {
                                        new Coordinate(publicLocalCampaignRequestDTO.east, publicLocalCampaignRequestDTO.north), //NE
                                        new Coordinate(publicLocalCampaignRequestDTO.west, publicLocalCampaignRequestDTO.north), //NW
                                        new Coordinate(publicLocalCampaignRequestDTO.west, publicLocalCampaignRequestDTO.south), //SW
                                        new Coordinate(publicLocalCampaignRequestDTO.east, publicLocalCampaignRequestDTO.south), //SE

                                        new Coordinate(publicLocalCampaignRequestDTO.east, publicLocalCampaignRequestDTO.north), //NE

                        };

                        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
                        var polygon = geometryFactory.CreatePolygon(polygonCoords);
                        var isValid = polygon.IsValid;

                        //var polygon = NetTopologySuite.Geometries.Polygon.DefaultFactory.CreatePolygon(polygonCoords);
                        qry = qry.Where(i => i.GeoLocation != null &&
                                            i.GeoLocation.Within(polygon)
                                         );
                        //else
                        //qry = qry.Where(i => i.GeoLocation != null);


                        var campaigns = await qry.ToListAsync();

                        foreach (var campaign in campaigns)
                        {
                            try
                            {
                                var publicLocalCampaignDataDTO = new PublicLocalCampaignDataDTO();

                                publicLocalCampaignDataDTO.Id = campaign.Id;
                                publicLocalCampaignDataDTO.TypeId = campaign.TypeId;
                                publicLocalCampaignDataDTO.SEOFriendlyURL = campaign.SEOFriendlyURL;
                                publicLocalCampaignDataDTO.Longitude = campaign.GeoLocation.X;
                                publicLocalCampaignDataDTO.Latitude = campaign.GeoLocation.Y;

                                switch (campaign.TypeId)
                                {
                                    case CampaignTypes.Organization:
                                        {
                                            var organizationCampaign = await _appDatabaseContext
                                                              .OrganizationCampaigns
                                                              .Include(i => i.BeneficiaryOrganization)
                                                              .AsNoTracking()
                                                              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                              .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                            publicLocalCampaignDataDTO.Category = $"Organization - {organizationCampaign.CategoryId.GetDescription()}";

                                            if (organizationCampaign.BeneficiaryOrganization != null)
                                                publicLocalCampaignDataDTO.Beneficiary = $"{organizationCampaign.BeneficiaryOrganization.Name}";
                                        }
                                        break;

                                    case CampaignTypes.People:
                                        {
                                            var peopleCampaign = await _appDatabaseContext
                                                              .PeopleCampaigns
                                                              .AsNoTracking()
                                                              .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                              .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                            publicLocalCampaignDataDTO.Category = $"Beneficiary";

                                            publicLocalCampaignDataDTO.Beneficiary = $"{peopleCampaign.BeneficiaryName}";
                                        }
                                        break;
                                }

                                if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                                {
                                    if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                                        publicLocalCampaignDataDTO.PictureURL = $"{BlobPathHelper.DomainPrefix}/public-md/{campaign.HeaderPictureURL}";
                                }
                                else
                                {
                                    var campaignFile = await _appDatabaseContext
                                                                .CampaignFiles
                                                                .AsNoTracking()
                                                                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                                .Select(i => new { i.CampaignId, i.UID, i.Filename })
                                                                .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                    if (campaignFile != null)
                                        publicLocalCampaignDataDTO.PictureURL = $"{BlobPathHelper.DomainPrefix}/public-md/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, campaignFile.UID, campaignFile.Filename)}";
                                }

                                if (string.IsNullOrWhiteSpace(publicLocalCampaignDataDTO.PictureURL))
                                    publicLocalCampaignDataDTO.PictureURL = $"/assets/images/campaigns/default_{(campaign.TypeId == CampaignTypes.Organization ? "organization" : "people")}.jpg";


                                publicLocalCampaignDataDTO.Name = campaign.Name;
                                publicLocalCampaignDataDTO.Summary = campaign.Summary;
                                publicLocalCampaignDataDTO.FundraisingGoals = campaign.FundraisingGoals;

                                if (campaign.Contributions.Any())
                                {
                                    var totalAmount = campaign.Contributions.Sum(i => i.Amount);
                                    publicLocalCampaignDataDTO.FundedPercentage = (int)((totalAmount / publicLocalCampaignDataDTO.FundraisingGoals) * 100);
                                    publicLocalCampaignDataDTO.NumberOfDonors = campaign.Contributions.Select(i => i.UserId).Distinct().Count();
                                }

                                /*
                                var user = await _appDatabaseContext.Users
                                        .AsNoTracking()
                                        .Select(i => new { i.Id, i.FirstName, i.LastName, i.AliasId, i.PictureURL })
                                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                        .FirstOrDefaultAsync(i => i.Id == campaign.CreatedByUserId);
                                */

                                if (campaign.User != null)
                                {
                                    publicLocalCampaignDataDTO.OrganizedBy = $"{campaign.User.FirstName} {campaign.User.LastName}".Trim();

                                    if (!string.IsNullOrWhiteSpace(campaign.User.PictureURL))
                                        publicLocalCampaignDataDTO.OrganizedByProfilePictureURL = $"https://app.upraise.fund/api/file/getprofilepicture/{campaign.User.AliasId}";
                                }
                                publicLocalCampaignResponseDTO.Campaigns.Add(publicLocalCampaignDataDTO);

                            }
                            catch (Exception ex)
                            {
                                wrappedLogger.LogError($"Unable to fetch Organization Campaign Id {campaign.Id} {ex.Message}");
                            }
                        }

                        return Ok(publicLocalCampaignResponseDTO);
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get local campaigns."));

            }
            /*
             * Populate SQL Test geography data 
             DECLARE @geo as GEOGRAPHY,@newgeo as GEOGRAPHY
SET @geo = (SELECT ZipGeo FROM Location.ZipCodes WHERE ZipCode='90210')

DECLARE @r float,@t float, @w float, @x float, @y float, @u float, @v float;

SET @u=RAND();
SET @v=RAND();

--8046m = ~ 5 miles
SET @r= 8046/(111300*1.0);
SET @w = @r * sqrt(@u);
SET @t = 2 * PI() * @v;
SET @x = @w * cos(@t);
SET @y = @w * sin(@t);
SET @x = @x / cos(@geo.Lat);

SET @newgeo = geography::STPointFromText('POINT('+CAST(@geo.Long+@x AS VARCHAR(MAX))+' '+CAST(@geo.Lat+@y AS VARCHAR(MAX))+')',4326)
--Convert the distance back to miles to validate
SELECT @geo.STDistance(@newgeo)/1609.34
            */

            //UPDATE Campaigns SET GeoLocation = dbo.GetRandomGeography(-114.066666, 51.049999)



        }

        //curl -i -X GET -H "Content-Type: application/json" https://localhost:5001/api/public/closestlocation
        [HttpGet("closestlocation")]
        public async Task<IActionResult> ClosestLocation()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    //
                    //We skip the cloest location check while we are not subscribed to this API ( free tier is too limited )
                    //
                    return Ok(new ResultDTO(ResultDTOStatuses.Error, "Not fetching closest location."));


                    var publicClosestLocationDTO = new PublicClosestLocationDTO();

                    var ipAddress = this.Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //ipAddress = "104.157.111.199";

                    var cacheKey = $"{_cacheClosestLocation}__{ipAddress}";

                    var jsonData = await _cache.GetStringAsync(cacheKey);
                    if (!string.IsNullOrWhiteSpace(jsonData))
                    {
                        try
                        {
                            publicClosestLocationDTO = JsonConvert.DeserializeObject<PublicClosestLocationDTO>(jsonData);
                            if (publicClosestLocationDTO != null &&
                                publicClosestLocationDTO.Latitude != 0.0 && publicClosestLocationDTO.Longitude != 0.0)
                                return Ok(new ResultDTO(ResultDTOStatuses.Success, publicClosestLocationDTO));
                        }
                        catch (Exception ex)
                        {
                            wrappedLogger.LogError($"Unable to deserialize json");
                        }
                    }

                    var ipAddressDetails = _ipStackClient.GetIpAddressDetails(ipAddress, "latitude,longitude", false, false);

                    publicClosestLocationDTO.Latitude = (float)ipAddressDetails.Latitude;
                    publicClosestLocationDTO.Longitude = (float)ipAddressDetails.Longitude;

                    var jsonSerializedData = JsonConvert.SerializeObject(publicClosestLocationDTO);
                    var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
                    await _cache.SetStringAsync(cacheKey, jsonSerializedData, options);

                    bool isValid = publicClosestLocationDTO.Latitude != 0 && publicClosestLocationDTO.Longitude != 0;

                    if (isValid)
                        return Ok(new ResultDTO(ResultDTOStatuses.Success, publicClosestLocationDTO));
                    else
                        return Ok(new ResultDTO(ResultDTOStatuses.Error, $"Could not find approx lat/lng for IP address {ipAddress}"));
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }


                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get closest location."));

            }
        }



    }
}