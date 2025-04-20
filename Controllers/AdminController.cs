using Eventurely.Web.Models;
using Eventurely.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Eventurely.Models; // You should keep this for accessing the base models
using Eventurely.Models.ViewModels; // Import the ViewModels for RecentRegistration, RegistrationTrend, and PopularEvent

namespace Eventurely.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly EventService _eventService;
        private readonly AnalyticsService _analyticsService;

        public AdminController(EventService eventService, AnalyticsService analyticsService)
        {
            _eventService = eventService;
            _analyticsService = analyticsService;
        }

        public IActionResult Dashboard()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalEvents = _analyticsService.GetTotalEvents(),
                ActiveEvents = _analyticsService.GetActiveEvents(),
                TotalRegistrations = _analyticsService.GetTotalRegistrations(),
                TotalUsers = _analyticsService.GetTotalUsers(),
                RecentRegistrations = _analyticsService.GetRecentRegistrations(),
                RegistrationTrends = _analyticsService.GetRegistrationTrends(),
                PopularEvents = _analyticsService.GetPopularEvents()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> ManageEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> ActiveEvents()
        {
            var events = await _eventService.GetActiveEventsAsync();
            return View(events);
        }

        public async Task<IActionResult> Registrations()
        {
            var registrations = await _eventService.GetAllRegistrationsAsync();
            return View(registrations);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _eventService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> ViewRegistration(int id)
        {
            var registration = await _eventService.GetRegistrationByIdAsync(id);
            if (registration == null)
            {
                return NotFound();
            }
            return View(registration);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAttendance(int id, string status)
        {
            var success = await _eventService.MarkAttendanceAsync(id, status);
            return Json(new { success });
        }
    }
}
