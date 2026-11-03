using System.ComponentModel.DataAnnotations;

namespace AirportTicketBookingSystem.Modules.Models;

public class Passenger
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string PassportNumber { get; set; } = string.Empty;
}