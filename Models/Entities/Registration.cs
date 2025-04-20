using System;

namespace Eventurely.Web.Models
{
    public class Registration
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public string UserId { get; set; } // Changed to string to match User.Id and ClaimTypes.NameIdentifier

        public string Notes { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string AttendanceStatus { get; set; }

        public Event Event { get; set; }
        public User User { get; set; }
    }
}