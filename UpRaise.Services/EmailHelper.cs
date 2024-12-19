using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpRaise.Entities;
using UpRaise.Extensions;
using UpRaise.Helpers;
using UpRaise.Models;
using UpRaise.Services.EF;

namespace UpRaise.Services
{
    //
    //https://emojipedia.org/key/ - url for emoji icons
    //
    public class EmailHelper
    {
        private readonly ILogger<EmailHelper> _log = null;
        private readonly AppDatabaseContext _dbContext;
        private readonly IEmail _email;
        private readonly SESSettings _sesSettings;

        public EmailHelper(
            ILogger<EmailHelper> log,
            AppDatabaseContext dbContext,
            IEmail email,
            IOptions<SESSettings> sesSettings
            )
        {
            _log = log;
            _dbContext = dbContext;
            _sesSettings = sesSettings.Value;

            _email = email;

        }


        public async Task<bool> SendSupportEmailAsync(string fromAddress, string subject, string message)
        {
            try
            {
                await _email.SendEmailAsync(_sesSettings.support_email, $"🆘 UpRaise support request from {fromAddress}: {subject}", message, isHighPriority: true);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return false;
        }



        public async Task<bool> SendForgotPasswordEmailAsync(string emailAddress, string resetLink)
        {
            try
            {
                await _email.SendEmailAsync(emailAddress, $"🔑 UpRaise password reset", $"Here is your password reset link (valid for 24 hours) <a href={resetLink}>click here</a>. <br/><br/>If you are receiving this e-mail in error please ignore.", isHighPriority: true);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return false;
        }


        public async Task<bool> SendConfirmationEmailAsync(string emailAddress, string confirmLink)
        {
            try
            {
                await _email.SendEmailAsync(emailAddress, $"📩 UpRaise email confirmation", $"Here is your email confirmation link (valid for 24 hours) <a href={confirmLink}>click here</a>", isHighPriority: true);
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return false;
        }



        public async Task<bool> SendWelcomeEmailAsync(int toUserId)
        {
            try
            {
                var user = _dbContext.Users
                    .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                    .FirstOrDefault(i => i.Id == toUserId);
                if (user != null)
                {
                    //
                    //email user
                    //
                    await _email.SendEmailAsync(user.UserName, $"📧 Welcome to UpRaise!", "Thank you for registering!");

                    return true;
                }
                else
                {
                    _log.LogWarning($"Could not find user {toUserId}");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return false;
        }

        public async Task<bool> SendContactMessageAsync(UserMessage userMessage)
        {
            using (var wrappedLogger = new WrappedLogger(_log))
            {
                try
                {
                    var toUser = _dbContext.Users
                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                        .FirstOrDefault(i => i.Id == userMessage.ToUserId);

                    if (toUser != null)
                    {

                        StringBuilder subject = new StringBuilder();

                        if (userMessage.FromUser != null)
                        {
                            subject.Append($"{userMessage.FromUser.FirstName} {userMessage.FromUser.LastName} ({userMessage.FromUser.UserName}) has sent a message.");
                        }
                        else
                        {
                            subject.Append($"{userMessage.FromFirstName} {userMessage.FromLastName} ({userMessage.FromEmail}) has sent a message.");
                        }

                        var message = new StringBuilder();
                        message.Append($"A comment was received:");
                        message.Append($"<br/><br/><br/>");
                        message.Append($"Subject: \"{userMessage.FromSubject}\"");
                        message.Append($"<br/><br/>");
                        message.Append($"Message: \"{userMessage.FromMessage}\"");


                        await _email.SendEmailAsync(toUser.UserName, $"📮 {subject.ToString()}", $"{message.ToString()}");

                        return true;
                    }
                    else
                    {
                        wrappedLogger.LogError($"Could not find user {userMessage.ToUserId}");
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex.Message);
                }

                return false;
            }
        }

        public async Task<bool> SendNewsletterSubscriptionEmailAsync(string emailAddress)
        {
            try
            {
                //
                //email user
                //
                await _email.SendEmailAsync(emailAddress, $"📧 Subscribed to UpRaise newsletter!", "Thank you for subscribing to our newsletter.");

                return true;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }

            return false;
        }


        public async Task<bool> SendBeneficiaryEmailAsync(int campaignId, HttpContext httpContext)
        {
            using (var wrappedLogger = new WrappedLogger(_log))
            {
                try
                {
                    var campaign = await _dbContext.Campaigns
                        .AsNoTracking()
                        .Select(i => new
                        {
                            i.Id,
                            i.TypeId,
                            i.TransactionId,
                            i.Name,
                        })
                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                        .FirstOrDefaultAsync(i => i.Id == campaignId);

                    if (campaign != null)
                    {
                        var subject = new StringBuilder();
                        var message = new StringBuilder();

                        var beneficiaryEmail = new StringBuilder();

                        switch (campaign.TypeId)
                        {
                            case Models.Enums.CampaignTypes.Organization:
                                {
                                    subject.Append($"Organization Campaign '{campaign.Name}'");
                                }
                                break;

                            case Models.Enums.CampaignTypes.People:
                                {
                                    subject.Append($"People Campaign '{campaign.Name}'");

                                    var campaignDraftLink = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}/campaign-redline/{campaign.TransactionId}";
                                    message.Append($"See your campaign draft and accept or offer revisions - <a href={campaignDraftLink}>campaign</a>.");

                                    var peopleCampaign = await _dbContext.PeopleCampaigns
                                                    .AsNoTracking()
                                                    .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(5))
                                                    .FirstOrDefaultAsync(i => i.CampaignId == campaignId);

                                    if (string.IsNullOrWhiteSpace(peopleCampaign.BeneficiaryEmail))
                                    {
                                        wrappedLogger.LogWarning($"Could not find people beneficiary email for campaign {campaignId}");
                                        return false;
                                    }

                                    if (!peopleCampaign.BeneficiaryEmail.IsValidEmail())
                                    {
                                        wrappedLogger.LogWarning($"Beneficiary email '{peopleCampaign.BeneficiaryEmail}' is not a valid email address");
                                        return false;
                                    }

                                    if (peopleCampaign != null)
                                    {
                                        beneficiaryEmail.Append(!string.IsNullOrWhiteSpace(peopleCampaign.BeneficiaryName) ? $"{peopleCampaign.BeneficiaryName} <{peopleCampaign.BeneficiaryEmail}>" : peopleCampaign.BeneficiaryEmail);
                                    }
                                    else
                                        wrappedLogger.LogError($"Could not find people campaign {campaignId}");
                                }
                                break;
                        }

                        await _email.SendEmailAsync(beneficiaryEmail.ToString(), $"📮 {subject.ToString()}", $"{message.ToString()}");

                        return true;
                    }
                    else
                    {
                        wrappedLogger.LogError($"Could not find campaign {campaignId}");
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex.Message);
                }

                return false;
            }
        }


        /*
        public static List<string> GetHCSAAdminEmails(string _adminEmail)
        {
            var adminEmails = new List<string>();
            try
            {
                adminEmails.Add(_adminEmail);
            }
            catch (Exception ex)
            {
            }


            return adminEmails;
        }

        public static async Task<bool> SendDrafNotification(ApplicationDbContext _applicationDbContext, IEmailSender _emailSender, Claim claim)
        {
            try
            {
                //
                //email user
                //
                await _emailSender.SendEmailAsync(claim.ActiveDirectoryEmployeeEmailAddress, $"🕘 HCSA Claim Draft", "Your HCSA claim draft has been saved.  Please complete your claim at your convenience.");

                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }


        public static async Task<bool> SendSubmissionNotification(AppSettings _appSettings, ApplicationDbContext _applicationDbContext, IEmailSender _emailSender, Claim claim)
        {
            try
            {
                //
                //email user
                //
                await _emailSender.SendEmailAsync(claim.ActiveDirectoryEmployeeEmailAddress, $"📧 HCSA Claim Submission", "Thank you for your submission.  Your claim will be reviewed and an email will be sent when processed.");


                //
                //notify HR admins so they can approve/reject claim
                //
                var adminEmails = GetHCSAAdminEmails(_appSettings.HCSAAdminEmail);

                await _emailSender.SendEmailAsync(adminEmails, $"📧 {claim.ActiveDirectoryEmployeeName} Submitted HCSA Claim", $"{claim.ActiveDirectoryEmployeeName} has submitted a HCSA claim for ${claim.TotalAmountSubmitted.ToString("F2")}.  Please log in to review claim.");

                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public static async Task<bool> SendProcessNotification(ApplicationDbContext _applicationDbContext, IEmailSender _emailSender, Claim claim, IServiceProvider _IServiceProvider)
        {
            try
            {
                //
                //email user
                //
                var content = new StringBuilder();

                content.Append($"Dear {claim.ActiveDirectoryEmployeeName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]},<br><br>");

                content.Append($"Your most recent HCSA claim has been processed.  Please see below for description of reimbursements:<br>");


                //                of ${ claim.TotalAmountSubmitted.ToString("F2")}
                //has been processed and you will be paid ${ claim.TotalAmountApproved.ToString("F2")}
                //on the next pay deposit.");

                //content.Append($"Your most recent HCSA claim of ${claim.TotalAmountSubmitted.ToString("F2")} has been processed and you will be paid ${claim.TotalAmountApproved.ToString("F2")} on the next pay deposit.");

                try
                {
                    if (claim.ClaimDetails.Any())
                        content.AppendLine("<br>");

                    int idx = 1;
                    foreach (var claimDetail in claim.ClaimDetails.OrderByDescending(i => i.ServiceDate))
                    {
                        content.Append($"{idx++}. Your <b>{claimDetail.TaxableNontaxableItem.Type}</b> claim for <b>{claimDetail.TaxableNontaxableItem.Description}");

                        content.Append($"</b> in the amount of <b>${ claimDetail.AmountSubmitted.ToString("F2")}</b> has been ");

                        if (claimDetail.IsApproved)
                            content.Append($"<b>approved</b> for <b>${ claimDetail.AmountApproved.ToString("F2")}</b>.");
                        else
                            content.Append($"<b>declined</b>.");

                        if (!string.IsNullOrEmpty(claimDetail.EmailComment))
                            content.Append($" Administrator comments: <b>'{claimDetail.EmailComment.Trim('.')}'</b>.");
                        else
                            content.Append($" Administrator comments: <b>None</b>.");

                        content.AppendLine("<br>");
                        content.AppendLine("<br>");
                    }


                    var _activeDirectoryHelper = _IServiceProvider.GetService<ActiveDirectoryHelper>();
                    var employeeRole = _activeDirectoryHelper.GetEmployeeRole(claim.ActiveDirectoryEmployeeId);
                    if (!string.IsNullOrWhiteSpace(employeeRole))
                    {
                        var year = DateTime.UtcNow.Year;

                        var roleConfig = _activeDirectoryHelper.GetRoleConfig(_applicationDbContext, employeeRole);
                        if (roleConfig != null)
                        {
                            content.Append($"Total HCSA allotment for {year}: ${roleConfig.YearlyAmount.ToString("F2")}<br>");

                            var yearlyUsed = _activeDirectoryHelper.GetRoleYearlyUsed(_applicationDbContext, claim.ActiveDirectoryEmployeeId, year);
                            if (yearlyUsed.HasValue)
                            {
                                content.Append($"Total HCSA utilized: ${yearlyUsed.Value.ToString("F2")}<br>");

                                var remaining = roleConfig.YearlyAmount - yearlyUsed.Value;
                                content.Append($"HCSA remaining for {year}: ${remaining.ToString("F2")}<br>");
                            }
                        }
                    }

                }
                catch (Exception)
                {

                }

                await _emailSender.SendEmailAsync(claim.ActiveDirectoryEmployeeEmailAddress, $"✔️ Your HCSA claim has been processed", content.ToString());

                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }

    */
    }

}