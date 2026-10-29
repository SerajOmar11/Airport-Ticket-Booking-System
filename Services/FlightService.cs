using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;

namespace AirportTicketBookingSystem.Services;

public class FlightService
{
    private readonly StorageService<Flight>  _flightStorage;
    private readonly StorageService<Booking>  _bookingStorage;

    private readonly string _flightPath;
    private readonly string _bookingPath;

    public FlightService(StorageService<Flight> flightStorage, StorageService<Booking> bookingStorage, string flightPath, string bookingPath)
    {
        _flightStorage = flightStorage??throw new ArgumentNullException(nameof(flightStorage));
        _bookingStorage = bookingStorage??throw new ArgumentNullException(nameof(bookingStorage));
        _flightPath = flightPath??throw new ArgumentNullException(nameof(flightPath));
        _bookingPath = bookingPath??throw new ArgumentNullException(nameof(bookingPath));
    }
    
    public List<Flight> GetAllFlights()
    {
        return _flightStorage.Load(_flightPath);
    }

    public Flight? GetFlightById(Guid id)
    {
        return GetAllFlights().FirstOrDefault(f=>f.Id == id);
    }

    public decimal CalculateTotalPrice(Flight flight,FlightClass flightClass)
    {
        ArgumentNullException.ThrowIfNull(flight);
        
        return flightClass switch
        {
            FlightClass.Economy=>flight.BasePrice,
            FlightClass.Business=>flight.BasePrice*2m,
            FlightClass.FirstClass=>flight.BasePrice*3m,
            _ => throw new ArgumentOutOfRangeException(nameof(flightClass), "Invalid flight class"),
        };
    }

    public List<Flight> SearchFlights(FlightSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if(criteria.MaxPrice<0)
            throw new ArgumentOutOfRangeException(nameof(criteria.MaxPrice), "MaxPrice cannot be negative");
        
        if(criteria.Class.HasValue&&!Enum.IsDefined(criteria.Class.Value))
            throw new ArgumentOutOfRangeException(nameof(criteria.Class), "Invalid flight class");
        
        IEnumerable<Flight> query = GetAllFlights();
        query = query.Where(f => f.DepartureDate > DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(criteria.DepartureCountry)) {
            query=query.Where(f=>f.DepartureCountry.Equals(criteria.DepartureCountry,StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(criteria.DestinationCountry)) {
            query=query.Where(f=>f.DestinationCountry.Equals(criteria.DestinationCountry,StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(criteria.DepartureAirport)) {
            query=query.Where(f=>f.DepartureAirport.Equals(criteria.DepartureAirport,StringComparison.OrdinalIgnoreCase));
        }
        
        if (!string.IsNullOrWhiteSpace(criteria.ArrivalAirport)) {
            query=query.Where(f=>f.ArrivalAirport.Equals(criteria.ArrivalAirport,StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.DepartureDate.HasValue) {
            query=query.Where(f => f.DepartureDate.Date == criteria.DepartureDate.Value.Date);
        }

        if (criteria.MaxPrice.HasValue) {
            query=query.Where(f=>CalculateTotalPrice(f,criteria.Class??FlightClass.Economy)<=criteria.MaxPrice.Value);
        }
        
        var bookedSeats = _bookingStorage.Load(_bookingPath)
            .Where(b => b.Status == BookingStatus.Confirmed)
            .GroupBy(b => b.FlightId)
            .ToDictionary(g => g.Key, g => g.Count());

        query=query.Where(f => f.TotalSeats > bookedSeats.GetValueOrDefault(f.Id));

        return query.OrderBy(f => f.DepartureDate).ToList();
    }

    public int GetAvailableSeats(Guid flightId)
    {
        Flight flight = GetFlightById(flightId)??throw new KeyNotFoundException("Flight not found");
        
        int bookedSeats = _bookingStorage
                        .Load(_bookingPath)
                        .Count(b=>b.FlightId==flightId&&b.Status==BookingStatus.Confirmed);
        
        return Math.Max(flight.TotalSeats-bookedSeats,0);
    }
}