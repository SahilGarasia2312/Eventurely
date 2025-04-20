using Eventurely.Models.ViewModels;

namespace Eventurely.Web.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int TotalRegistrations { get; set; }
        public int TotalUsers { get; set; }
        public List<RecentRegistration> RecentRegistrations { get; set; } // Change to ViewModels
        public List<RegistrationTrend> RegistrationTrends { get; set; } // Change to ViewModels
        public List<PopularEvent> PopularEvents { get; set; } // Change to ViewModels
    }
}
