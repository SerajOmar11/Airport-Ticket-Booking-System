using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Modules;

public class Passenger
{
    public Guid Id { get; set; }
    
    [Required]
    public string FullName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string PassportNumber { get; set; }
}