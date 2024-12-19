
using AutoMapper;
using EFCoreSecondLevelCacheInterceptor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UpRaise.Extensions;
using UpRaise.DTOs;
using UpRaise.Helpers;
using UpRaise.Services;
using UpRaise.Services.EF;

namespace UpRaise.Controllers
{
    [Authorize]
    [ApiController]
    [Produces("application/json")]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        //private readonly IUserService _userService;
        private readonly IMapper _mapper;
        //private readonly AppSettings _appSettings;
        private readonly IBlobManager _blobManager;
        private readonly AppDatabaseContext _appDatabaseContext;
        //private readonly CosmosDB _cosmosDB;

        public FileController(
            ILogger<FileController> logger,
            //IUserService userService,
            IMapper mapper,
            IBlobManager blobManager,
            //CosmosDB cosmosDB,
            AppDatabaseContext appDatabaseContext
            )
        {
            _logger = logger;
            //_userService = userService;
            _mapper = mapper;
            //_appSettings = appSettings.Value;
            _blobManager = blobManager;
            _appDatabaseContext = appDatabaseContext;
            //_cosmosDB = cosmosDB;
        }


        [HttpPost("upload")]
        async public Task<IActionResult> Upload(IFormFile file)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    if (this.Request.Form == null)
                        return BadRequest(new { message = "Invalid request." });

                    if (!this.Request.Form.ContainsKey("SaveFile"))
                        return BadRequest(new { message = "Request is missing expected save value." });

                    //(SaveTypes)int.Parse(
                    var saveFileJSON = this.Request.Form["SaveFile"];
                    var saveFile = JsonConvert.DeserializeObject<SaveFileDTO>(saveFileJSON);

                    //if (!saveFile.CompanyId.HasValue || saveFile.CompanyId.Value == 0)
                    //return BadRequest(new { message = "Request is invalid." });

                    var userId = this.User.Identity.GetUserId();
                    if (!userId.HasValue || userId.Value == 0)
                        return BadRequest(new { message = "Request is invalid." });


                    {
                        var fileName = "";
                        Dictionary<string, string> tags = null;
                        ContainerNames containerName = ContainerNames.Data;
                        switch (saveFile.SaveType)
                        {
                            case SaveTypes.CampaignFile:
                                {
                                    var campaignFileDTO = JsonConvert.DeserializeObject<CampaignFileDTO>(saveFileJSON);
                                    fileName = BlobPathHelper.GetCampaignFileBlobFilename(campaignFileDTO.TransactionId, campaignFileDTO.UID, file.FileName);

                                    tags = new Dictionary<string, string>
                                      {
                                          { "TransactionId", campaignFileDTO.TransactionId },
                                          { "Status", "temp" },
                                          { "UID", campaignFileDTO.UID },
                                          { "CreatedAt", $"{DateTimeOffset.UtcNow.ToString("yyyy-MM-dd")}" },
                                      };

                                    containerName = ContainerNames.Public;
                                    break;
                                }

                            case SaveTypes.UserProfilePicture:
                                {
                                    var user = await _appDatabaseContext
                                        .Users
                                        .AsNoTracking()
                                        .Select(i => new
                                        {
                                            i.Id,
                                            i.PictureURL
                                        })
                                        .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(10))
                                        .FirstOrDefaultAsync(i => i.Id == userId.Value);


                                    if (user != null && !string.IsNullOrWhiteSpace(user.PictureURL))
                                    {
                                        var existingBlobName = $"{BlobPathHelper.GetUserProfilePath(userId.Value)}{user.PictureURL}";
                                        await _blobManager.DeleteAsync(ContainerNames.Data, existingBlobName);
                                    }

                                    fileName = $"{BlobPathHelper.GetUserProfilePath(userId.Value)}{saveFile.Filename.ToLower()}";
                                    break;
                                }
                            default:
                                {
                                    return BadRequest(new { message = "Invalid save request." });
                                }
                        }

                        if (string.IsNullOrWhiteSpace(fileName))
                            return BadRequest(new { message = "Unable to determine file path." });

                        // Create or overwrite the blob with the contents of a local file 
                        using (var fileStream = file.OpenReadStream())
                        {
                            var provider = new FileExtensionContentTypeProvider();
                            string contentType;
                            provider.TryGetContentType(fileName, out contentType);
                            var blockBlob = await _blobManager.UploadFromStreamAsync(containerName, fileName, fileStream, contentDisposition: file.FileName, contentType, tags);

                            if (blockBlob != null)
                            {
                                //
                                //we successfully saved the file/stream so we can now do any post-save work
                                //
                                switch (saveFile.SaveType)
                                {

                                    case SaveTypes.CampaignFile:
                                        {
                                            //
                                            //placeholder -- 
                                            //

                                            //check if the file was an image, if it was then we should create thumbnails for it


                                        }
                                        break;

                                    case SaveTypes.UserProfilePicture:
                                        {
                                            try
                                            {
                                                var user = await _appDatabaseContext.Users.FirstAsync(i => i.Id == userId.Value);
                                                user.PictureURL = System.IO.Path.GetFileName(fileName).ToLower();
                                                user.UpdatedAt = DateTimeOffset.Now;
                                                await _appDatabaseContext.SaveChangesAsync();
                                            }
                                            catch (Exception ex)
                                            {
                                                wrappedLogger.LogError(ex);
                                                return BadRequest(new { message = $"Unable to save attachment ''." });
                                            }
                                            break;
                                        }


                                    default:
                                        return BadRequest(new { message = "Invalid save request." });
                                }

                                // Respond with success
                                return new JsonResult(new
                                {
                                    name = blockBlob.Name,
                                    uri = blockBlob.Uri,
                                    size = fileStream.Length
                                });
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
        */
        [HttpPost("remove")]
        public ActionResult Async_Remove(string[] fileNames)
        {
            if (fileNames != null)
            {
                foreach (var fullName in fileNames)
                {
                    /*
                    var fileName = Path.GetFileName(fullName);
                    var physicalPath = Path.Combine(_webHhostingEnvironment.WebRootPath, "Upload_Directory", fileName);

                    // TODO: Verify user permissions

                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                    */
                }
            }

            // Return an empty string to signify success
            return Content("");
        }


        [AllowAnonymous]
        [HttpGet("getprofilepicture/{userAliasId}")]
        async public Task<IActionResult> GetProfilePicture(Guid userAliasId)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    //var userId = this.User.Identity.GetUserId();
                    //if (userId.HasValue) //allow anonymous calls
                    {

                        var user = await _appDatabaseContext
                            .Users
                            .AsNoTracking()
                            .Select(i=> new
                            {
                                i.AliasId,
                                i.Id,
                                i.PictureURL
                            })
                            .Cacheable(CacheExpirationMode.Sliding, TimeSpan.FromMinutes(10))
                            .FirstOrDefaultAsync(i => i.AliasId == userAliasId);

                        if (user != null)
                        {
                            var blobName = $"{BlobPathHelper.GetUserProfilePath(user.Id)}{user.PictureURL}";

                            var blobDownloadModel = await _blobManager.ReadBlobStreamWithPropertiesAsync(ContainerNames.Data, blobName);

                            if (blobDownloadModel != null)
                            {
                                var fileStreamResult = new FileStreamResult(blobDownloadModel.Stream, blobDownloadModel.ContentType)
                                {
                                    FileDownloadName = blobDownloadModel.FileName,
                                    LastModified = blobDownloadModel.LastModified.HasValue ? blobDownloadModel.LastModified.Value : DateTimeOffset.Now
                                };

                                return fileStreamResult;
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return NotFound();
                //return BadRequest();
            }
        }



    }
}