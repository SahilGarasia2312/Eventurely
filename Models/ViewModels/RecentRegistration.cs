// File: Models/ViewModels/RecentRegistration.cs
namespace Eventurely.Models.ViewModels
{
    public class RecentRegistration
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string EventTitle { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string AttendanceStatus { get; set; }
    }
}
