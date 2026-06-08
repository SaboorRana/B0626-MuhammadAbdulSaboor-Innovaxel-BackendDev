using System.Text.Json;
using EventRegistrationAPI.Models;

namespace EventRegistrationAPI.Data;
public class EventStore
{
    private readonly string _filePath;
    private readonly object _fileLock = new();
    private List<Event> _events;
    public EventStore(string filePath = "events.json")
    {
        _filePath = filePath;
        _events = new List<Event>();
        LoadData();
    }
    private void LoadData()
    {
        if (File.Exists(_filePath))
        {
            string json = File.ReadAllText(_filePath);
            _events = JsonSerializer.Deserialize<List<Event>>(json) ?? new List<Event>();
        }
    }
    public void SaveChanges()
    {
        string json = JsonSerializer.Serialize(_events, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
    public Event AddEvent(Event newEvent)
    {
        _events.Add(newEvent);
        SaveChanges();
        return newEvent;
    }
    public Event? GetEventById(string id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }
    public List<Event> GetAllEvents()
    {
        return _events;
    }
    public bool EventNameExists(string name)
    {
        return _events.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    public Registration RegisterUser(string userName, string eventId)
    {
        lock (_fileLock)
        {
            var evt = _events.FirstOrDefault(e => e.Id == eventId);
            if (evt == null)
                throw new KeyNotFoundException("Event not found.");

            int activeRegistrations = evt.Registrations.Count(r => !r.IsCancelled);
            if (activeRegistrations >= evt.TotalSeats)
                throw new InvalidOperationException("Cannot register. Event is full.");

            bool alreadyRegistered = evt.Registrations.Any(r =>
                r.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && !r.IsCancelled);

            if (alreadyRegistered)
                throw new InvalidOperationException("User is already registered for this event.");

            var registration = new Registration
            {
                UserName = userName,
                RegisteredAt = DateTime.UtcNow
            };

            evt.Registrations.Add(registration);
            SaveChanges();

            return registration;
        }
    }
    public void CancelRegistration(string userName, string eventId)
    {
        lock (_fileLock)
        {
            var evt = _events.FirstOrDefault(e => e.Id == eventId);
            if (evt == null)
                throw new KeyNotFoundException("Event not found.");

            var registration = evt.Registrations.FirstOrDefault(r =>
                r.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && !r.IsCancelled);

            if (registration == null)
                throw new InvalidOperationException("No active registration found for this user.");

            registration.IsCancelled = true;
            registration.CancelledAt = DateTime.UtcNow;
            SaveChanges();
        }
    }
}