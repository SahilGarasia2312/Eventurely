using Eventurely.Web.Data;
using Eventurely.Models;
using Eventurely.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventurely.Web.Services
{
    public class EventService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventService> _logger;

        public EventService(ApplicationDbContext context, ILogger<EventService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Event>> GetFeaturedEventsAsync()
        {
            try
            {
                return await _context.Events
                    .Where(e => e.Date >= DateTime.Now)
                    .OrderBy(e => e.Date)
                    .Take(3)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching featured events.");
                throw;
            }
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            try
            {
                return await _context.Events.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all events.");
                throw;
            }
        }

        public async Task<List<Event>> GetActiveEventsAsync()
        {
            try
            {
                return await _context.Events
                    .Where(e => e.Date >= DateTime.Now)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching active events.");
                throw;
            }
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            try
            {
                return await _context.Events.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching event with ID {id}.");
                throw;
            }
        }

        public async Task AddEventAsync(Event newEvent)
        {
            if (newEvent == null) throw new ArgumentNullException(nameof(newEvent));

            try
            {
                await _context.Events.AddAsync(newEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new event.");
                throw;
            }
        }

        public async Task UpdateEventAsync(Event updatedEvent)
        {
            if (updatedEvent == null) throw new ArgumentNullException(nameof(updatedEvent));

            try
            {
                var existingEvent = await _context.Events.FindAsync(updatedEvent.Id);
                if (existingEvent == null)
                {
                    throw new InvalidOperationException("Event not found.");
                }

                existingEvent.Title = updatedEvent.Title;
                existingEvent.Date = updatedEvent.Date;
                existingEvent.Venue = updatedEvent.Venue;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.ImagePath = updatedEvent.ImagePath;

                _context.Events.Update(existingEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating event with ID {updatedEvent.Id}.");
                throw;
            }
        }

        public async Task DeleteEventAsync(int id)
        {
            try
            {
                var eventToDelete = await _context.Events.FindAsync(id);
                if (eventToDelete == null)
                {
                    throw new InvalidOperationException("Event not found.");
                }

                _context.Events.Remove(eventToDelete);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting event with ID {id}.");
                throw;
            }
        }

        public async Task<bool> IsUserRegisteredAsync(int eventId, string userId)
        {
            try
            {
                return await _context.Registrations
                    .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking registration for event ID {eventId} and user ID {userId}.");
                throw;
            }
        }

        public async Task<string?> GetRegistrationIdAsync(int eventId, string userId)
        {
            try
            {
                var registration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
                return registration?.Id.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching registration ID for event ID {eventId} and user ID {userId}.");
                throw;
            }
        }

        public async Task<int> GetRegistrationsCountAsync(int eventId)
        {
            try
            {
                return await _context.Registrations
                    .CountAsync(r => r.EventId == eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting registrations for event ID {eventId}.");
                throw;
            }
        }

        public async Task<Registration?> RegisterUserForEventAsync(int eventId, string userId, string? notes)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found.");
                    return null;
                }

                var registration = new Registration
                {
                    EventId = eventId,
                    UserId = userId,
                    Notes = notes,
                    RegistrationTime = DateTime.Now,
                    AttendanceStatus = "Registered"
                };

                await _context.Registrations.AddAsync(registration);
                await _context.SaveChangesAsync();
                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user ID {userId} for event ID {eventId}.");
                throw;
            }
        }

        public async Task<bool> CancelRegistrationAsync(int registrationId, string userId)
        {
            try
            {
                var registration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.Id == registrationId && r.UserId == userId);
                if (registration == null) return false;

                _context.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error canceling registration ID {registrationId} for user ID {userId}.");
                throw;
            }
        }

        public async Task<int> GetEventIdByRegistrationAsync(int registrationId)
        {
            try
            {
                var registration = await _context.Registrations.FindAsync(registrationId);
                return registration?.EventId ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching event ID for registration ID {registrationId}.");
                throw;
            }
        }

        public async Task<List<Registration>> GetUserRegistrationsAsync(string userId)
        {
            try
            {
                return await _context.Registrations
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Event)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching registrations for user ID {userId}.");
                throw;
            }
        }

        public async Task<List<Registration>> GetAllRegistrationsAsync()
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Event)
                    .Include(r => r.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all registrations.");
                throw;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users.");
                throw;
            }
        }

        public async Task<Registration?> GetRegistrationByIdAsync(int id)
        {
            try
            {
                return await _context.Registrations
                    .Include(r => r.Event)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching registration with ID {id}.");
                throw;
            }
        }

        public async Task<bool> MarkAttendanceAsync(int id, string status)
        {
            try
            {
                var registration = await _context.Registrations.FindAsync(id);
                if (registration == null) return false;

                registration.AttendanceStatus = status;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking attendance for registration ID {id}.");
                throw;
            }
        }
    }
}