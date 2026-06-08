namespace EventRegistrationAPI.Models;
public class Registration
{
    public string UserName { get; set; } = "";
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; } = false;
    public DateTime? CancelledAt { get; set; } 
}