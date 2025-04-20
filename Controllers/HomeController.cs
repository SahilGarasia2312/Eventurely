using Eventurely.Models.ViewModels;
using Eventurely.Web.Models;
using Eventurely.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Eventurely.Models;

namespace Eventurely.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventService _eventService;

        public HomeController(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            List<Event> featuredEvents = null;
            try
            {
                featuredEvents = await _eventService.GetFeaturedEventsAsync();
            }
            catch (Exception ex)
            {
                // Log error (replace with ILogger in real apps)
                Console.WriteLine($"Error fetching featured events: {ex.Message}");
            }

            if (featuredEvents == null || !featuredEvents.Any())
            {
                // Fallback mock data
                featuredEvents = new List<Event>
                {
                    new Event
                    {
                        Id = 1,
                        Title = "Tech Summit 2025",
                        Date = DateTime.Now.AddDays(10),
                        Venue = "New York Convention Center",
                        Description = "Join the future of technology with top innovators.",
                        ImagePath = "/images/codeevent.jpg"
                    },
                    new Event
                    {
                        Id = 2,
                        Title = "Summer Music Fest",
                        Date = DateTime.Now.AddDays(20),
                        Venue = "Los Angeles Park",
                        Description = "Vibe with top artists under the stars.",
                        ImagePath = "/images/musicevent.jpg"
                    },
                    new Event
                    {
                        Id = 3,
                        Title = "Business Workshop",
                        Date = DateTime.Now.AddDays(30),
                        Venue = "Chicago Business Hub",
                        Description = "Grow your network with industry leaders.",
                        ImagePath = "/images/business.jpg"
                    }
                };
            }

            var viewModel = new HomeViewModel
            {
                FeaturedEvents = featuredEvents
            };

            ViewData["Title"] = "Eventurely - Discover Events";
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
