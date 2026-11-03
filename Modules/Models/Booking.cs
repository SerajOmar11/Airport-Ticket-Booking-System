namespace AirportTicketBookingSystem.Modules;

public class Booking
{
    public Guid Id { get; set; }= Guid.NewGuid();
    
    public Guid PassengerId { get; set; }
    
    public Guid FlightId { get; set; }
    
    public FlightClass Class { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public DateTime BookingDate { get; set; }=DateTime.UtcNow;
    
    public BookingStatus Status { get; set; }= BookingStatus.Confirmed;
}