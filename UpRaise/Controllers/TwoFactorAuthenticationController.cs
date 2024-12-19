using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Helpers;
using UpRaise.Services.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/2fa")]
    public class TwoFactorAuthenticationController : ControllerBase
    {
        private readonly UserManager<IDUser> _userManager;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly ILogger<TwoFactorAuthenticationController> _logger;
        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;

        public TwoFactorAuthenticationController(
            UserManager<IDUser> userManager,
            SignInManager<IDUser> signInManager,
            UrlEncoder urlEncoder,
            AppDatabaseContext appDatabaseContext,
            IConfiguration configuration,
            ILogger<TwoFactorAuthenticationController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
            _logger = logger;
            _appDatabaseContext = appDatabaseContext;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("details")]
        public async Task<IActionResult> Details()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    var logins = await _userManager.GetLoginsAsync(user);

                    var userSecurityDTO = new UserSecurityDTO
                    {
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumber = user.PhoneNumber,
                        ExternalLogins = logins.Select(login => login.ProviderDisplayName).ToList(),
                        TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                        HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                        TwoFactorClientRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
                    };

                    return Ok(userSecurityDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpGet("setupAuthenticator")]
        [Authorize]
        public async Task<IActionResult> SetupAuthenticator()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    var twoFactoryAuthAuthenticatorDetailDTO = await GetAuthenticatorDetailsAsync(user);

                    return Ok(twoFactoryAuthAuthenticatorDetailDTO);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }

        }


        [Authorize]
        [HttpPost("verifyAuthenticator")]
        public async Task<IActionResult> VerifyAuthenticator([FromBody] TwoFactorVerifyAuthenticatorRequestDTO verifyAuthenticator)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (!ModelState.IsValid)
                    {
                        var errors = GetErrors(ModelState).Select(e => "<li>" + e + "</li>");
                        return BadRequest(new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Invalid data",
                            Data = string.Join("", errors)
                        });
                    }

                    var verificationCode = verifyAuthenticator.VerificationCode.Replace(" ", string.Empty).Replace("-", string.Empty);

                    var is2FaTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                        user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

                    if (!is2FaTokenValid)
                    {
                        return Ok(new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Invalid data",
                            Data = "<li>Verification code is invalid.</li>"
                        });
                    }

                    await _userManager.SetTwoFactorEnabledAsync(user, true);

                    var result = new ResultDTO
                    {
                        Status = ResultDTOStatuses.Success,
                        Message = "Your authenticator app has been verified",
                    };

                    //if (await _userManager.CountRecoveryCodesAsync(user) != 0) return Ok(result);

                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                    result.Data = new { recoveryCodes };

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }

        }

        [Authorize]
        [HttpPost("resetAuthenticator")]
        public async Task<IActionResult> ResetAuthenticator()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {

                    var user = await _userManager.GetUserAsync(User);

                    await _userManager.SetTwoFactorEnabledAsync(user, false);
                    await _userManager.ResetAuthenticatorKeyAsync(user);

                    await _signInManager.RefreshSignInAsync(user);

                    var result = new ResultDTO
                    {
                        Status = ResultDTOStatuses.Success,
                        Message = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key."
                    };

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpPost("disable2FA")]
        [Authorize]
        public async Task<IActionResult> Disable2FA()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {

                    var user = await _userManager.GetUserAsync(User);

                    if (!await _userManager.GetTwoFactorEnabledAsync(user))
                    {
                        return BadRequest(new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Cannot disable 2FA as it's not currently enabled"
                        });
                    }

                    var result = await _userManager.SetTwoFactorEnabledAsync(user, false);

                    return Ok(new ResultDTO
                    {
                        Status = result.Succeeded ? ResultDTOStatuses.Success : ResultDTOStatuses.Error,
                        Message = result.Succeeded ? "2FA has been successfully disabled" : $"Failed to disable 2FA {result.Errors.FirstOrDefault()?.Description}"
                    });
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }

        [HttpPost("generateRecoveryCodes")]
        [Authorize]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {

                    var user = await _userManager.GetUserAsync(User);

                    var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

                    if (!isTwoFactorEnabled)
                    {
                        return BadRequest(new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Cannot generate recovery codes as you do not have 2FA enabled"
                        });
                    }

                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

                    return Ok(new ResultDTO
                    {
                        Status = ResultDTOStatuses.Success,
                        Message = "You have generated new recovery codes",
                        Data = new { recoveryCodes }
                    });
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return BadRequest();
            }
        }
        

        [AllowAnonymous]
        [HttpPost("loginwith2fa")]
        public async Task<IActionResult> LoginWith2FA([FromBody] TwoFactorCodeLoginRequestDTO model)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var user = await _userManager.FindByNameAsync(model.Username);
                        if (user != null)
                        {

                            if (model.UseRecoveryCode)
                            {
                                //
                                //Recovery Code
                                //
                                var twoFaLogin = await TwoFaLogin(model.TwoFactorCode, isRecoveryCode: true);

                                if (twoFaLogin.Status == ResultDTOStatuses.Success)
                                {
                                    var tokenHelper = new Helpers.TokenHelper();

                                    var authResponse = new AuthResponseDTO();

                                    authResponse.User = await tokenHelper.GetLoginUserDTO(_appDatabaseContext, user);
                                    authResponse.Token = tokenHelper.GetToken(_configuration["AuthenticationSecret"], user);

                                    twoFaLogin.Data = authResponse;
                                }

                                return Ok(twoFaLogin);
                            }
                            else
                            {
                                //
                                //Two Factor Code
                                //
                                var twoFaLogin = await TwoFaLogin(model.TwoFactorCode, isRecoveryCode: false, model.RememberMachine);

                                if (twoFaLogin.Status == ResultDTOStatuses.Success)
                                {
                                    var tokenHelper = new Helpers.TokenHelper();

                                    var authResponse = new AuthResponseDTO();

                                    authResponse.User = await tokenHelper.GetLoginUserDTO(_appDatabaseContext, user);
                                    authResponse.Token = tokenHelper.GetToken(_configuration["AuthenticationSecret"], user);

                                    twoFaLogin.Data = authResponse;
                                }

                                return Ok(twoFaLogin);
                            }
                        }

                        return BadRequest();
                    }

                    var errors = GetErrors(ModelState).Select(e => "<li>" + e + "</li>");
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

                return BadRequest();
            }
        }


        #region Private methods

        private async Task<ResultDTO> TwoFaLogin(string code, bool isRecoveryCode, bool rememberMachine = false)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    SignInResult result = null;

                    var authenticatorCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

                    if (!isRecoveryCode)
                    {
                        result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, true, rememberMachine);
                    }
                    else
                    {
                        result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(authenticatorCode);
                    }

                    if (result.Succeeded)
                    {
                        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                        if (user == null)
                        {
                            return new ResultDTO
                            {
                                Status = ResultDTOStatuses.Error,
                                Message = "Unable to load two-factor authentication user",
                                Data = "<li>Unable to load two-factor authentication user</li>"
                            };
                        }

                        return new ResultDTO
                        {
                            Status = ResultDTOStatuses.Success,
                            Message = $"Welcome {user.UserName}"
                        };
                    }
                    else if (result.IsLockedOut)
                    {
                        return new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = "Account locked out",
                            Data = "<li>Account locked out</li>"
                        };
                    }
                    else
                    {
                        return new ResultDTO
                        {
                            Status = ResultDTOStatuses.Error,
                            Message = $"Invalid {(isRecoveryCode ? "recovery" : "authenticator")} code",
                            Data = $"<li>Invalid {(isRecoveryCode ? "recovery" : "authenticator")} code</li>"
                        };
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }
                return null;
            }
        }

        private List<string> GetErrors(ModelStateDictionary modelState)
        {
            var errors = new List<string>();

            foreach (var state in modelState.Values)
            {
                foreach (var error in state.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }

            return errors;
        }


        private async Task<TwoFactorAuthAuthenticatorDetailDTO> GetAuthenticatorDetailsAsync(IDUser user)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    // Load the authenticator key & QR code URI to display on the form
                    var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                    if (string.IsNullOrEmpty(unformattedKey))
                    {
                        await _userManager.ResetAuthenticatorKeyAsync(user);
                        unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                    }

                    var email = await _userManager.GetEmailAsync(user);

                    var twoFactorAuthAuthenticatorDetailDTO = new TwoFactorAuthAuthenticatorDetailDTO
                    {
                        SharedKey = FormatKey(unformattedKey),
                        AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey)
                    };

                    return twoFactorAuthAuthenticatorDetailDTO;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return null;
            }

        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode(Helpers.Constants.AppName),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        #endregion

    }

}
