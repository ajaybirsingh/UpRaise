using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using UpRaise.DTOs;
using UpRaise.DTOs.Entities;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Models.Enums;
using UpRaise.Services;
using UpRaise.Services.EF;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/campaigns")]
    public class CampaignsController : Controller
    {
        private readonly ILogger<CampaignsController> _logger;
        private readonly IUserService _userService;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly IBlobManager _blobManager;
        //private readonly AppSettings _appSettings;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Entities.IDUser> _userManager;
        private readonly EmailHelper _emailHelper;
        private readonly SearchService _searchService;
        private readonly IHttpClientFactory _clientFactory;


        public CampaignsController(
            ILogger<CampaignsController> logger,
            IUserService userService,
            IMapper mapper,
            //IOptions<AppSettings> appSettings,
            AppDatabaseContext appDatabaseContext,
            IConfiguration configuration,
            UserManager<Entities.IDUser> userManager,
            SignInManager<IDUser> signInManager,
            EmailHelper emailHelper,
            SearchService searchService,
            IBlobManager blobManager,
            IHttpClientFactory clientFactory
            )
        {
            _logger = logger;
            _userService = userService;
            _signInManager = signInManager;
            _mapper = mapper;
            //_appSettings = appSettings.Value;
            _appDatabaseContext = appDatabaseContext;
            _configuration = configuration;
            _userManager = userManager;
            _emailHelper = emailHelper;
            //IUserStore<Entities.IdentityUser> store = null;
            _searchService = searchService;
            _blobManager = blobManager;

            _clientFactory = clientFactory;
        }



        private async Task<bool> AddCampaignFilesAsync(UpRaise.Entities.Campaign campaign, CampaignDTO campaignDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (campaignDTO.HeaderPhoto != null && !campaignDTO.HeaderPhoto.Id.HasValue)
                    {
                        var blobName = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, campaignDTO.HeaderPhoto.UID, campaignDTO.HeaderPhoto.Filename);
                        if (await _blobManager.ExistsAsync(ContainerNames.Public, blobName))
                        {
                            var campaignFile = new CampaignFile();
                            campaignFile.TypeId = CampaignFileTypes.HeaderPhoto;
                            campaignFile.UID = campaignDTO.HeaderPhoto.UID;
                            campaignFile.Filename = campaignDTO.HeaderPhoto.Filename;
                            campaignFile.SizeInBytes = campaignDTO.HeaderPhoto.FileSize;
                            campaignFile.UpdatedAt = campaignFile.CreatedAt = DateTimeOffset.UtcNow;
                            campaignFile.CreatedByUserId = this.User.Identity.GetUserId().Value;

                            campaign.CampaignFiles.Add(campaignFile);
                        }
                        else
                            wrappedLogger.LogError($"{blobName} does not exist.");
                    }


                    foreach (var video in campaignDTO.Videos)
                    {
                        if (!video.Id.HasValue)
                        {
                            var blobName = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, video.UID, video.Filename);
                            if (await _blobManager.ExistsAsync(ContainerNames.Public, blobName))
                            {
                                var campaignFile = new CampaignFile();
                                campaignFile.TypeId = CampaignFileTypes.Video;
                                campaignFile.UID = video.UID;
                                campaignFile.Filename = video.Filename;
                                campaignFile.SizeInBytes = video.FileSize;
                                campaignFile.UpdatedAt = campaignFile.CreatedAt = DateTimeOffset.UtcNow;
                                campaignFile.CreatedByUserId = this.User.Identity.GetUserId().Value;

                                campaign.CampaignFiles.Add(campaignFile);
                            }
                            else
                                wrappedLogger.LogError($"{blobName} does not exist.");
                        }
                    }

                    foreach (var photo in campaignDTO.Photos)
                    {
                        if (!photo.Id.HasValue)
                        {
                            var blobName = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, photo.UID, photo.Filename);
                            if (await _blobManager.ExistsAsync(ContainerNames.Public, blobName))
                            {
                                var campaignFile = new CampaignFile();
                                campaignFile.TypeId = CampaignFileTypes.Photo;
                                campaignFile.UID = photo.UID;
                                campaignFile.Filename = photo.Filename;
                                campaignFile.SizeInBytes = photo.FileSize;
                                campaignFile.UpdatedAt = campaignFile.CreatedAt = DateTimeOffset.UtcNow;
                                campaignFile.CreatedByUserId = this.User.Identity.GetUserId().Value;

                                campaign.CampaignFiles.Add(campaignFile);
                            }
                            else
                                wrappedLogger.LogError($"{blobName} does not exist.");
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return false;
            }
        }

        private async Task<bool> AddCampaignRedlineEventAsync(int campaignId, int userId, CampaignRedlineEventTypes eventType)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var campaignRedlineEvent = new CampaignRedlineEvent();
                    campaignRedlineEvent.CampaignId = campaignId;
                    campaignRedlineEvent.UserId = userId;
                    campaignRedlineEvent.Note = "";
                    campaignRedlineEvent.EventType = eventType;

                    await _appDatabaseContext.AddAsync(campaignRedlineEvent);

                    int numChanged = await _appDatabaseContext.SaveChangesAsync();

                    return true;

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return false;
            }
        }

        private async Task<string> GetFriendlyURLAsync(string campaignName)
        {
            if (string.IsNullOrWhiteSpace(campaignName))
                return null;

            var friendlyURl = UrlHelper.URLFriendly(campaignName, 120);
            
            var seoExists = await _appDatabaseContext.Campaigns
                                           .AnyAsync(i => i.SEOFriendlyURL == friendlyURl);

            if (seoExists)
            {
                var existingSEOs = _appDatabaseContext.Campaigns
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

                friendlyURl = $"{friendlyURl}-{maxNumber + 1}";
            }

            return friendlyURl;
        }

        [HttpPost("saveCampaign")]
        public async Task<IActionResult> SaveCampaign([FromBody] CampaignDTO campaignDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            Campaign campaign = null;

                            if (!campaignDTO.Id.HasValue)
                            {
                                //
                                //new item
                                //
                                campaign = _mapper.Map<Campaign>(campaignDTO);

                                campaign.CreatedByUserId = this.User.Identity.GetUserId().Value;
                                campaign.Summary = campaignDTO.Description.StripHTML().LeftTrim(1024, true);

                                if (campaignDTO.HeaderPhoto != null)
                                    campaign.HeaderPictureURL = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, campaignDTO.HeaderPhoto.UID, campaignDTO.HeaderPhoto.Filename);

                                if (campaignDTO.GeoLocationLongitude.HasValue && campaignDTO.GeoLocationLatitude.HasValue)
                                    campaign.GeoLocation = new NetTopologySuite.Geometries.Point(campaignDTO.GeoLocationLongitude.Value, campaignDTO.GeoLocationLatitude.Value) { SRID = 4326 };

                                campaign.CreatedAt = DateTimeOffset.UtcNow;
                                campaign.UpdatedAt = DateTimeOffset.UtcNow;

                                campaign.TypeId = campaignDTO.TypeId;

                                switch (campaignDTO.TypeId)
                                {
                                    case CampaignTypes.Organization:
                                        campaign.OrganizationCampaign = _mapper.Map<OrganizationCampaign>(campaignDTO.OrganizationCampaignDTO);
                                        campaign.StatusId = CampaignStatuses.Active;
                                        break;

                                    case CampaignTypes.People:
                                        campaign.PeopleCampaign = _mapper.Map<PeopleCampaign>(campaignDTO.PeopleCampaignDTO);
                                        campaign.StatusId = CampaignStatuses.PendingAcceptance;
                                        break;
                                }

                                await _appDatabaseContext.Campaigns.AddAsync(campaign);
                                await AddCampaignFilesAsync(campaign, campaignDTO);
                            }
                            else
                            {
                                //
                                //update campaign
                                //
                                campaign = await _appDatabaseContext
                                                            .Campaigns
                                                            .Include(i => i.CampaignFiles)
                                                            .FirstOrDefaultAsync(i => i.Id == campaignDTO.Id.Value);
                                if (campaign != null)
                                {
                                    campaign = _mapper.Map(campaignDTO, campaign);

                                    //
                                    //Remove any blobs that are no longer referenced
                                    //
                                    var incomingCampaignFilesIds = new List<int>();
                                    if (campaignDTO.HeaderPhoto != null)
                                    {
                                        if (campaignDTO.HeaderPhoto.Id.HasValue)
                                            incomingCampaignFilesIds.Add(campaignDTO.HeaderPhoto.Id.Value);
                                        else
                                            campaign.HeaderPictureURL = $"{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, campaignDTO.HeaderPhoto.UID, campaignDTO.HeaderPhoto.Filename)}"; //this is a new or changed file so we should save it
                                    }
                                    else
                                        campaign.HeaderPictureURL = null;



                                    if (campaignDTO.Photos != null)
                                        incomingCampaignFilesIds.AddRange(campaignDTO.Photos.Where(i => i.Id.HasValue).Select(i => i.Id.Value));

                                    if (campaignDTO.Videos != null)
                                        incomingCampaignFilesIds.AddRange(campaignDTO.Videos.Where(i => i.Id.HasValue).Select(i => i.Id.Value));

                                    var removedCampaignFiles = campaign.CampaignFiles.ExceptBy(incomingCampaignFilesIds, j => j.Id);
                                    foreach (var removedCampaignFile in removedCampaignFiles)
                                    {
                                        campaign.CampaignFiles.Remove(removedCampaignFile);
                                    }


                                    await AddCampaignFilesAsync(campaign, campaignDTO);


                                    switch (campaignDTO.TypeId)
                                    {
                                        case CampaignTypes.Organization:
                                            _appDatabaseContext
                                                .Entry(campaign)
                                                .Reference(i => i.OrganizationCampaign)
                                                .Load();

                                            campaign.OrganizationCampaign = _mapper.Map(campaignDTO.OrganizationCampaignDTO, campaign.OrganizationCampaign);
                                            campaign.OrganizationCampaign.UpdatedAt = DateTimeOffset.UtcNow;
                                            break;

                                        case CampaignTypes.People:
                                            _appDatabaseContext
                                                .Entry(campaign)
                                                .Reference(i => i.PeopleCampaign)
                                                .Load();


                                            campaign.PeopleCampaign = _mapper.Map(campaignDTO.PeopleCampaignDTO, campaign.PeopleCampaign);
                                            campaign.PeopleCampaign.UpdatedAt = DateTimeOffset.UtcNow;
                                            break;
                                    }

                                    campaign.UpdatedAt = DateTimeOffset.UtcNow;
                                }
                                else
                                {
                                    //error - could not find claim detail to update
                                    wrappedLogger.LogError("Could not save organization claim.");
                                    return Ok(new ResultDTO(ResultDTOStatuses.Error, "Could not save organization claim."));
                                }
                            }

                            campaign.SEOFriendlyURL = await GetFriendlyURLAsync(campaign.Name);


                            var upsertedCampaignFiles = _appDatabaseContext.ChangeTracker
                                .Entries<CampaignFile>()
                                .Where(i => (i.State == EntityState.Added || i.State == EntityState.Modified))
                                .Select(i => i.Entity)
                                .ToList();

                            var deletedCampaignFiles = _appDatabaseContext.ChangeTracker
                                .Entries<CampaignFile>()
                                .Where(i => i.State == EntityState.Deleted)
                                .Select(i => i.Entity)
                                .ToList();


                            var numChanges = await _appDatabaseContext.SaveChangesAsync(false);

                            if (numChanges.HasValue)
                            {
                                var descriptionBlobPath = BlobPathHelper.GetCampaignDescriptionFilename(campaignDTO.TypeId, campaign.Id, null);
                                var blobClient = await _blobManager.UploadFromStringAsync(ContainerNames.Data, descriptionBlobPath, campaignDTO.Description, true);
                                if (blobClient == null)
                                    wrappedLogger.LogError("Unable to save description.");

                                //
                                //we got this far then that means the save went through and we should clear the temp flag off of the blobs so they are not deleted
                                //
                                foreach (var upsertedCampaignFile in upsertedCampaignFiles)
                                {
                                    var blobName = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, upsertedCampaignFile.UID, upsertedCampaignFile.Filename);
                                    await _blobManager.SetTagAsync(ContainerNames.Public, blobName, "Status", "ok");
                                }

                                //
                                //delete any unused files from blob storage
                                //
                                foreach (var deletedCampaignFile in deletedCampaignFiles)
                                {
                                    var blobName = BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, deletedCampaignFile.UID, deletedCampaignFile.Filename);

                                    if (!await _blobManager.DeleteAsync(ContainerNames.Public, blobName))
                                        wrappedLogger.LogError($"Unable to delete blob '{blobName}'");
                                }


                                await _searchService.UpsertCampaignAsync(campaign);

                                if (!campaignDTO.Id.HasValue)
                                {
                                    //new item
                                    await AddCampaignRedlineEventAsync(campaign.Id, this.User.Identity.GetUserId().Value, CampaignRedlineEventTypes.Created);
                                }

                                return Ok(new ResultDTO(ResultDTOStatuses.Success, campaign));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to save organization campaign."));
            }
        }

        [HttpPost("emailBeneficiary")]
        public async Task<IActionResult> EmailBeneficiary([FromBody] EmailBeneficiaryRequestDTO emailBeneficiaryRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            if (await _emailHelper.SendBeneficiaryEmailAsync(emailBeneficiaryRequestDTO.CampaignId, this.HttpContext))
                            {
                                return Ok(new ResultDTO(ResultDTOStatuses.Success, ""));
                            }
                            else
                            {
                                wrappedLogger.LogError($"Unable to send email to beneficiary");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to notify beneficiary."));
            }
        }



        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] CampaignFollowDTO campaignFollowDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var userId = this.User.Identity.GetUserId();
                            if (userId.HasValue)
                            {
                                var campaignFollower = await _appDatabaseContext
                                                                        .CampaignFollowers
                                                                        .FirstOrDefaultAsync(i => i.CampaignId == campaignFollowDTO.Id &&
                                                                                                  i.UserId == userId.Value);

                                if (campaignFollowDTO.IsFollowing)
                                {
                                    //
                                    // the user wants to follow and they are not in the follow list then add them
                                    //
                                    if (campaignFollower == null)
                                    {
                                        campaignFollower = new CampaignFollower();

                                        campaignFollower.CampaignId = campaignFollowDTO.Id;
                                        campaignFollower.UserId = userId.Value;

                                        await _appDatabaseContext
                                            .CampaignFollowers
                                            .AddAsync(campaignFollower);

                                        await _appDatabaseContext.SaveChangesAsync();
                                    }

                                }
                                else
                                {
                                    //
                                    // the user wants to stop following and they are currently following then remove them
                                    //
                                    if (campaignFollower != null)
                                    {
                                        _appDatabaseContext
                                                .CampaignFollowers
                                                .Remove(campaignFollower);

                                        await _appDatabaseContext.SaveChangesAsync();
                                    }
                                }

                                return Ok(new ResultDTO(ResultDTOStatuses.Success, campaignFollowDTO.IsFollowing));

                            }

                        }

                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to follow campaign."));
            }
        }

        [HttpPost("redlinecomment")]
        public async Task<IActionResult> RedlineComment([FromBody] CampaignRedlineCommentRequestDTO campaignRedlineCommentRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var userId = this.User.Identity.GetUserId();
                            if (userId.HasValue)
                            {
                                var campaignRedlineComment = new CampaignRedlineComment();
                                campaignRedlineComment.CampaignId = campaignRedlineCommentRequestDTO.CampaignId;
                                campaignRedlineComment.UserId = userId.Value;
                                campaignRedlineComment.Comment = campaignRedlineCommentRequestDTO.Comment;

                                await _appDatabaseContext.AddAsync(campaignRedlineComment);

                                int numChanged = await _appDatabaseContext.SaveChangesAsync();

                                //await _emailHelper.SendNewsletterSubscriptionEmailAsync(newsletterSubscriptionDTO.Email);

                                return Ok(new ResultDTO(ResultDTOStatuses.Success, ""));
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to add redline comment."));
            }
        }

        [HttpPost("redlinestatus")]
        public async Task<IActionResult> RedlineStatus([FromBody] CampaignRedlineStatusDTO campaignRedlineStatusDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var userId = this.User.Identity.GetUserId();
                            if (userId.HasValue)
                            {
                                var campaign = await _appDatabaseContext.Campaigns
                                                            .FirstOrDefaultAsync(i => i.TransactionId == campaignRedlineStatusDTO.transactionId);

                                if (campaign != null)
                                {
                                    if (campaign.StatusId == CampaignStatuses.PendingAcceptance)
                                    {
                                        if (campaignRedlineStatusDTO.approved)
                                            campaign.StatusId = CampaignStatuses.Active;
                                        else
                                            campaign.StatusId = CampaignStatuses.Disabled;

                                        var numChanges = await _appDatabaseContext.SaveChangesAsync();

                                        await AddCampaignRedlineEventAsync(campaign.Id, userId.Value, campaignRedlineStatusDTO.approved?CampaignRedlineEventTypes.Accepted: CampaignRedlineEventTypes.Rejected);

                                        return Ok(new ResultDTO(ResultDTOStatuses.Success, ""));
                                    }
                                }

                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to add redline comment."));
            }
        }


        [HttpGet("redline")]
        public async Task<IActionResult> Redline(string id)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var campaignRedlineDTO = new CampaignRedlineDTO();

                    var campaign = await _appDatabaseContext.Campaigns
                            //.Include(i => i.User)
                            .Include(i => i.CampaignRedlineComments).ThenInclude(i => i.User)
                            .Include(i => i.CampaignRedlineEvents).ThenInclude(i => i.User)
                            .Select(i => new
                            {
                                i.TransactionId,
                                i.StatusId,
                                i.Id,
                                i.TypeId,
                                i.Name,
                                i.CreatedAt,
                                i.HeaderPictureURL,
                                i.UpdatedAt,
                                i.FundraisingGoals,
                                i.GeoLocationAddress,
                                i.FeatureScore,
                                i.EndDate,
                                i.CampaignRedlineComments,
                                i.CampaignRedlineEvents,
                                User = (i.User != null) ? new { PictureURL = i.User.PictureURL, FirstName = i.User.FirstName, LastName = i.User.LastName, AliasId = i.User.AliasId } : null
                            })
                            .AsSplitQuery()
                            .AsNoTracking()
                            //.Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                            .FirstOrDefaultAsync(i => i.TransactionId == id);

                    if (campaign == null)
                    {
                        wrappedLogger.LogError($"Could not find campaign via tx id {id}");
                        return NotFound();
                    }

                    //if (campaign.StatusId == CampaignStatuses.Active)
                    //{
                    //var redirectUrl = $"~/home/detail/{(int)campaign.TypeId}/{campaign.Id}";
                    //return LocalRedirect(redirectUrl);
                    //}

                    campaignRedlineDTO.Id = campaign.Id;
                    campaignRedlineDTO.TransactionId = campaign.TransactionId;
                    campaignRedlineDTO.Type = campaign.TypeId;

                    campaignRedlineDTO.Status = campaign.StatusId;

                    /*
                    if (campaign.TypeId == CampaignTypes.Organization)
                    {
                        var organizationCampaign = await _appDatabaseContext.OrganizationCampaigns
                        .AsNoTracking()
                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                        .Select(i => new { i.CampaignId, i.CategoryId })
                        .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                        campaignRedlineDTO.CategoryId = organizationCampaign.CategoryId;
                    }
                    */

                    campaignRedlineDTO.Name = campaign.Name;

                    var descriptionBlobPath = BlobPathHelper.GetCampaignDescriptionFilename(campaign.TypeId, campaign.Id, null);
                    campaignRedlineDTO.Description = await _blobManager.ReadBlobStringAsync(ContainerNames.Data, descriptionBlobPath);


                    campaignRedlineDTO.CreatedByUserFullName = $"{campaign.User.FirstName} {campaign.User.LastName}".Trim();

                    //if (campaign.BeneficiaryOrganization != null)
                    //{
                    //publicCampaignResponseDTO.BeneficiaryOrganizationName = campaign.BeneficiaryOrganization.Name;
                    //publicCampaignResponseDTO.Location = $"{campaign.BeneficiaryOrganization.AddressCity}, {campaign.BeneficiaryOrganization.AddressStateProvinceCounty}";
                    //}
                    campaignRedlineDTO.CreatedAt = campaign.CreatedAt;

                    if (!string.IsNullOrWhiteSpace(campaign.HeaderPictureURL))
                        campaignRedlineDTO.HeaderPictureURL = $"{BlobPathHelper.DomainPrefix}/public/{campaign.HeaderPictureURL}";

                    if (string.IsNullOrWhiteSpace(campaignRedlineDTO.HeaderPictureURL))
                        campaignRedlineDTO.HeaderPictureURL = $"/assets/images/campaigns/default_{(campaign.TypeId == CampaignTypes.Organization ? "organization" : "people")}.jpg";

                    campaignRedlineDTO.UpdatedAt = campaign.UpdatedAt;

                    campaignRedlineDTO.CreatedByUserAliasId = campaign.User.AliasId;
                    campaignRedlineDTO.ShowProfilePic = !string.IsNullOrWhiteSpace(campaign.User.PictureURL);

                    campaignRedlineDTO.FundraisingGoals = campaign.FundraisingGoals;

                    campaignRedlineDTO.Location = campaign.GeoLocationAddress;

                    campaignRedlineDTO.Featured = campaign.FeatureScore.HasValue && campaign.FeatureScore.Value > 0;

                    if (campaign.EndDate.HasValue)
                    {
                        campaignRedlineDTO.Completed = campaign.EndDate < DateTimeOffset.Now;

                        if (!campaignRedlineDTO.Completed)
                            campaignRedlineDTO.DaysLeft = (campaign.EndDate.Value - DateTimeOffset.UtcNow).Days;
                    }


                    campaignRedlineDTO.Comments = campaign
                                                      .CampaignRedlineComments
                                                      .OrderBy(i => i.CreatedAt)
                                                      .Select(x => new CampaignRedlineCommentDTO()
                                                      {
                                                          CampaignId = x.CampaignId,
                                                          UserAliasId = string.IsNullOrWhiteSpace(x.User.PictureURL) ? null : x.User.AliasId.ToString(),
                                                          UserFullName = $"{x.User.FirstName} {x.User.LastName}".Trim(),
                                                          Comment = x.Comment,
                                                          CreatedAt = x.CreatedAt
                                                      });

                    campaignRedlineDTO.Events = campaign
                                                      .CampaignRedlineEvents
                                                      .OrderBy(i => i.CreatedAt)
                                                      .Select(x => new CampaignRedlineEventDTO()
                                                      {
                                                          CampaignId = x.CampaignId,
                                                          UserAliasId = string.IsNullOrWhiteSpace(x.User.PictureURL) ? null : x.User.AliasId.ToString(),
                                                          UserFullName = $"{x.User.FirstName} {x.User.LastName}".Trim(),
                                                          Note = x.Note,
                                                          EventType = x.EventType,
                                                          CreatedAt = x.CreatedAt
                                                      });

                    /*
                    campaignRedlineDTO.Photos = campaign.CampaignFiles.Where(i => i.TypeId == CampaignFileTypes.Photo)
                                                                    .Select(cFile => $"{BlobPathHelper.DomainPrefix}/public/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}");

                    campaignRedlineDTO.Videos = campaign.CampaignFiles.Where(i => i.TypeId == CampaignFileTypes.Video)
                                                                    .Select(cFile => $"{BlobPathHelper.DomainPrefix}/public/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}");
                    */

                    return Ok(campaignRedlineDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to redline."));

            }
        }

        [HttpPost("yourCampaigns")]
        public async Task<IActionResult> YourCampaigns([FromBody] YourCampaignRequestDTO yourCampaignRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {

                            var userId = this.User.Identity.GetUserId();

                            var campaignsQry = _appDatabaseContext.Campaigns
                                .AsNoTracking()
                                .Select(i => new
                                {
                                    i.Id,
                                    i.TransactionId,
                                    i.StatusId,
                                    i.TypeId,
                                    i.Name,
                                    i.Summary,
                                    i.StartDate,
                                    i.EndDate,
                                    i.CreatedAt,
                                    i.HeaderPictureURL,
                                    i.CreatedByUserId,
                                    i.UpdatedAt
                                })
                                .Where(i => i.CreatedByUserId == userId)
                                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5));

                            switch (yourCampaignRequestDTO.SortOrder)
                            {
                                case YourCampaignSortOrders.Newest:
                                    campaignsQry = campaignsQry.OrderByDescending(i => i.UpdatedAt);
                                    break;

                                case YourCampaignSortOrders.Oldest:
                                    campaignsQry = campaignsQry.OrderBy(i => i.UpdatedAt);
                                    break;
                            }


                            var yourCampaignResponseDTO = new YourCampaignResponseDTO();
                            yourCampaignResponseDTO.PageNumber = yourCampaignRequestDTO.PageNumber;
                            yourCampaignResponseDTO.PageSize = yourCampaignRequestDTO.PageSize;

                            yourCampaignResponseDTO.TotalItems = await campaignsQry.CountAsync();

                            yourCampaignResponseDTO.TotalPages = yourCampaignResponseDTO.TotalItems / yourCampaignRequestDTO.PageSize;

                            yourCampaignResponseDTO.YourCampaigns = await campaignsQry
                                                                        .Skip((yourCampaignRequestDTO.PageNumber - 1) * yourCampaignRequestDTO.PageSize)
                                                                        .Take(yourCampaignRequestDTO.PageSize)
                                                                        .Select(i => new YourCampaignDTO
                                                                        {
                                                                            CampaignId = i.Id,
                                                                            TransactionId = i.TransactionId,
                                                                            Status = i.StatusId,
                                                                            Type = i.TypeId,
                                                                            Name = i.Name,
                                                                            Description = i.Summary,
                                                                            StartDate = i.StartDate,
                                                                            EndDate = i.EndDate,
                                                                            HeaderPictureURL = string.IsNullOrWhiteSpace(i.HeaderPictureURL) ? "" : $"{BlobPathHelper.DomainPrefix}/public/{i.HeaderPictureURL}",
                                                                            CreatedAt = i.CreatedAt,
                                                                            UpdatedAt = i.UpdatedAt,
                                                                        })
                                                                        .ToListAsync();

                            return Ok(new ResultDTO(ResultDTOStatuses.Success, yourCampaignResponseDTO));
                        }
                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to fetch your campaigns."));
            }
        }


        [HttpGet("getEditCampaign")]
        public async Task<IActionResult> GetEditCampaign(string id)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var editCampaignDTO = new EditCampaignDTO();

                    var campaign = await _appDatabaseContext.Campaigns
                            .Include(i => i.CampaignFiles)
                            .AsSplitQuery()
                            .AsNoTracking()
                            //.Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5)) //geospatial 
                            .FirstOrDefaultAsync(i => i.TransactionId == id);

                    if (campaign == null)
                    {
                        wrappedLogger.LogError($"Could not find campaign via tx id {id}");
                        return NotFound();
                    }

                    editCampaignDTO.Id = campaign.Id;
                    editCampaignDTO.TransactionId = campaign.TransactionId;
                    editCampaignDTO.TypeId = campaign.TypeId;

                    editCampaignDTO.GeoLocationCountryId = campaign.GeoLocationCountryId;
                    editCampaignDTO.GeoLocationAddress = campaign.GeoLocationAddress;
                    editCampaignDTO.CurrencyId = campaign.CurrencyId;
                    editCampaignDTO.FundraisingGoals = campaign.FundraisingGoals;

                    editCampaignDTO.Name = campaign.Name;

                    var descriptionBlobPath = BlobPathHelper.GetCampaignDescriptionFilename(campaign.TypeId, campaign.Id, null);
                    editCampaignDTO.Description = await _blobManager.ReadBlobStringAsync(ContainerNames.Data, descriptionBlobPath);

                    editCampaignDTO.StartDate = campaign.StartDate;
                    editCampaignDTO.EndDate = campaign.EndDate;

                    var headerCampaignFile = campaign.CampaignFiles.FirstOrDefault(i => i.TypeId == CampaignFileTypes.HeaderPhoto);
                    if (headerCampaignFile != null)
                    {
                        editCampaignDTO.HeaderPhoto = new EditCampaignFileDTO()
                        {
                            Id = headerCampaignFile.Id,
                            url = $"{BlobPathHelper.DomainPrefix}/public/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, headerCampaignFile.UID, headerCampaignFile.Filename)}"
                        };
                    }


                    editCampaignDTO.Photos = campaign
                                                        .CampaignFiles
                                                        .Where(i => i.TypeId == CampaignFileTypes.Photo)
                                                        .Select(cFile => new EditCampaignFileDTO()
                                                        {
                                                            Id = cFile.Id,
                                                            url = $"{BlobPathHelper.DomainPrefix}/public/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}"
                                                        });

                    editCampaignDTO.Videos = campaign
                                                        .CampaignFiles
                                                        .Where(i => i.TypeId == CampaignFileTypes.Video)
                                                        .Select(cFile => new EditCampaignFileDTO()
                                                        {
                                                            Id = cFile.Id,
                                                            url = $"{BlobPathHelper.DomainPrefix}/public/{BlobPathHelper.GetCampaignFileBlobFilename(campaign.TransactionId, cFile.UID, cFile.Filename)}"
                                                        });

                    editCampaignDTO.AcceptTermsAndConditions = campaign.AcceptedTermsAndConditions;

                    editCampaignDTO.DistributionTerms = campaign.DistributionTerms?.ConvertJSONToList();


                    switch (campaign.TypeId)
                    {
                        case CampaignTypes.Organization:
                            {
                                var organization = await _appDatabaseContext.OrganizationCampaigns
                                        .Include(i => i.BeneficiaryOrganization)
                                        .AsNoTracking()
                                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                        .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                editCampaignDTO.Organization = new EditOrganizationCampaignDTO();

                                editCampaignDTO.Organization.CategoryId = organization.CategoryId;

                                editCampaignDTO.Organization.BeneficiaryOrganization = organization.BeneficiaryOrganization?.Name;

                                editCampaignDTO.Organization.Location = organization.Location;
                                editCampaignDTO.Organization.ContactName = organization.ContactName;
                                editCampaignDTO.Organization.ContactEmail = organization.ContactEmail;
                                editCampaignDTO.Organization.ContactPhone = organization.ContactPhone;

                                editCampaignDTO.Organization.CampaignConditions = organization.Conditions?.ConvertJSONToList();

                                editCampaignDTO.Organization.BeneficiaryMessage = organization.BeneficiaryMessage;
                            }
                            break;

                        case CampaignTypes.People:
                            {
                                var people = await _appDatabaseContext.PeopleCampaigns
                                        .AsNoTracking()
                                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                        .FirstOrDefaultAsync(i => i.CampaignId == campaign.Id);

                                editCampaignDTO.People = new EditPeopleCampaignDTO();

                                editCampaignDTO.People.BeneficiaryName = people.BeneficiaryName;
                                editCampaignDTO.People.BeneficiaryEmail = people.BeneficiaryEmail;
                                editCampaignDTO.People.BeneficiaryMessage = people.BeneficiaryMessage;
                            }
                            break;
                    }

                    return Ok(editCampaignDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get campaign."));

            }
        }

        [HttpPost("geocoding")]
        public async Task<IActionResult> Geocoding([FromBody] GeocodingRequestDTO geocodingRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        //
                        //https://positionstack.com/quickstart

                        var client = _clientFactory.CreateClient();

                        /*
                        var countryCode = "";
                        switch (geocodingRequestDTO.CountryId)
                        {
                            case Countries.Canada:
                                countryCode = "CA";
                                break;

                            case Countries.USA:
                                countryCode = "US";
                                break;

                        }
                         */


                        var result = await GetAddressLocationAsync(geocodingRequestDTO.Location);
                        if (result.latitude != null && result.longitude != null)
                        {
                            var geocodingResponse = new GeocodingResponseDTO();

                            geocodingResponse.FormattedAddress = result.formattedAddress;
                            geocodingResponse.Longitude = result.longitude.Value;
                            geocodingResponse.Latitude = result.latitude.Value;

                            return Ok(new ResultDTO(ResultDTOStatuses.Success, geocodingResponse));
                        }


                        /*
                        var encodedLocation = HttpUtility.UrlEncode(geocodingRequestDTO.Location);
                        var uri = $"http://api.positionstack.com/v1/forward?access_key=b95bd90868c1fe808cbafe5110dfec16&output=json&limit=1&&query={encodedLocation}";
                        var request = new HttpRequestMessage(HttpMethod.Get, uri);
                        var response = await client.SendAsync(request);

                        if (response.IsSuccessStatusCode)
                        {
                            using (var responseStream = await response.Content.ReadAsStreamAsync())
                            {
                                var geocodeInfo = await JsonSerializer.DeserializeAsync<GeocodeResult>(responseStream);

                                if (geocodeInfo != null && geocodeInfo.data.Any())
                                {
                                    var geocodingResponse = new GeocodingResponseDTO();

                                    var bestResponse = geocodeInfo.data
                                                            //.OrderByDescending(i=>i.confidence)
                                                            .First();


                                    geocodingResponse.Longitude = bestResponse.longitude;
                                    geocodingResponse.Latitude = bestResponse.latitude;

                                    return Ok(new ResultDTO(ResultDTOStatuses.Success, geocodingResponse));

                                }

                            }

                        }
                        */


                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to geocode."));
            }
        }


        private async Task<(double? latitude, double? longitude, string formattedAddress)> GetAddressLocationAsync(string address)
        {
            try
            {
                var encodedAddress = HttpUtility.UrlEncode(address);

                var _apiKey = "AIzaSyDUGZjfzhDWQuuTjaQHeO89BloLDL1nOIw";
                var targetUrl = $"https://maps.googleapis.com/maps/api/geocode/json" +
                    $"?address={encodedAddress}" +
                    $"&inputtype=textquery&fields=geometry" +
                    $"&key={_apiKey}";


                _logger.LogDebug($"GetAddressLocation : targetUrl:{targetUrl}");

                using var client = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    if (stream == null || stream.CanRead == false)
                        return (null, null, null);

                    using var sr = new StreamReader(stream);
                    var jsonString = sr.ReadToEnd();
                    dynamic responseObject = JObject.Parse(jsonString);

                    var results = responseObject["results"];

                    var formattedAddress = results[0]?["formatted_address"];
                    var lat = results[0]?["geometry"]?["location"]?["lat"];
                    var lng = results[0]?["geometry"]?["location"]?["lng"];

                    return (lat, lng, formattedAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR USING Geocoding Service", ex);
                throw;
            }

            return (null, null, null);
        }

    }
}