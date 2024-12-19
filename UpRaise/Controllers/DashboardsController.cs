using Amazon.SimpleEmailV2.Model;
using AutoMapper;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Services;
using UpRaise.Services.EF;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/dashboards")]
    public class DashboardsController : Controller
    {
        private readonly ILogger<DashboardsController> _logger;
        private readonly IUserService _userService;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly IMapper _mapper;
        //private readonly AppSettings _appSettings;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Entities.IDUser> _userManager;
        private readonly EmailHelper _emailHelper;

        public DashboardsController(
            ILogger<DashboardsController> logger,
            IUserService userService,
            IMapper mapper,
            //IOptions<AppSettings> appSettings,
            AppDatabaseContext appDatabaseContext,
            IConfiguration configuration,
            UserManager<Entities.IDUser> userManager,
            SignInManager<IDUser> signInManager,
            EmailHelper emailHelper
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
        }


        [HttpGet("getcampaigns")]
        public async Task<IActionResult> GetCampaigns()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var result = new ResultDTO();

                    var userId = this.User.Identity.GetUserId();

                    var campaigns = await _appDatabaseContext
                                .Campaigns
                                .Where(i => i.CreatedByUserId == userId)
                                .OrderByDescending(i => i.CreatedAt)
                                .Select(i => new DashboardCampaignResponseDTO
                                {
                                    Id = i.Id,
                                    Type = i.TypeId,
                                    Name = i.Name
                                })
                                .AsNoTracking()
                                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                .ToListAsync();


                    result.Status = ResultDTOStatuses.Success;

                    var dashboardCampaignResponseDTOs = campaigns;

                    result.Data = dashboardCampaignResponseDTOs;

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get campaign names."));

            }
        }


        [HttpPost("campaign")]
        public async Task<IActionResult> Campaign([FromBody] DashboardCampaignRequestDTO dashboardCampaignRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var dashboardCampaignResponseDTO = new DashboardCampaignResponseDTO();

                        var campaign = await _appDatabaseContext
                            .Campaigns
                            .AsNoTracking()
                            .Select(i => new
                            {
                                i.Id,
                                i.TypeId,
                                i.Name,
                                i.StartDate,
                                i.EndDate,
                                i.FundraisingGoals
                            })
                            .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(2))
                            .FirstOrDefaultAsync(i => i.Id == dashboardCampaignRequestDTO.Id);


                        if (campaign != null)
                        {
                            dashboardCampaignResponseDTO.Id = campaign.Id;
                            dashboardCampaignResponseDTO.Type = campaign.TypeId;
                            dashboardCampaignResponseDTO.Name = campaign.Name;


                            /*
                            dashboardCampaignResponseDTO.Location = organizationCampaign.Location;

                            if (organizationCampaign.BeneficiaryOrganization != null)
                                dashboardCampaignResponseDTO.BeneficiaryName = organizationCampaign.BeneficiaryOrganization.Name;
                            */

                            dashboardCampaignResponseDTO.StartDate = campaign.StartDate;
                            dashboardCampaignResponseDTO.EndDate = campaign.EndDate;
                            dashboardCampaignResponseDTO.FundraisingGoal = campaign.FundraisingGoals;

                            if (campaign.EndDate.HasValue && campaign.EndDate > DateTimeOffset.Now)
                                dashboardCampaignResponseDTO.DaysLeft = (campaign.EndDate.Value - DateTimeOffset.UtcNow).Days;
                            else
                                dashboardCampaignResponseDTO.DaysLeft = 0;

                            var contributions = await _appDatabaseContext
                                .Contributions
                                .AsNoTracking()
                                .Where(i => i.CampaignId == campaign.Id)
                                .Select(i => new { Id = i.Id, UserId = i.UserId, Amount = i.Amount, Date = i.CreatedAt, ContributionTypeId = i.ContributionTypeId })
                                .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(2))
                                .ToListAsync();

                            dashboardCampaignResponseDTO.NumberOfTotalContributors = contributions.Count();
                            dashboardCampaignResponseDTO.NumberOfUniqueContributors = contributions.Select(i => i.UserId).Distinct().Count();
                            dashboardCampaignResponseDTO.AmountRaised = contributions.Sum(i => i.Amount);

                            dashboardCampaignResponseDTO.Contributions = contributions
                                                                            .Select(i => new DashboardCampaignContributionDTO()
                                                                            {
                                                                                Id = i.Id,
                                                                                Date = i.Date,
                                                                                Amount = i.Amount,
                                                                                ContributionTypeId = i.ContributionTypeId
                                                                            })
                                                                            .ToList();

                            return Ok(dashboardCampaignResponseDTO);
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }



        [HttpPost("activities")]
        public async Task<IActionResult> Activities([FromBody] DashboardActivityRequestDTO dashboardActivityRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var result = new ResultDTO();

                        var userId = this.User.Identity.GetUserId();

                        return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get activities."));

                        var dashboardActivityResponseDTOs = await _appDatabaseContext
                                    .Contributions
                                    .Include(i => i.Campaign)
                                    .Include(i => i.User)
                                    .Where(i => i.CampaignId == dashboardActivityRequestDTO.CampaignId)
                                    .OrderByDescending(i => i.CreatedAt)
                                    .Select(i => new DashboardActivityResponseDTO
                                    {
                                        Id = i.Id,
                                        Icon = "heroicons_solid:user",
                                        UserAliasId = !string.IsNullOrWhiteSpace(i.User.PictureURL) ? i.User.AliasId.ToString() : "",
                                        Image = "",
                                        Description = $"<strong>{i.User.FullName}</strong> contributed <strong>{i.Amount.ToString("C")}</strong>",
                                        Date = i.CreatedAt,
                                        ExtraContent = "",
                                        LinkedContent = "",
                                        Link = "",
                                        UseRouter = null
                                    })
                                    .AsNoTracking()
                                    .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                    .ToListAsync();


                        result.Status = ResultDTOStatuses.Success;

                        result.Data = dashboardActivityResponseDTOs;

                        return Ok(result);
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to get activities."));

            }
        }

    }
}