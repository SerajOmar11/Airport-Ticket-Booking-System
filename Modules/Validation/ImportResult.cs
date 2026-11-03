using AirportTicketBookingSystem.Modules.Models;

namespace AirportTicketBookingSystem.Modules.Validation;

public class ImportResult
{
    public List<Flight> Flights { get; set; } = new();
    public List<ValidationError> Errors { get; set; } = new();
    public bool IsSuccessful => Errors.Count == 0;
}