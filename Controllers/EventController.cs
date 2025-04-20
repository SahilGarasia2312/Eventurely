using Eventurely.Web.Models;
using Eventurely.Models.ViewModels;
using Eventurely.Web.Services;
using Eventurely.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Eventurely.Web.Controllers
{
    public class EventController : Controller
    {
        private readonly EventService _eventService;
        private readonly BadgeService _badgeService;
        private readonly PdfGenerator _pdfGenerator;
        private readonly ILogger<EventController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EventController(EventService eventService, BadgeService badgeService, PdfGenerator pdfGenerator, ILogger<EventController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
            _badgeService = badgeService ?? throw new ArgumentNullException(nameof(badgeService));
            _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _eventService.GetAllEventsAsync();
                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching events for Index.");
                TempData["ErrorMessage"] = "Unable to load events. Please try again later.";
                return View(new List<Event>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid event ID {Id} provided.", id);
                    TempData["ErrorMessage"] = "Invalid event ID.";
                    return RedirectToAction(nameof(Index));
                }

                var evt = await _eventService.GetEventByIdAsync(id);
                if (evt == null)
                {
                    _logger.LogWarning("Event with ID {Id} not found.", id);
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Index));
                }

                string? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        _logger.LogWarning("Invalid NameIdentifier claim.");
                    }
                }

                var viewModel = new EventDetailViewModel
                {
                    Event = evt,
                    IsRegistered = userId != null && await _eventService.IsUserRegisteredAsync(id, userId),
                    RegistrationId = userId != null ? (await _eventService.GetRegistrationIdAsync(id, userId) ?? "0") : "0",
                    RegistrationsCount = await _eventService.GetRegistrationsCountAsync(id)
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching details for event ID {Id}.", id);
                TempData["ErrorMessage"] = "An error occurred while loading the event details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int eventId, string? notes)
        {
            try
            {
                if (eventId <= 0)
                {
                    _logger.LogWarning("Invalid event ID {EventId} provided for registration.", eventId);
                    TempData["ErrorMessage"] = "Invalid event ID.";
                    return RedirectToAction(nameof(Index));
                }

                var evt = await _eventService.GetEventByIdAsync(eventId);
                if (evt == null)
                {
                    _logger.LogWarning("Event with ID {EventId} not found for registration.", eventId);
                    TempData["ErrorMessage"] = "Event not found.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid or missing NameIdentifier claim for user.");
                    return Unauthorized();
                }

                var registration = await _eventService.RegisterUserForEventAsync(eventId, userId, notes);
                TempData["SuccessMessage"] = "Successfully registered for the event!";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user for event ID {EventId}.", eventId);
                TempData["ErrorMessage"] = "Failed to register for the event. Please try again.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(int registrationId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid or missing NameIdentifier claim for user.");
                    return Unauthorized();
                }

                var eventId = await _eventService.GetEventIdByRegistrationAsync(registrationId);
                if (eventId == 0)
                {
                    _logger.LogWarning("Registration ID {RegistrationId} not found.", registrationId);
                    TempData["ErrorMessage"] = "Registration not found.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _eventService.CancelRegistrationAsync(registrationId, userId);
                if (!success)
                {
                    _logger.LogWarning("Failed to cancel registration ID {RegistrationId} for user ID {UserId}.", registrationId, userId);
                    TempData["ErrorMessage"] = "Failed to cancel registration.";
                    return RedirectToAction(nameof(Details), new { id = eventId });
                }

                TempData["SuccessMessage"] = "Registration canceled successfully!";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling registration ID {RegistrationId}.", registrationId);
                TempData["ErrorMessage"] = "Failed to cancel registration.";
                var eventId = await _eventService.GetEventIdByRegistrationAsync(registrationId);
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var evt = await _eventService.GetEventByIdAsync(id);
                if (evt == null)
                {
                    _logger.LogWarning("Event with ID {Id} not found for editing.", id);
                    return NotFound();
                }
                return View(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching event ID {Id} for editing.", id);
                TempData["ErrorMessage"] = "An error occurred while loading the event.";
                return StatusCode(500, "An error occurred while loading the event.");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Event updatedEvent, IFormFile? imageFile)
        {
            if (updatedEvent == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedEvent);
            }

            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Define the path to save the image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/events");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(updatedEvent.ImagePath))
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images/events", updatedEvent.ImagePath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Update the ImagePath with the new file name
                    updatedEvent.ImagePath = uniqueFileName;
                }

                await _eventService.UpdateEventAsync(updatedEvent);
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event ID {EventId}.", updatedEvent.Id);
                ModelState.AddModelError("", "Failed to update event. Please try again.");
                return View(updatedEvent);
            }
        }

        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var evt = await _eventService.GetEventByIdAsync(id);
                if (evt == null)
                {
                    _logger.LogWarning("Event with ID {Id} not found for deletion.", id);
                    return NotFound();
                }
                return View(evt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching event ID {Id} for deletion.", id);
                TempData["ErrorMessage"] = "An error occurred while loading the event.";
                return StatusCode(500, "An error occurred while loading the event.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var evt = await _eventService.GetEventByIdAsync(id);
                if (evt != null && !string.IsNullOrEmpty(evt.ImagePath))
                {
                    // Delete the image file if it exists
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images/events", evt.ImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                await _eventService.DeleteEventAsync(id);
                TempData["SuccessMessage"] = "Event deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event ID {Id}.", id);
                TempData["ErrorMessage"] = "Failed to delete event. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize]
        public IActionResult Create()
        {
            return View(new Event());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event newEvent, IFormFile? imageFile)
        {
            if (newEvent == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(newEvent);
            }

            try
            {
                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Define the path to save the image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/events");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // Set the ImagePath to the file name
                    newEvent.ImagePath = uniqueFileName;
                }

                await _eventService.AddEventAsync(newEvent);
                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new event.");
                ModelState.AddModelError("", "Failed to create event. Please try again.");
                return View(newEvent);
            }
        }

        [Authorize]
        public async Task<IActionResult> ViewBadge(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid or missing NameIdentifier claim for user.");
                    return Unauthorized();
                }

                var badge = _badgeService.GenerateBadge(id, userId);
                if (badge == null)
                {
                    TempData["ErrorMessage"] = "Badge not found or invalid registration.";
                    return RedirectToAction(nameof(Index));
                }

                return View(badge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating badge for registration ID {Id}.", id);
                TempData["ErrorMessage"] = "An error occurred while generating the badge.";
                return StatusCode(500, "An error occurred while generating the badge.");
            }
        }
    }
}