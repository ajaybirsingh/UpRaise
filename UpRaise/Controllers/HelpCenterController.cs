using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UpRaise.DTOs;
using UpRaise.Entities;
using UpRaise.Helpers;
using UpRaise.Models;
using UpRaise.Services;
using UpRaise.Services.EF;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/help-center")]
    public class HelpCenterController : Controller
    {
        private readonly ILogger<HelpCenterController> _logger;
        private readonly IUserService _userService;
        private readonly SignInManager<IDUser> _signInManager;
        private readonly IMapper _mapper;

        private readonly AppDatabaseContext _appDatabaseContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Entities.IDUser> _userManager;
        private readonly EmailHelper _emailHelper;

        public HelpCenterController(
            ILogger<HelpCenterController> logger,
            IUserService userService,
            IMapper mapper,

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
            _appDatabaseContext = appDatabaseContext;
            _configuration = configuration;
            _userManager = userManager;
            _emailHelper = emailHelper;
            //IUserStore<Entities.IdentityUser> store = null;
        }

        [HttpPost("support")]
        public async Task<IActionResult> Support([FromBody] SupportRequestDTO supportRequestDTO)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var user = await _userManager.GetUserAsync(User);

                        var fromUser = $"{user.FirstName} {user.LastName} <{user.UserName}>";
                        var ret = await _emailHelper.SendSupportEmailAsync(fromUser, supportRequestDTO.Subject, supportRequestDTO.Message);

                        var result = new ResultDTO();

                        if (ret)
                        {
                            result.Status = ResultDTOStatuses.Success;
                            result.Message = $"Support email sent.";
                        }
                        else
                        {
                            result.Status = ResultDTOStatuses.Error;
                            result.Message = $"Failed sending support email.";
                        }

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




    }
}