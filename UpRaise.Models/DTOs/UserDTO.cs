using System;

namespace UpRaise.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public Guid AliasId { get; set; }
        public int CompanyId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PictureURL { get; set; }
        //public string CompanyName { get; set; }
        //public string CompanyPictureURL { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}