using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;

namespace AirportTicketBookingSystem.Services;

public class BookingService
{
    private readonly StorageService<Booking> _bookingStorage;
    private readonly StorageService<Passenger> _passengerStorage;
    private readonly FlightService _flightService;

    private readonly string _bookingPath;
    private readonly string _passengerPath;

    public BookingService(
        StorageService<Booking> bookingStorage, StorageService<Passenger> passengerStorage,
        FlightService flightService,
        string bookingPath, string passengerPath
        )
    {
        _bookingStorage = bookingStorage ?? throw new ArgumentNullException(nameof(bookingStorage));
        _passengerStorage = passengerStorage ?? throw new ArgumentNullException(nameof(passengerStorage));
        _flightService = flightService ?? throw new ArgumentNullException(nameof(flightService));
        
        _bookingPath = !string.IsNullOrWhiteSpace(bookingPath) ? bookingPath
            : throw new ArgumentException("Booking path cannot be empty.", nameof(bookingPath));

        _passengerPath = !string.IsNullOrWhiteSpace(passengerPath) ? passengerPath
            : throw new ArgumentException("Passenger path cannot be empty.", nameof(passengerPath));
    }

    private bool PassengerExists(Guid passengerId)
    {
        return _passengerStorage.Load(_passengerPath).Any(p => p.Id == passengerId);
    }

    public Booking BookFlight(Guid flightId, Guid passengerId, FlightClass flightClass)
    {
        if (passengerId == Guid.Empty)
            throw new ArgumentException("Passenger ID cannot be empty.", nameof(passengerId));

        if (!PassengerExists(passengerId))
            throw new KeyNotFoundException("Passenger with this ID doesn't exist.");

        Flight flight = _flightService.GetFlightById(flightId) ?? 
                        throw new KeyNotFoundException("Flight with this ID doesn't exist.");

        if (flight.DepartureDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot book a flight that has already departed.");

        if (_flightService.GetAvailableSeats(flightId) <= 0)
            throw new InvalidOperationException("No available seats on this flight.");

        decimal totalPrice = _flightService.CalculateTotalPrice(flight, flightClass);

        Booking newBooking = new()
        {
            PassengerId = passengerId,
            FlightId = flightId,
            Class = flightClass,
            TotalPrice = totalPrice,
            BookingDate = DateTime.UtcNow,
            Status = BookingStatus.Confirmed
        };

        List<Booking> bookings = _bookingStorage.Load(_bookingPath);
        bookings.Add(newBooking);
        _bookingStorage.Save(_bookingPath, bookings);

        return newBooking;
    }

    public bool CancelBooking(Guid bookingId, Guid passengerId)
    {
        if (bookingId == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty.", nameof(bookingId));

        if (passengerId == Guid.Empty)
            throw new ArgumentException("Passenger ID cannot be empty.", nameof(passengerId));

        List<Booking> bookings = _bookingStorage.Load(_bookingPath);

        Booking booking = bookings.FirstOrDefault(b => b.Id == bookingId) 
                          ?? throw new KeyNotFoundException("Booking with this ID doesn't exist.");

        if (booking.PassengerId != passengerId)
            throw new UnauthorizedAccessException("You cannot cancel another passenger's booking.");

        if (booking.Status == BookingStatus.Cancelled)
            return false;
    
        Flight flight = _flightService.GetFlightById(booking.FlightId) ?? throw new KeyNotFoundException("Flight not found.");

        if (flight.DepartureDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot cancel a booking after the flight has departed.");
        
        booking.Status = BookingStatus.Cancelled;
        _bookingStorage.Save(_bookingPath, bookings);

        return true;
    }

    public bool ModifyBooking(Guid bookingId, Guid passengerId,Guid? newFlightId,FlightClass? newFlightClass)
    {
        if(bookingId == Guid.Empty||passengerId==Guid.Empty) 
            throw new ArgumentException("Invalid booking or passenger ID");
        
        if(newFlightId==null&&newFlightClass==null)
            throw new ArgumentException("No changes were provided");
        
        List<Booking> bookings = _bookingStorage.Load(_bookingPath);

        Booking booking = bookings.FirstOrDefault(b => b.Id == bookingId)
                        ?? throw new KeyNotFoundException("Booking not found");
        
        if (booking.PassengerId != passengerId)
            throw new UnauthorizedAccessException("You cannot modify another passenger's booking.");
        
        
        if (booking.Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify a cancelled booking");
        
        Flight originalFlight = _flightService.GetFlightById(booking.FlightId)
                                ?? throw new KeyNotFoundException("Original flight not found.");

        if (originalFlight.DepartureDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot modify a booking after its original flight has departed.");
        
        Guid flightId = newFlightId ?? booking.FlightId;
        FlightClass flightClass = newFlightClass ?? booking.Class;
        
        Flight  flight = _flightService.GetFlightById(flightId) 
                        ??  throw new KeyNotFoundException("Flight not found");
        
        if (flightId == booking.FlightId && flightClass == booking.Class)
            return false;

        if (flight.DepartureDate <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot modify a booking for a departed flight");
        
        if(flightId!=booking.FlightId&&_flightService.GetAvailableSeats(flightId) <= 0)
            throw new InvalidOperationException("No available seats on the new flight");
        
        decimal newPrice=_flightService.CalculateTotalPrice(flight, flightClass);
        
        booking.FlightId=flightId;
        booking.Class=flightClass;
        booking.TotalPrice=newPrice;
        
        _bookingStorage.Save(_bookingPath, bookings);
        return true;
    }
    
    public List<Booking> GetPassengerBookings(Guid passengerId)
    {
        if(passengerId==Guid.Empty)
            throw new ArgumentException("Passenger ID cannot be empty.", nameof(passengerId));
        
        var bookings = _bookingStorage.Load(_bookingPath);
        return bookings.Where(b => b.PassengerId == passengerId).ToList();
    }

    public Booking? GetBookingById(Guid bookingId)
    {
        if(bookingId==Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty.", nameof(bookingId));
        
        var bookings = _bookingStorage.Load(_bookingPath);
        return bookings.FirstOrDefault(b => b.Id == bookingId);
    }

    
    public List<Booking> FilterBookings(BookingFilterCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        if (criteria.MaxPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(criteria.MaxPrice), "MaxPrice cannot be negative");

        if (criteria.Class.HasValue && !Enum.IsDefined(criteria.Class.Value))
            throw new ArgumentOutOfRangeException(nameof(criteria.Class), "Invalid flight class");

        IEnumerable<Booking> query = _bookingStorage.Load(_bookingPath);

        var flights = _flightService.GetAllFlights().ToDictionary(f => f.Id);

        if (criteria.FlightId.HasValue)
            query = query.Where(b => b.FlightId == criteria.FlightId.Value);

        if (criteria.PassengerId.HasValue)
            query = query.Where(b => b.PassengerId == criteria.PassengerId.Value);

        if (criteria.Class.HasValue)
            query = query.Where(b => b.Class == criteria.Class.Value);

        if (criteria.MaxPrice.HasValue)
            query = query.Where(b => b.TotalPrice <= criteria.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(criteria.DepartureCountry))
        {
            query = query.Where(b => flights.TryGetValue(b.FlightId, out var flight) && 
                                     string.Equals(flight.DepartureCountry, criteria.DepartureCountry, 
                                         StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.DestinationCountry))
        {
            query = query.Where(b => flights.TryGetValue(b.FlightId, out var flight) &&
                                     string.Equals(flight.DestinationCountry, criteria.DestinationCountry,
                                         StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.DepartureAirport))
        {
            query = query.Where(b => flights.TryGetValue(b.FlightId, out var flight) &&
                                     string.Equals(flight.DepartureAirport, criteria.DepartureAirport,
                                         StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.ArrivalAirport))
        {
            query = query.Where(b => flights.TryGetValue(b.FlightId, out var flight) &&
                                     string.Equals(flight.ArrivalAirport, criteria.ArrivalAirport,
                                         StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.DepartureDate.HasValue)
        {
            query = query.Where(b => flights.TryGetValue(b.FlightId, out var flight) &&
                                     flight.DepartureDate.Date == criteria.DepartureDate.Value.Date);
        }
        return query.OrderBy(b => b.BookingDate).ToList();
    }

}
