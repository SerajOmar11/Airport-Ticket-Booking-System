using AirportTicketBookingSystem.Modules.Enums;

namespace AirportTicketBookingSystem.Modules.Filtering;

public class FlightSearchCriteria
{
    public decimal? MaxPrice { get; set; }
    public string? DepartureCountry { get; set; }
    public string? DestinationCountry { get; set; }
    public DateTime? DepartureDate { get; set; }
    public string? DepartureAirport { get; set; }
    public string? ArrivalAirport { get; set; }
    public FlightClass? Class { get; set; }
}