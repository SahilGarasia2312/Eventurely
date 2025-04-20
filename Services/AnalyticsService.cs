using Eventurely.Web.Data;
using Eventurely.Web.Models; // Add this line for BadgeViewModel
using Eventurely.Models.ViewModels; // This line is already present
using Eventurely.Models.ViewModels; // Ensure this line is present for RegistrationTrend and PopularEvent
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Eventurely.Web.Services
{
    public class AnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public int GetTotalEvents()
        {
            return _context.Events.Count();
        }

        public int GetActiveEvents()
        {
            return _context.Events.Count(e => e.Date >= DateTime.Now);
        }

        public int GetTotalRegistrations()
        {
            return _context.Registrations.Count();
        }

        public int GetTotalUsers()
        {
            return _context.Users.Count();
        }

        public List<RecentRegistration> GetRecentRegistrations()
        {
            return _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .OrderByDescending(r => r.RegistrationTime)
                .Take(5)
                .Select(r => new RecentRegistration
                {
                    Id = r.Id,
                    UserName = r.User.Name,
                    EventTitle = r.Event.Title,
                    RegistrationTime = r.RegistrationTime,
                    AttendanceStatus = r.AttendanceStatus
                })
                .ToList();
        }

        public List<RegistrationTrend> GetRegistrationTrends()
        {
            var startDate = DateTime.Now.AddDays(-30);
            return _context.Registrations
                .Where(r => r.RegistrationTime >= startDate)
                .GroupBy(r => r.RegistrationTime.Date)
                .Select(g => new RegistrationTrend
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Date)
                .ToList();
        }

        public List<PopularEvent> GetPopularEvents()
        {
            return _context.Registrations
                .GroupBy(r => r.Event)
                .Select(g => new PopularEvent
                {
                    Title = g.Key.Title,
                    RegistrationsCount = g.Count()
                })
                .OrderByDescending(e => e.RegistrationsCount)
                .Take(5)
                .ToList();
        }
    }
}