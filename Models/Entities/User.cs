using System;
using System.Collections.Generic;

namespace Eventurely.Web.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Registration> Registrations { get; set; }
    }
}