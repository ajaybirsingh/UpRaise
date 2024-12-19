using AutoMapper;
using Google.Apis.Auth;
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
using System.Web;
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
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly IMapper _mapper;
        //private readonly AppSettings _appSettings;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Entities.IDUser> _userManager;
        private readonly EmailHelper _emailHelper;

        public AuthController(
            ILogger<AuthController> logger,
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
        
        [HttpPost("refresh-access-token")]
        public async Task<IActionResult> RefreshAccessToken([FromBody] RefreshAccessTokenRequestDTO refreshAccessTokenDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var user = await _userManager.GetUserAsync(User);

                        var tokenHelper = new Helpers.TokenHelper();
                        var tokenString = tokenHelper.GetToken(_configuration["AuthenticationSecret"], user);

                        var userDTO = await tokenHelper.GetLoginUserDTO(_appDatabaseContext, user);

                        var authResponse = new AuthResponseDTO();
                        authResponse.User = userDTO;
                        authResponse.Token = tokenString;

                        var result = new ResultDTO();
                        result.Status = ResultDTOStatuses.Success;
                        result.Message = $"Welcome {user.UserName}";
                        result.Data = authResponse;

                        return Ok(result);
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordDTO forgotPasswordDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var user = await _userManager.FindByNameAsync(forgotPasswordDTO.Username);

                        var resultDTO = new ResultDTO();
                        if (user != null)
                        {
                            wrappedLogger.LogInformation($"Forgot Password - {forgotPasswordDTO.Username}");

                            if (!await _signInManager.CanSignInAsync(user))
                            {
                                await SendConfirmationEmailAsync(user);

                                resultDTO = new ResultDTO
                                {
                                    Status = ResultDTOStatuses.Error,
                                    Message = "Please confirm your email first (confirmation email resent, please check your spam folder)",
                                };
                            }
                            else
                            {
                                var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                                var encodedUsername = user.UserName.EncodeToBase64();
                                var encodedPasswordResetToken = passwordResetToken.EncodeToBase64();

                                var urlDomain = $"{this.HttpContext.Request.Scheme}://{this.HttpContext.Request.Host.Value}";
                                var resetLink = $"{urlDomain}/reset-password?u={encodedUsername}&rt={encodedPasswordResetToken}";
                                await _emailHelper.SendForgotPasswordEmailAsync(user.UserName, resetLink);

                                resultDTO = new ResultDTO
                                {
                                    Status = ResultDTOStatuses.Success,
                                    Message = "Password reset email sent.",
                                };
                            }
                        }
                        else
                        {
                            resultDTO = new ResultDTO
                            {
                                Status = ResultDTOStatuses.Error,
                                Message = "Invalid Username",
                            };
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

        [AllowAnonymous]
        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody]LoginRequestDTO loginDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    //wrappedLogger.LogInformation($"Authenticating user");

                    if (ModelState.IsValid)
                    {

                        var user = await _userManager.FindByNameAsync(loginDTO.Username);

                        if (user != null)
                        {
                            wrappedLogger.LogInformation($"Sign In - {loginDTO.Username}");

                            var result = new ResultDTO();

                            if (await _userManager.CheckPasswordAsync(user, loginDTO.Password))
                            {
                                // Rule #1
                                if (!await _signInManager.CanSignInAsync(user))
                                {
                                    await SendConfirmationEmailAsync(user);

                                    result.Status = ResultDTOStatuses.Error;
                                    result.Message = "Email confirmation required";

                                    return Ok(result);
                                }

                                var signInResult = await _signInManager.PasswordSignInAsync(loginDTO.Username, loginDTO.Password, true, lockoutOnFailure: false);

                                if (signInResult.RequiresTwoFactor)
                                {
                                    result.Status = ResultDTOStatuses.Success;
                                    result.Message = "Enter the code generated by your authenticator app";
                                    result.Data = new { requires2FA = true };
                                    return Ok(result);
                                }


                                try
                                {
                                    //wrappedLogger.LogInformation($"Returning user info");

                                    var tokenHelper = new Helpers.TokenHelper();
                                    var tokenString = tokenHelper.GetToken(_configuration["AuthenticationSecret"], user);

                                    var userDTO = await tokenHelper.GetLoginUserDTO(_appDatabaseContext, user);

                                    var authResponse = new AuthResponseDTO();
                                    authResponse.User = userDTO;
                                    authResponse.Token = tokenString;

                                    result.Status = ResultDTOStatuses.Success;
                                    result.Message = $"Welcome {user.UserName}";
                                    result.Data = authResponse;

                                    return Ok(result);
                                }
                                catch (Exception ex)
                                {
                                    wrappedLogger.LogError(ex);
                                }

                                return BadRequest();
                            }
                        }

                        return Ok(new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Invalid Username or Password",
                        });
                    }

                    var errors = ModelState.Keys.Select(e => "<li>" + e + "</li>");
                    return Ok(new ResultDTO
                    {
                        Status = ResultDTOStatuses.Error,
                        Message = "Invalid data",
                        Data = string.Join("", errors)
                    });

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO
                {
                    Status = ResultDTOStatuses.Error,
                    Message = "Error signing in",
                });
            }
        }
        

        [AllowAnonymous]
        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpUserDTO userDto)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {

                try
                {

                    if (ModelState.IsValid)
                    {
                        //
                        //check if user exists
                        //
                        var userExists = await _userManager.FindByNameAsync(userDto.Username);
                        if (userExists != null)
                        {
                            return Ok(new ResultDTO
                            {
                                Status = ResultDTOStatuses.Error,
                                Message = "User already exists",
                            });
                        }

                        // map dto to entity
                        var user = _mapper.Map<Entities.IDUser>(userDto);

                        //user.CompanyId = 30; //companyid
                        user.Email = user.UserName;
                        user.AliasId = Guid.NewGuid();
                        user.DefaultCurrencyId = Currencies.USD;

                        // save 
                        var identityResult = await _userManager.CreateAsync(user, userDto.Password);
                        if (identityResult.Succeeded)
                        {
                            /*
                            if (model.StartFreeTrial)
                            {
                                Claim trialClaim = new Claim("Trial", DateTime.Now.ToString());
                                await _userManager.AddClaimAsync(user, trialClaim);
                            }
                            else if (model.IsAdmin)
                            {
                                await _userManager.AddToRoleAsync(user, "Admin");
                            }                         
                             */

                            await SendConfirmationEmailAsync(user);

                            var resultDTO = new ResultDTO()
                            {
                                Status = ResultDTOStatuses.Success,
                                Message = "Email confirmation is pending",
                            };

                            return Ok(resultDTO);
                        }
                        else
                        {
                            var exceptionText = identityResult.Errors.Aggregate("User Creation Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error.Code + " -- " + error.Description + "\n\r"));
                            wrappedLogger.LogInformation(exceptionText);

                            var resultErrors = identityResult.Errors.Select(e => e.Description);
                            var resultDTO = new ResultDTO
                            {
                                Status = ResultDTOStatuses.Error,
                                Message = "Invalid data",
                                Data = resultErrors
                            };
                            return Ok(resultDTO);
                        }
                    }
                    else
                    {
                        var errors = ModelState.Keys.Select(e => e );
                        return Ok(new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Invalid data",
                            Data = errors
                        });
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }
                return BadRequest();

            }
        }

        private async Task<bool> SendConfirmationEmailAsync(IDUser user)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var emailTokenUrlEncoded = emailToken.EncodeToBase64();

                    var aliasIdUrlEncoded = $"{user.AliasId.ToString().Replace("-", "")}";

                    var confirmEmailUrl = $"{this.HttpContext.Request.Scheme}://{this.HttpContext.Request.Host.Value}/api/auth/confirmemail/{aliasIdUrlEncoded}/{emailTokenUrlEncoded}";

                    wrappedLogger.LogInformation($"Sending email confirm url -- {confirmEmailUrl}");

                    await _emailHelper.SendConfirmationEmailAsync(user.Email, confirmEmailUrl);

                    return true;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return false;
            }
        }



        [AllowAnonymous]
        [HttpGet("confirmemail/{userId}/{emailToken}")]
        public async Task<IActionResult> ConfirmEmail(Guid userId, string emailToken)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var user = await _appDatabaseContext.Users.FirstOrDefaultAsync(i => i.AliasId == userId);
                    if (user != null)
                    {
                        wrappedLogger.LogInformation($"Received email token - {emailToken}");

                        var emailTokenUrlDecoded = emailToken.DecodeFromBase64();

                        var result = await _userManager.ConfirmEmailAsync(user, emailTokenUrlDecoded);

                        if (result.Succeeded)
                            return Redirect("/sign-in?t=1");
                    }
                    else
                    {
                        wrappedLogger.LogInformation($"Unable to find user alias id- {userId}");
                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Redirect("/sign-in");
            }
        }


        


        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody]ForgotPasswordResetDTO forgetPasswordResetDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var userName = forgetPasswordResetDTO.Username.DecodeFromBase64();

                        var user = await _userManager.FindByNameAsync(userName);
                        if (user != null)
                        {
                            var resetToken = forgetPasswordResetDTO.ResetToken.DecodeFromBase64();
                            var identityResult = await _userManager.ResetPasswordAsync(user, resetToken, forgetPasswordResetDTO.Password);

                            if (identityResult.Succeeded)
                            {
                                var resultDTO = new ResultDTO
                                {
                                    Status = ResultDTOStatuses.Success,
                                    Message = "",
                                };
                                return Ok(resultDTO);
                            }
                            else
                            {
                                //var exceptionText = identityResult.Errors.Aggregate("Pasword Reset Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error.Code + " -- " + error.Description + "\n\r"));
                                //wrappedLogger.LogInformation(exceptionText);
                                //return new BadRequestObjectResult(identityResult.Errors);

                                var resultErrors = identityResult.Errors.Select(e => e.Description);

                                var resultDTO = new ResultDTO
                                {
                                    Status = ResultDTOStatuses.Error,
                                    Message = "Invalid data",
                                    Data = resultErrors
                                };
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

        /*
        [Authorize(Roles = SecurityRoles.Admin)]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var userDtos = _mapper.Map<IList<UserDTO>>(users);
            return Ok(userDtos);
        }
        */

        /*
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            // only allow admins to access other user records
            var currentUserId = int.Parse(User.Identity.Name);
            //if (id != currentUserId && !User.IsInRole(SecurityRoles.Admin))
            {
                return Forbid();
            }

            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]UserDTO userDto)
        {
            // map dto to entity and set id
            var user = _mapper.Map<Entities.IDUser>(userDto);
            user.Id = id;

            try
            {
                // save 
                await _userService.UpdateAsync(user, userDto.Password);
                return Ok();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }
        */

        /*
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }
        */

        /*
        [HttpGet("[action]")]
        public JsonResult GetCompanyUsers([DataSourceRequest]DataSourceRequest request)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var companyId = this.User.Identity.GetCompanyId();
                    if (companyId.HasValue)
                    {
                        var users = _appDatabaseContext.Users
                        .AsNoTracking()
                        .Where(i => i.CompanyId == companyId.Value)
                        .OrderBy(i => i.NormalizedUserName)
                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                        .ProjectTo<UserCompanyDTO>(_mapper.ConfigurationProvider);

                        var jsonObjects = users.ToDataSourceResult(request);
                        return Json(jsonObjects);
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Json(new List<UserCompanyDTO>());
            }
        }



        [HttpGet("GetUserEditById/{id}")]
        public async Task<IActionResult> GetUserEditById(Guid id)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var user = await _appDatabaseContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.AliasId == id);

                    if (user == null)
                        return NotFound();

                    var userEditDTO = _mapper.Map<UserEditDTO>(user);

                    var resultDTO = new ResultDTO
                    {
                        Status = ResultDTOStatuses.Success,
                        Message = "",
                        Data = userEditDTO
                    };
                    return Ok(resultDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpPost()]
        public async Task<IActionResult> UpsertUserEdit([FromBody]UserEditDTO userEditDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var companyId = this.User.Identity.GetCompanyId();
                            if (companyId.HasValue)
                            {
                                /*
                                var company = await _appDatabaseContext
                                                               .Companies
                                                               .FirstOrDefaultAsync(i => i.Id == companyId.Value);
                                if (company != null)
                                {
                                    _mapper.Map(companyDTO, company);
                                    await _appDatabaseContext.SaveChangesAsync();
                                }
                                */
        /*
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
        */

        //https://medium.com/building-a-simple-text-correction-tool/third-party-authentication-with-angular-and-asp-net-core-web-api-bfa10d169e47
        private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleToken(string googleTokenId)
        {
            GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();
            settings.Audience = new List<string>() { "Google ClientId here!!" };
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(googleTokenId, settings);
            return payload;
        }

    }
}