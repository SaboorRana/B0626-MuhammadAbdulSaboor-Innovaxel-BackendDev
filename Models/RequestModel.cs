using System.ComponentModel.DataAnnotations;

namespace EventRegistrationAPI.Models;
public class CreateEventRequest
{
    [Required(ErrorMessage = "Event name is required.")]
    public string Name { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "Total seats must be greater than 0.")]
    public int TotalSeats { get; set; }

    public DateTime Date { get; set; }
}
public class RegisterRequest
{
    [Required(ErrorMessage = "User name is required.")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Event ID is required.")]
    public string EventId { get; set; } = "";
}
public class CancelRequest
{
    [Required(ErrorMessage = "User name is required.")]
    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Event ID is required.")]
    public string EventId { get; set; } = "";
}