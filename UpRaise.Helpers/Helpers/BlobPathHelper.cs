using System;
using UpRaise.Models.Enums;

namespace UpRaise.Helpers
{

    public static class BlobPathHelper
    {
        public static readonly string DomainPrefix = "https://data.upraise.fund";
        //public static readonly string DomainPrefix = "https://upraise.blob.core.windows.net";
        //private static readonly string userFolder = "user";
        public static string GetCampaignFilePath(int campaignId)
        {
            var path = $"campaign/{campaignId}/";
            return path;
        }

        public static string GetCampaignFileBlobFilename(string transactionId, string uid, string filename)
        {
            var path = $"campaign_files/{transactionId}/{uid}".ToLower() + $"_{filename}";
            return path;
        }

        public static string GetCampaignDescriptionFilename(CampaignTypes campaignType, int campaignId, int? updateId)
        {
            var filename = $"desc{(updateId.HasValue?"_" + updateId.Value:"")}.quill";
            var path = $"description/{(int)campaignType}/{campaignId}/{filename}";
            return path;
        }


        public static string GetUserProfilePath(int userId)
        {
            var path = $"user/{userId}/";
            return path;
        }

    }
}
