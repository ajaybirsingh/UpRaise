using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace UpRaise.Services
{
    public interface IBlobManager
    {
        Task<BlobClient> UploadFromStreamAsync(ContainerNames containerName, string fileName, Stream fileStream, string contentDisposition = "", string contentType = "", Dictionary<string, string> tags = null);
        Task<BlobClient> UploadFromStringAsync(ContainerNames containerName, string fileName, string data, bool overwrite);

        Task<BlobDownloadModel> ReadBlobStreamWithPropertiesAsync(ContainerNames containerName, string blobName);
        Task<string> ReadBlobStringAsync(ContainerNames containerName, string blobName);

        Task<bool> DeleteAsync(ContainerNames containerName, string blobName);

        Task<bool> ExistsAsync(ContainerNames containerName, string blobName);

        Task<bool> SetTagAsync(ContainerNames containerName, string blobName,string key, string value);
        
    }

    public class BlobDownloadModel
    {
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Length { get; set; }
        public DateTimeOffset? LastModified { get; set; }
    }
}

public enum ContainerNames 
{ 
    [Description("data")]
    Data,

    [Description("public")]
    Public,

    [Description("temp")]
    Temp

}
