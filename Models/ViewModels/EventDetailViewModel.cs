using Eventurely.Web.Models;

namespace Eventurely.Models.ViewModels
{
    public class EventDetailViewModel
    {
        public Event Event { get; set; }
        public bool IsRegistered { get; set; } // Changed to nullable bool
        public string RegistrationId { get; set; } // Changed to nullable int
        public int RegistrationsCount { get; set; } // Changed to nullable int
        public string Notes { get; set; }
    }
}