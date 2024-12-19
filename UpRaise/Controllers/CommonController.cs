using AutoMapper;
using UpRaise.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Helpers;
using UpRaise.Models.Enums;
using UpRaise.Services;
using UpRaise.Services.EF;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/common")]
    public class CommonController : Controller
    {
        private readonly ILogger<CommonController> _logger;
        private readonly IUserService _userService;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly IMapper _mapper;
        //private readonly AppSettings _appSettings;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Entities.IDUser> _userManager;
        private readonly EmailHelper _emailHelper;

        public CommonController(
            ILogger<CommonController> logger,
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

        [HttpGet("messages")]
        public async Task<IActionResult> Messages()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    return Ok();
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpGet("navigation")]
        public async Task<IActionResult> Navigation()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var navigationDTOs = new List<NavigationDTO>();

                    //
                    //Dashboard
                    //
                    var dashboardMenu = new NavigationDTO() { Id = "dashboards", Title = "Dashboards", Subtitle = "Quick Analytics On Your Data", Type = NavigationDTO.Types.group.ToString(), Icon = "heroicons_outline:home" };
                    dashboardMenu.Children.Add(new NavigationDTO() { Id = "dashboards-campaign", Title = "Campaign", Type = NavigationDTO.Types.basic.ToString(), Icon = "heroicons_outline:chart-pie", Link = "/dashboards/campaign" });
                    navigationDTOs.Add(dashboardMenu);


                    //
                    //Funding
                    //
                    var fundingdMenu = new NavigationDTO() { Id = "funding", Title = "Funding", Subtitle = "Manage your funding data", Type = NavigationDTO.Types.group.ToString(), Icon = "heroicons_outline:currency-dollar" };
                    //fundingdMenu.Children.Add(new NavigationDTO() { Id = "funding.campaign", Title = "Campaign", Type = NavigationDTO.Types.basic.ToString(), Icon = "heroicons_outline:sparkles", Link = "/funding/campaign"});
                    fundingdMenu.Children.Add(new NavigationDTO() { Id = "funding.home", Title = "All Public Campaigns", Type = NavigationDTO.Types.basic.ToString(), Icon = "heroicons_outline:sparkles", Link = "/home" });
                    fundingdMenu.Children.Add(new NavigationDTO() { Id = "funding.new-campaign", Title = "Create A Campaign", Type = NavigationDTO.Types.basic.ToString(), Icon = "heroicons_outline:sparkles", Link = "/funding/new-campaign" });
                    fundingdMenu.Children.Add(new NavigationDTO() { Id = "funding.your-campaigns", Title = "Your Campaigns", Type = NavigationDTO.Types.basic.ToString(), Icon = "heroicons_outline:table", Link = "/funding/your-campaigns" });

                    navigationDTOs.Add(fundingdMenu);


                    //
                    //User
                    //
                    var userMenu = new NavigationDTO() { Id = "user", Title = "User", Subtitle = "User Settings", Type = NavigationDTO.Types.group.ToString(), Icon = "heroicons_outline:user-circle" };
                    /*
                    userMenu.Children.Add(new NavigationDTO()
                    {
                        Id = "user.messages",
                        Title = "Messages",
                        Type = NavigationDTO.Types.basic.ToString(),
                        Icon = "heroicons_outline:mail",
                        Link = "/user/messages",
                    });

                    userMenu.Children.Add(new NavigationDTO()
                    {
                        Id = "user.notifications",
                        Title = "System Notifications",
                        Type = NavigationDTO.Types.basic.ToString(),
                        Icon = "heroicons_outline:bell",
                        Link = "/user/notifications",
                    });
                    */

                    userMenu.Children.Add(new NavigationDTO()
                    {
                        Id = "user.settings",
                        Title = "Profile",
                        Type = NavigationDTO.Types.basic.ToString(),
                        Icon = "heroicons_outline:cog",
                        Link = "/user/profile",
                    });


                    navigationDTOs.Add(userMenu);



                    //
                    //System
                    //
                    var helpCenterMenu = new NavigationDTO() { Id = "help-center", Title = "Help Center", Type = NavigationDTO.Types.group.ToString(), Icon = "heroicons_outline:home" };
                    /*
                 helpCenterMenu.Children.Add(new NavigationDTO()
                 {
                     Id = "help-center.help",
                     Title = "Help",
                     Type = NavigationDTO.Types.basic.ToString(),
                     Icon = "heroicons_outline:question-mark-circle",
                     Link = "/help-center",
                 });

                 helpCenterMenu.Children.Add(new NavigationDTO()
                 {
                     Id = "help-center.support",
                     Title = "FAQs",
                     Type = NavigationDTO.Types.basic.ToString(),
                     Icon = "heroicons_outline:support",
                     Link = "/help-center/faqs",
                 });


                 helpCenterMenu.Children.Add(new NavigationDTO()
                 {
                     Id = "help-center.guides",
                     Title = "Guides",
                     Type = NavigationDTO.Types.basic.ToString(),
                     Icon = "heroicons_outline:book-open",
                     Link = "/help-center/guides",
                 });
                 */

                    helpCenterMenu.Children.Add(new NavigationDTO()
                    {
                        Id = "help-center.support",
                        Title = "Support",
                        Type = NavigationDTO.Types.basic.ToString(),
                        Icon = "heroicons_outline:support",
                        Link = "/help-center/support",
                    });


                    helpCenterMenu.Children.Add(new NavigationDTO()
                    {
                        Id = "help-center.changelog",
                        Title = "Changelog",
                        Type = NavigationDTO.Types.basic.ToString(),
                        Icon = "heroicons_outline:speakerphone",
                        Link = "/help-center/changelog",
                        Badge = new NavigationBadgeDTO()
                        {
                            Title = "1.0.1",
                            Classes = "px-2 bg-yellow-300 text-black rounded-full"
                        }
                    });

                    navigationDTOs.Add(helpCenterMenu);


                    return Ok(navigationDTOs);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> Notifications()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    return Ok();
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpGet("shortcuts")]
        public async Task<IActionResult> Shortcuts()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    return Ok();
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> UserData()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var tokenHelper = new Helpers.TokenHelper();
                    var user = await _userManager.GetUserAsync(this.User);
                    var userDTO = await tokenHelper.GetLoginUserDTO(_appDatabaseContext, user);

                    return Ok(userDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }


        [HttpPost("getEnums")]
        public IActionResult GetEnums([FromBody] EnumTypes enumType)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var resultDTO = new ResultDTO();
                    resultDTO.Status = ResultDTOStatuses.Success;

                    var enumDTOs = new List<EnumDTO>();

                    Array enums = null;
                    switch (enumType)
                    {
                        case EnumTypes.Countries:
                            enums = Enum.GetValues(typeof(Countries));
                            break;

                        case EnumTypes.Currencies:
                            enums = Enum.GetValues(typeof(Currencies));
                            break;

                        case EnumTypes.OrganizationCampaignCategories:
                            enums = Enum.GetValues(typeof(OrganizationCampaignCategories));
                            break;

                        default:
                            resultDTO.Status = ResultDTOStatuses.Error;
                            break;
                    }

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



    }
}