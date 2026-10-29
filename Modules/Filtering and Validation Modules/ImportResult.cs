namespace AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;

public class ImportResult
{
    public List<Flight> Flights { get; set; } = new();
    public List<ValidationError> Errors { get; set; } = new();
    public bool IsSuccessful => Errors.Count == 0;
}