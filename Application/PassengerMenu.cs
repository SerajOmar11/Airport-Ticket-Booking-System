
using System.ComponentModel.DataAnnotations;
using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;
using AirportTicketBookingSystem.Services;

namespace AirportTicketBookingSystem.Application;

public class PassengerMenu
{
    private readonly BookingService _bookingService;
    private readonly FlightService _flightService;
    private readonly StorageService<Passenger> _passengerStorage;
    private readonly string _passengerPath;

    private Guid? _currentPassengerId;

    public PassengerMenu(
        BookingService bookingService,
        FlightService flightService,
        StorageService<Passenger> passengerStorage,
        string passengerPath)
    {
        _bookingService = bookingService;
        _flightService = flightService;
        _passengerStorage = passengerStorage;
        _passengerPath = passengerPath;
    }

    public void Show()
    {
        _currentPassengerId = null;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("===== PASSENGER MENU =====");
            Console.WriteLine($"Passenger: {_currentPassengerId?.ToString() ?? "None"}");
            Console.WriteLine();
            Console.WriteLine("1. Register Passenger");
            Console.WriteLine("2. Select Existing Passenger");
            Console.WriteLine("3. Search Available Flights");
            Console.WriteLine("4. Book a Flight");
            Console.WriteLine("5. View My Bookings");
            Console.WriteLine("6. Modify Booking");
            Console.WriteLine("7. Cancel Booking");
            Console.WriteLine("0. Back");

            int choice = ConsoleHelper.ReadInt("Choose: ", 0, 7);

            if (choice == 0)
                return;

            try
            {
                switch (choice)
                {
                    case 1: 
                        RegisterPassenger(); 
                        break;
                    case 2: 
                        SelectPassenger(); 
                        break;
                    case 3: 
                        SearchFlights(); 
                        break;
                    case 4: 
                        BookFlight(); 
                        break;
                    case 5: 
                        ViewBookings(); 
                        break;
                    case 6: 
                        ModifyBooking(); 
                        break;
                    case 7: 
                        CancelBooking(); 
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.ShowError(ex);
            }

            ConsoleHelper.Pause();
        }
    }

    private Guid RequirePassenger()
    {
        if (!_currentPassengerId.HasValue)
            throw new InvalidOperationException("Register or select a passenger first.");

        return _currentPassengerId.Value;
    }

    private void RegisterPassenger()
    {
        Passenger passenger = new()
        {
            FullName = ConsoleHelper.ReadRequired("Full name: "),
            Email = ConsoleHelper.ReadRequired("Email: "),
            PassportNumber = ConsoleHelper.ReadRequired("Passport number: ")
        };

        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(passenger, new ValidationContext(passenger),
                results, true))
        {
            foreach (var error in results)
                Console.WriteLine(error.ErrorMessage);

            return;
        }

        var passengers = _passengerStorage.Load(_passengerPath);

        if (passengers.Any(p =>
                string.Equals(p.Email, passenger.Email,
                    StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("This email is already registered.");
            return;
        }

        passengers.Add(passenger);
        _passengerStorage.Save(_passengerPath, passengers);

        _currentPassengerId = passenger.Id;

        Console.WriteLine("Passenger registered successfully.");
        Console.WriteLine($"Your ID: {passenger.Id}");
    }

    private void SelectPassenger()
    {
        Guid id = ConsoleHelper.ReadGuid("Passenger ID: ");

        bool exists = _passengerStorage.Load(_passengerPath)
            .Any(p => p.Id == id);

        if (!exists)
        {
            Console.WriteLine("Passenger not found.");
            return;
        }

        _currentPassengerId = id;
        Console.WriteLine("Passenger selected.");
    }

    private void SearchFlights()
    {
        FlightSearchCriteria criteria = new()
        {
            DepartureCountry = ConsoleHelper.ReadOptional("Departure country: "),
            DestinationCountry = ConsoleHelper.ReadOptional("Destination country: "),
            DepartureAirport = ConsoleHelper.ReadOptional("Departure airport: "),
            ArrivalAirport = ConsoleHelper.ReadOptional("Arrival airport: "),
            DepartureDate = ConsoleHelper.ReadOptionalDate(
                "Departure date (yyyy-MM-dd, Enter to skip): "),
            Class = ConsoleHelper.ReadFlightClass(optional: true),
            MaxPrice = ConsoleHelper.ReadOptionalDecimal(
                "Maximum price (Enter to skip): ")
        };

        List<Flight> flights = _flightService.SearchFlights(criteria);

        Console.WriteLine($"\nFound {flights.Count} flights.");

        foreach (Flight flight in flights)
        {
            decimal price = _flightService.CalculateTotalPrice(
                flight, criteria.Class ?? FlightClass.Economy);

            Console.WriteLine("--------------------------------");
            Console.WriteLine($"ID: {flight.Id}");
            Console.WriteLine($"Flight: {flight.FlightNumber}");
            Console.WriteLine($"{flight.DepartureCountry} -> {flight.DestinationCountry}");
            Console.WriteLine($"{flight.DepartureAirport} -> {flight.ArrivalAirport}");
            Console.WriteLine($"Departure (UTC): {flight.DepartureDate:u}");
            Console.WriteLine($"Price: {price}");
            Console.WriteLine($"Available seats: {_flightService.GetAvailableSeats(flight.Id)}");
        }
    }

    private void BookFlight()
    {
        Guid passengerId = RequirePassenger();

        Guid flightId = ConsoleHelper.ReadGuid("Flight ID: ");
        FlightClass flightClass = ConsoleHelper.ReadFlightClass()!.Value;

        Booking booking = _bookingService.BookFlight(flightId, passengerId, flightClass);

        Console.WriteLine("Booking created successfully.");
        Console.WriteLine($"Booking ID: {booking.Id}");
        Console.WriteLine($"Total price: {booking.TotalPrice}");
    }

    private void ViewBookings()
    {
        Guid passengerId = RequirePassenger();

        var bookings = _bookingService.GetPassengerBookings(passengerId);

        if (bookings.Count == 0)
        {
            Console.WriteLine("No bookings found.");
            return;
        }

        foreach (Booking booking in bookings)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Booking ID: {booking.Id}");
            Console.WriteLine($"Flight ID: {booking.FlightId}");
            Console.WriteLine($"Class: {booking.Class}");
            Console.WriteLine($"Price: {booking.TotalPrice}");
            Console.WriteLine($"Status: {booking.Status}");
            Console.WriteLine($"Booked at (UTC): {booking.BookingDate:u}");
        }
    }

    private void ModifyBooking()
    {
        Guid passengerId = RequirePassenger();
        Guid bookingId = ConsoleHelper.ReadGuid("Booking ID: ");

        Console.WriteLine("1. Change flight");
        Console.WriteLine("2. Change class");
        Console.WriteLine("3. Change both");

        int choice = ConsoleHelper.ReadInt("Choose: ", 1, 3);

        Guid? newFlightId = null;
        FlightClass? newClass = null;

        if (choice is 1 or 3)
            newFlightId = ConsoleHelper.ReadGuid("New flight ID: ");

        if (choice is 2 or 3)
            newClass = ConsoleHelper.ReadFlightClass();

        bool modified = _bookingService.ModifyBooking(bookingId, passengerId, newFlightId, newClass);

        Console.WriteLine(modified ? "Booking modified successfully." : "No changes were made.");
    }

    private void CancelBooking()
    {
        Guid passengerId = RequirePassenger();
        Guid bookingId = ConsoleHelper.ReadGuid("Booking ID: ");

        bool cancelled = _bookingService.CancelBooking(bookingId, passengerId);

        Console.WriteLine(cancelled ? "Booking cancelled successfully." : "Booking is already cancelled.");
    }
}
