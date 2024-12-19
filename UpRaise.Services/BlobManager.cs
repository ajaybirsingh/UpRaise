using UpRaise.Helpers;
using UpRaise.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.Text;

namespace UpRaise.Services
{
    public class BlobManager : IBlobManager
    {
        private readonly ILogger<BlobManager> _logger = null;
        private readonly IOptions<StorageAccountSettings> _storageAccountSettings = null;

        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobDataContainerClient;
        private readonly BlobContainerClient _blobTempContainerClient;
        private readonly BlobContainerClient _blobPublicContainerClient;

        private readonly IConfiguration _configuration;

        public BlobManager(ILogger<BlobManager> logger, IOptions<StorageAccountSettings> storageAccountSettings, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _storageAccountSettings = storageAccountSettings;

            _blobServiceClient = new BlobServiceClient(_configuration["ConnectionString:S3"]);
            _blobDataContainerClient = _blobServiceClient.GetBlobContainerClient(_storageAccountSettings.Value.data_container_name);
            _blobTempContainerClient = _blobServiceClient.GetBlobContainerClient(_storageAccountSettings.Value.temp_container_name);
            _blobPublicContainerClient = _blobServiceClient.GetBlobContainerClient(_storageAccountSettings.Value.public_container_name);
        }


        public async Task<BlobClient> UploadFromStringAsync(ContainerNames containerName, string fileName, string data, bool overwrite)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var blobContainerClient = GetBlobContainerClient(containerName);

                    var blobClient = blobContainerClient.GetBlobClient(fileName);

                    var content = Encoding.UTF8.GetBytes(data);
                    using (var ms = new MemoryStream(content))
                        await blobClient.UploadAsync(ms, overwrite);

                    return blobClient;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return null;
            }
        }


        public async Task<BlobClient> UploadFromStreamAsync(ContainerNames containerName, string fileName, Stream fileStream, string contentDisposition = "", string contentType = "", Dictionary<string, string> tags = null)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var blobContainerClient = GetBlobContainerClient(containerName);

                    var blobClient = blobContainerClient.GetBlobClient(fileName);

                    var blobHttpHeaders = new BlobHttpHeaders();

                    if (!string.IsNullOrWhiteSpace(contentType))
                        blobHttpHeaders.ContentType = GetContentType(contentType);

                    if (!string.IsNullOrWhiteSpace(contentDisposition))
                        blobHttpHeaders.ContentDisposition = $"attachment; filename={contentDisposition}";


                    // Create or overwrite the blob with the contents of a local file 
                    await blobClient.UploadAsync(fileStream, blobHttpHeaders);

                    if (tags != null)
                        await blobClient.SetTagsAsync(tags);

                    return blobClient;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return null;
            }
        }

        public async Task<bool> DeleteAsync(ContainerNames containerName, string blobName)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var blobContainerClient = GetBlobContainerClient(containerName);

                    var blobClient = blobContainerClient.GetBlobClient(blobName);

                    var result = await blobClient.DeleteIfExistsAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }
                return false;
            }
        }

        public async Task<bool> ExistsAsync(ContainerNames containerName, string blobName)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var blobContainerClient = GetBlobContainerClient(containerName);
                    var blobClient = blobContainerClient.GetBlobClient(blobName);
                    var exists = await blobClient.ExistsAsync();

                    return exists;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }
                return false;
            }
        }

        public async Task<bool> SetTagAsync(ContainerNames containerName, string blobName, string key, string value)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    var blobContainerClient = GetBlobContainerClient(containerName);
                    var blobClient = blobContainerClient.GetBlobClient(blobName);

                    var response = await blobClient.GetTagsAsync();

                    response.Value.Tags[key] = value;

                    await blobClient.SetTagsAsync(response.Value.Tags);

                    return true;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }
                return false;
            }
        }

        public async Task<BlobDownloadModel> ReadBlobStreamWithPropertiesAsync(ContainerNames containerName, string blobName)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    wrappedLogger.LogTrace($"{blobName}");

                    var blobContainerClient = GetBlobContainerClient(containerName);

                    var blobClient = blobContainerClient.GetBlobClient(blobName);

                    var blobProperties = await blobClient.GetPropertiesAsync();

                    var blobDownloadModel = new BlobDownloadModel();
                    blobDownloadModel.FileName = System.IO.Path.GetFileName(blobName);
                    blobDownloadModel.Stream = await blobClient.OpenReadAsync();
                    blobDownloadModel.Length = blobDownloadModel.Stream.Length;
                    blobDownloadModel.ContentType = GetContentType(blobDownloadModel.FileName);
                    blobDownloadModel.LastModified = blobProperties.Value.LastModified;

                    return blobDownloadModel;
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return null;
            }

        }

        public async Task<string> ReadBlobStringAsync(ContainerNames containerName, string blobName)
        {
            using (var wrappedLogger = new WrappedLogger(_logger))
            {
                try
                {
                    wrappedLogger.LogTrace($"{blobName}");

                    var blobContainerClient = GetBlobContainerClient(containerName);

                    var blobClient = blobContainerClient.GetBlobClient(blobName);

                    if (await blobClient.ExistsAsync())
                    {

                        var response = await blobClient.OpenReadAsync();

                        var sb = new StringBuilder();
                        using (var streamReader = new StreamReader(response))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                var line = await streamReader.ReadLineAsync();
                                sb.Append(line);
                            }
                        }

                        return sb.ToString();
                    }
                }
                catch (Exception ex)
                {
                    wrappedLogger.LogError(ex);
                }

                return null;
            }

        }

        private string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(fileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private BlobContainerClient GetBlobContainerClient(ContainerNames containerName)
        {
            switch (containerName)
            {
                case ContainerNames.Data:
                    return _blobDataContainerClient;

                case ContainerNames.Temp:
                    return _blobTempContainerClient;

                case ContainerNames.Public:
                    return _blobPublicContainerClient;
            }

            return null;
        }


    }

}
