using System;
using UpRaise.Models.Enums;

namespace UpRaise.DTOs
{
    public class UserPersonalInformationDTO
    {
        public string FirstName { get; set; }
        public string LastName{ get; set; }
        public string Country { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string ZipPostal { get; set; }

        public byte DefaultCurrencyId { get; set; }
    }

}

