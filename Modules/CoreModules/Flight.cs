using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Modules;

public class Flight
{
    public Guid Id { get; set; }
    
    [Required]
    public string FlightNumber { get; set; }=string.Empty;
    
    [Required]
    public string DepartureCountry { get; set; }=string.Empty;
    
    [Required]
    public string DestinationCountry { get; set; }=string.Empty;
    
    [Required]
    public string DepartureAirport { get; set; }=string.Empty;
    
    [Required]
    public string ArrivalAirport { get; set; }=string.Empty;
    
    public DateTime DepartureDate { get; set; }

    public DateTime ArrivalDate { get; set; }
    
    [Range(1.0,double.MaxValue)]
    public decimal BasePrice { get; set; }
    
    [Range(1,int.MaxValue)]
    public int TotalSeats { get; set; }
}