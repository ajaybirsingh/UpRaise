using System;

namespace UpRaise.DTOs
{
    public enum SaveTypes
    {
        CampaignFile = 1,
        UserProfilePicture = 2,
        CompanyLogoPicture = 3,
        CampaignHeaderPicture = 4,
    }

    public class SaveFileDTO
    {
        public SaveTypes SaveType { get; set; }
        //public int? CompanyId { get; set; }
        public string Filename { get; set; }
    }



    public class CampaignFileDTO
    {
        public SaveTypes SaveType { get; set; }

        public string TransactionId { get; set; }

        public byte TypeId { get; set; }

        public string UID { get; set; }

        public string Filename { get; set; }
    }
}