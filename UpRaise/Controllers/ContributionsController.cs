using AutoMapper;
using BitPaySDK;
using AutoMapper.QueryableExtensions;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Stripe;
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
using Newtonsoft.Json;
using UpRaise.Models.BitPay;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/contributions")]
    public class ContributionsController : Controller
    {
        private readonly ILogger<ContributionsController> _logger;
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


        public ContributionsController(
            ILogger<ContributionsController> logger,
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



        [HttpPost("stripecreate")]
        public async Task<IActionResult> StripeCreate([FromBody] StripeCreateRequestDTO stripeCreateRequest)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            var paymentIntentService = new PaymentIntentService();

                            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                            {
                                Amount = (long)(stripeCreateRequest.Amount * 100),
                                Currency = "usd",
                                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                                {
                                    Enabled = true,
                                },

                            });


                            paymentIntent.Metadata["CampaignId"] = stripeCreateRequest.CampaignId.ToString();
                            paymentIntent.Metadata["UserId"] = this.User.Identity.GetUserId().Value.ToString();

                            var stripeCreateResponse = new StripeCreateResponseDTO();
                            stripeCreateResponse.ClientSecret = paymentIntent.ClientSecret;

                            return Ok(new ResultDTO(ResultDTOStatuses.Success, stripeCreateResponse));
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to create stripe payment."));
            }
        }



        [HttpPost("cryptocreate")]
        public async Task<IActionResult> CryptoCreate([FromBody] CryptoCreateRequestDTO cryptoCreateRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {

                            var bitPay = new BitPay(Env.Test, @"bitpay_private_test.key", new Env.Tokens() { Merchant = "ECR1s6bd9GTf5gTsqEKxZy5z74NM25u6WV4xkKPMywnn" });
                            //var pairing = bitPay.RequestClientAuthorization(Facade.Merchant);

                            cryptoCreateRequestDTO.UserId = this.User.Identity.GetUserId().Value;

                            var cryptoCreateRequestData = JsonConvert.SerializeObject(cryptoCreateRequestDTO);

                            var invoice = await bitPay.CreateInvoice(new BitPaySDK.Models.Invoice.Invoice()
                            {
                                Price = cryptoCreateRequestDTO.Amount,
                                Currency = "USD",
                                OrderId = cryptoCreateRequestDTO.CampaignId.ToString(),
                                PosData = cryptoCreateRequestData,
                                RedirectUrl = $"https://app.upraise.fund/home/detail/{cryptoCreateRequestDTO.CampaignType.ToString()}/{cryptoCreateRequestDTO.CampaignId.ToString()}",
                                NotificationUrl = $"https://app.upraise.fund/api/contributions/cryptonotification",
                                //NotificationUrl = $"https://hookb.in/zrEOWk271MUol3MMlx0O",
                                FullNotifications = true,
                                ExtendedNotifications = true
                            });


                            var cryptoCreateResponseDTO = new CryptoCreateResponseDTO();
                            cryptoCreateResponseDTO.url = invoice.Url;

                            return Ok(new ResultDTO(ResultDTOStatuses.Success, cryptoCreateResponseDTO));
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex.Message);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to create crypto payment."));
            }
        }

        [HttpPost("cryptonotification")]
        [AllowAnonymous]
        public async Task<IActionResult> CryptoNotification([FromBody] InvoicePaymentNotification invoicePaymentNotification)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        wrappedLogger.LogInformation($"Received Crypto Notification - {invoicePaymentNotification.Event.Code} :: {invoicePaymentNotification.Event.Name}");

                        if (invoicePaymentNotification.Event.Code == 1005 /*invoice_confirmed code*/ ||
                            invoicePaymentNotification.Event.Code == 1006 /*invoice_completed code*/ )
                        {

                            //
                            //We never trust what was sent to this API as it could have been faked.  So we retrieve the latest status info from bitpay
                            //
                            var bitPay = new BitPay(Env.Test, @"bitpay_private_test.key", new Env.Tokens() { Merchant = "ECR1s6bd9GTf5gTsqEKxZy5z74NM25u6WV4xkKPMywnn" });
                            if (bitPay != null)
                            {
                                var invoice = await bitPay.GetInvoice(invoicePaymentNotification.Data.Id, Facade.Merchant, true);
                                if (invoice != null)
                                {
                                    wrappedLogger.LogInformation($"Invoice Status - {invoice.Status}");

                                    if (invoice.Status == "confirmed" ||
                                        invoice.Status == "complete")
                                    {
                                        var contributionRequest = JsonConvert.DeserializeObject<CryptoCreateRequestDTO>(invoice.PosData);

                                        var contribution = new Contribution();

                                        contribution.CampaignId = contributionRequest.CampaignId;
                                        contribution.UserId = contributionRequest.UserId;
                                        contribution.ContributionTypeId = ContributionTypes.Crypto;
                                        contribution.Amount = (decimal)contributionRequest.Amount;
                                        contribution.Note = contributionRequest.Comment;

                                        await _appDatabaseContext.AddAsync(contribution);
                                        int numChanges = await _appDatabaseContext.SaveChangesAsync();
                                    }
                                }

                                return Ok();
                            }
                            else
                                wrappedLogger.LogError($"Unable to create bitpay client");
                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to create stripe payment."));
            }
        }


        [HttpPost("contribution")]
        public async Task<IActionResult> Contribution([FromBody] ContributionRequestDTO contributionRequest)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        if (this.User != null)
                        {
                            bool saveContributionData = false;

                            switch (contributionRequest.ContributionType)
                            {
                                case ContributionTypes.Crypto:
                                    {
                                        //var invoice = 
                                    }
                                    break;

                                case ContributionTypes.CreditCard:
                                    //saveContributionData = true;
                                    break;

                                case ContributionTypes.ETransfer:
                                    break;

                                case ContributionTypes.GooglePay:
                                    break;
                            }


                            if (saveContributionData)
                            {
                                var contribution = new Contribution();

                                contribution.CampaignId = contributionRequest.CampaignId;
                                contribution.UserId = this.User.Identity.GetUserId().Value;
                                contribution.ContributionTypeId = contributionRequest.ContributionType;
                                contribution.Amount = (decimal)contributionRequest.Amount;
                                contribution.Note = contributionRequest.Comment;

                                await _appDatabaseContext.AddAsync(contribution);
                                int numChanges = await _appDatabaseContext.SaveChangesAsync();

                                return Ok(new ResultDTO(ResultDTOStatuses.Success, ""));
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return Ok(new ResultDTO(ResultDTOStatuses.Error, "Unable to save contribution."));
            }
        }



    }
}