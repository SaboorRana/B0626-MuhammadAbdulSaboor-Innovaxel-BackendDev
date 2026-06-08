using Microsoft.AspNetCore.Mvc;
using EventRegistrationAPI.Data;
using EventRegistrationAPI.Models;

namespace EventRegistrationAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly EventStore _eventStore;

    public EventsController(EventStore eventStore)
    {
        _eventStore = eventStore;
    }
    [HttpPost]
    public IActionResult CreateEvent([FromBody] CreateEventRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Event name cannot be empty." });

        if (request.TotalSeats <= 0)
            return BadRequest(new { error = "Total seats must be greater than 0." });

        if (request.Date <= DateTime.Now)
            return BadRequest(new { error = "Event date must be in the future." });

        // Check unique name
        if (_eventStore.EventNameExists(request.Name))
            return Conflict(new { error = $"Event with name '{request.Name}' already exists." });

        var newEvent = new Event
        {
            Name = request.Name,
            TotalSeats = request.TotalSeats,
            Date = request.Date
        };

        _eventStore.AddEvent(newEvent);

        return CreatedAtAction(nameof(GetEvent), new { id = newEvent.Id }, new
        {
            message = "Event created successfully.",
            eventData = newEvent
        });
    }

    [HttpGet]
    public IActionResult GetAllEvents([FromQuery] bool upcomingOnly = false)
    {
        var result = _eventStore.GetAllEvents().AsEnumerable();

        if (upcomingOnly)
            result = result.Where(e => e.Date > DateTime.Now);

        result = result.OrderBy(e => e.Date);

        var response = result.Select(e => new
        {
            e.Id,
            e.Name,
            e.TotalSeats,
            AvailableSeats = e.TotalSeats - e.Registrations.Count(r => !r.IsCancelled),
            TotalRegistrations = e.Registrations.Count,
            ActiveRegistrations = e.Registrations.Count(r => !r.IsCancelled),
            e.Date
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetEvent(string id)
    {
        var evt = _eventStore.GetEventById(id);

        if (evt == null)
            return NotFound(new { error = "Event not found." });

        return Ok(new
        {
            evt.Id,
            evt.Name,
            evt.TotalSeats,
            AvailableSeats = evt.TotalSeats - evt.Registrations.Count(r => !r.IsCancelled),
            TotalRegistrations = evt.Registrations.Count,
            ActiveRegistrations = evt.Registrations.Count(r => !r.IsCancelled),
            evt.Date
        });
    }

    [HttpPost("register")]
    public IActionResult RegisterUser([FromBody] RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest(new { error = "User name cannot be empty." });

        if (string.IsNullOrWhiteSpace(request.EventId))
            return BadRequest(new { error = "Event ID cannot be empty." });

        try
        {
            var registration = _eventStore.RegisterUser(request.UserName, request.EventId);
            var evt = _eventStore.GetEventById(request.EventId);

            return Ok(new
            {
                message = "Registration successful.",
                registration = new
                {
                    userName = registration.UserName,
                    eventId = evt!.Id,
                    eventName = evt.Name,
                    registeredAt = registration.RegisteredAt
                }
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("cancel")]
    public IActionResult CancelRegistration([FromBody] CancelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest(new { error = "User name cannot be empty." });

        if (string.IsNullOrWhiteSpace(request.EventId))
            return BadRequest(new { error = "Event ID cannot be empty." });

        try
        {
            _eventStore.CancelRegistration(request.UserName, request.EventId);

            var evt = _eventStore.GetEventById(request.EventId);
            int availableNow = evt!.TotalSeats - evt.Registrations.Count(r => !r.IsCancelled);

            return Ok(new
            {
                message = "Registration cancelled successfully.",
                availableSeats = availableNow
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}