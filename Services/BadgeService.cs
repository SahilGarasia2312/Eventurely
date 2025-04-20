using Eventurely.Web.Data;
using Eventurely.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Eventurely.Web.Services
{
    public class BadgeService
    {
        private readonly ApplicationDbContext _context;

        public BadgeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public BadgeViewModel GenerateBadge(int registrationId, string userId)
        {
            var registration = _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefault(r => r.Id == registrationId && r.UserId == userId);

            if (registration == null) return null;

            var user = registration.User;
            var evt = registration.Event;

            return new BadgeViewModel
            {
                UserName = user.Name,
                UserEmail = user.Email,
                UserInitials = user.Name.Length > 0 ? user.Name.Substring(0, 1).ToUpper() : "",
                EventTitle = evt.Title,
                EventDate = evt.Date,
                EventVenue = evt.Venue,
                RegistrationId = registration.Id.ToString(),
                RegistrationDate = registration.RegistrationTime,
                QrCodeUrl = GenerateQrCodeUrl(registration.Id),
                EventId = evt.Id
            };
        }

        private string GenerateQrCodeUrl(int registrationId)
        {
            return $"/api/qrcode/{registrationId}";
        }
    }
}