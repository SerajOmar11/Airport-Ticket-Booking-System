using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Modules.Models;

public class Flight
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(20,MinimumLength = 1)]
    public string FlightNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100,MinimumLength = 1)]
    public string DepartureCountry { get; set; } = string.Empty;

    [Required]
    [StringLength(100,MinimumLength = 1)]
    public string DestinationCountry { get; set; } = string.Empty;

    [Required]
    [StringLength(100,MinimumLength = 1)]
    public string DepartureAirport { get; set; } = string.Empty;

    [Required]
    [StringLength(100,MinimumLength = 1)]
    public string ArrivalAirport { get; set; } = string.Empty;

    public DateTime DepartureDate { get; set; }

    public DateTime ArrivalDate { get; set; }

    [Range(typeof(decimal), "1", "1000000")]
    public decimal BasePrice { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalSeats { get; set; }
}