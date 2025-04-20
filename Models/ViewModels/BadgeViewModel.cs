namespace Eventurely.Models.ViewModels
{
    public class BadgeViewModel
    {
        public string RegistrationId { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public string EventVenue { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserInitials { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string QrCodeUrl { get; set; }
    }
}