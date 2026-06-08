namespace EventRegistrationAPI.Models;
public class Event
{
    public string Id { get; set; } = Guid.NewGuid().ToString()[..8];
    public string Name { get; set; } = "";
    public int TotalSeats { get; set; }
    public DateTime Date { get; set; }
    public List<Registration> Registrations { get; set; } = new();
}