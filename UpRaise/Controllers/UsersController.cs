using AutoMapper;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Models.Enums;
using UpRaise.Services;
using UpRaise.Services.EF;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IUserService _userService;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly IMapper _mapper;
        //private readonly AppSettings _appSettings;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Entities.IDUser> _userManager;
        private readonly EmailHelper _emailHelper;

        public UsersController(
            ILogger<UsersController> logger,
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


        [HttpGet("user")]
        public async Task<IActionResult> RefreshUser()
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


        [HttpPost("editProfile")]
        public async Task<IActionResult> EditProfile([FromBody] UserEditProfileDTO userEditProfileDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var user = await _userManager.GetUserAsync(this.User);

                            user.FirstName = userEditProfileDTO.FirstName;
                            user.LastName = userEditProfileDTO.LastName;

                            var identityResult = await _userManager.UpdateAsync(user);

                            var resultDTO = new ResultDTO();

                            if (identityResult.Succeeded)
                            {
                                resultDTO.Status = ResultDTOStatuses.Success;

                                var tokenHelper = new Helpers.TokenHelper();
                                var userDTO = await tokenHelper.GetLoginUserDTO(_appDatabaseContext, user);

                                resultDTO.Data = userDTO;
                            }
                            else
                                resultDTO.Status = ResultDTOStatuses.Error;

                            return Ok(resultDTO);

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

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] UserEditPasswordDTO userEditPasswordDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var user = await _userManager.GetUserAsync(this.User);
                            var result = await _userManager.ChangePasswordAsync(user, userEditPasswordDTO.OldPassword, userEditPasswordDTO.NewPassword);

                            var resultDTO = new ResultDTO();

                            if (result.Succeeded)
                                resultDTO.Status = ResultDTOStatuses.Success;
                            else
                            {
                                //var exceptionText = result.Errors.Aggregate("User Creation Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error.Code + " -- " + error.Description + "\n\r"));
                                //wrappedLogger.LogInformation(exceptionText);

                                var resultErrors = result.Errors.Select(e => e.Description);
                                resultDTO.Message = "Error settings password";
                                resultDTO.Data = resultErrors;
                                resultDTO.Status = ResultDTOStatuses.Error;
                            }

                            return Ok(resultDTO);
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

        [HttpPost("sendMessage")]
        [AllowAnonymous]
        public async Task<IActionResult> SendMessage([FromBody] UserMessageDTO userMessageDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var resultDTO = new ResultDTO();

                        var userMessage = new UserMessage();

                        userMessage.CampaignId = userMessageDTO.CampaignId;

                        var toUserAliasId = new Guid(userMessageDTO.ToUserAliasId);
                        var toUserId = await _userService.GetUserIdByAliasIdAsync(toUserAliasId);
                        if (toUserId.HasValue)
                        {
                            userMessage.ToUserId = toUserId.Value;

                            var fromUserId = this.User.Identity.GetUserId();
                            if (fromUserId.HasValue)
                            {
                                var fromUser = await _userService.GetByIdAsync(fromUserId.Value);
                                userMessage.FromUser = fromUser;
                            }

                            userMessage.FromFirstName = userMessageDTO.FirstName;
                            userMessage.FromLastName = userMessageDTO.LastName;
                            userMessage.FromPhone = userMessageDTO.Phone;
                            userMessage.FromEmail = userMessageDTO.Email;


                            userMessage.FromSubject = userMessageDTO.Subject;
                            userMessage.FromMessage = userMessageDTO.Message;

                            userMessage.StatusId = UserMessagesStatuses.InBox;
                            userMessage.Unread = true;

                            userMessage.IPAddress = this.Request.HttpContext.Connection.RemoteIpAddress.GetAddressBytes();

                            await _appDatabaseContext.UserMessages.AddAsync(userMessage);
                            await _appDatabaseContext.SaveChangesAsync();


                            await _emailHelper.SendContactMessageAsync(userMessage);

                            resultDTO.Status = ResultDTOStatuses.Success;
                        }
                        else
                        {
                            wrappedLogger.LogError($"Unable to find user associated with alias id {userMessageDTO.ToUserAliasId}");

                            resultDTO.Status = ResultDTOStatuses.Error;
                            resultDTO.Message = $"Unable to send message";
                        }

                        return Ok(resultDTO);
                    }
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
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {

                            var userId = this.User.Identity.GetUserId();

                            var notifications = await _appDatabaseContext
                                     .Users
                                     .AsNoTracking()
                                     .Select(i => new
                                     {
                                         Id = i.Id,
                                         NotificationOnCampaignDonations = i.NotificationOnCampaignDonations,
                                         NotificationOnCampaignFollows = i.NotificationOnCampaignFollows,
                                         NotificationOnUpraiseEvents = i.NotificationOnUpraiseEvents,
                                     })
                                     .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(10))
                                     .FirstOrDefaultAsync(i => i.Id == userId.Value);

                            var resultDTO = new ResultDTO();

                            if (notifications != null)
                            {
                                resultDTO.Status = ResultDTOStatuses.Success;
                                resultDTO.Data = new
                                {
                                    NotificationOnCampaignDonations = notifications.NotificationOnCampaignDonations,
                                    NotificationOnCampaignFollows = notifications.NotificationOnCampaignFollows,
                                    NotificationOnUpraiseEvents = notifications.NotificationOnUpraiseEvents
                                };

                            }
                            else
                                resultDTO.Status = ResultDTOStatuses.Error;

                            return Ok(resultDTO);
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


        [HttpPost("notifications")]
        public async Task<IActionResult> Notifications([FromBody] UserNotificationsDTO userNotifications)
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

                            var user = _appDatabaseContext.Users.FirstOrDefault(i => i.Id == userId.Value);

                            if (user != null)
                            {
                                user.NotificationOnCampaignDonations = userNotifications.NotificationOnCampaignDonations;
                                user.NotificationOnCampaignFollows = userNotifications.NotificationOnCampaignFollows;
                                user.NotificationOnUpraiseEvents = userNotifications.NotificationOnUpraiseEvents;

                                int numChanges = await _appDatabaseContext.SaveChangesAsync();

                                var resultDTO = new ResultDTO();
                                resultDTO.Status = ResultDTOStatuses.Success;
                                return Ok(resultDTO);
                            }
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



        [HttpGet("personalinformation")]
        public async Task<IActionResult> PersonalInformation()
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

                            var user = await _appDatabaseContext
                                     .Users
                                     .AsNoTracking()
                                     .Select(i => new
                                     {
                                         Id = i.Id,

                                         FirstName = i.FirstName,
                                         LastName = i.LastName,
                                         Country = i.AddressCountry,
                                         StreetAddress = i.AddressStreet,
                                         City = i.AddressCity,
                                         StateProvince = i.AddressStateProvince,
                                         ZipPostal = i.AddressZipPostal,
                                         DefaultCurrencyId = i.DefaultCurrencyId

                                     })
                                     .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(10))
                                     .FirstOrDefaultAsync(i => i.Id == userId.Value);

                            var resultDTO = new ResultDTO();

                            if (user != null)
                            {
                                resultDTO.Status = ResultDTOStatuses.Success;
                                resultDTO.Data = new
                                {
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    Country = user.Country,
                                    StreetAddress = user.StreetAddress,
                                    City = user.City,
                                    StateProvince = user.StateProvince,
                                    ZipPostal = user.ZipPostal,
                                    DefaultCurrencyId = user.DefaultCurrencyId
                                };

                            }
                            else
                                resultDTO.Status = ResultDTOStatuses.Error;

                            return Ok(resultDTO);
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


        [HttpPost("personalinformation")]
        public async Task<IActionResult> PersonalInformation([FromBody] UserPersonalInformationDTO userPersonalInformation)
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

                            var user = _appDatabaseContext.Users.FirstOrDefault(i => i.Id == userId.Value);

                            if (user != null)
                            {
                                user.FirstName = userPersonalInformation.FirstName;
                                user.LastName = userPersonalInformation.LastName;
                                user.AddressCountry = userPersonalInformation.Country;
                                user.AddressStreet = userPersonalInformation.StreetAddress;
                                user.AddressCity = userPersonalInformation.City;
                                user.AddressStateProvince = userPersonalInformation.StateProvince;
                                user.AddressZipPostal = userPersonalInformation.ZipPostal;
                                user.DefaultCurrencyId = (Currencies)userPersonalInformation.DefaultCurrencyId;

                                int numChanges = await _appDatabaseContext.SaveChangesAsync();

                                var resultDTO = new ResultDTO();
                                resultDTO.Status = ResultDTOStatuses.Success;
                                return Ok(resultDTO);
                            }
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



    }
}