using System.ComponentModel.DataAnnotations;
using AirportTicketBookingSystem.Modules.Enums;
using AirportTicketBookingSystem.Modules.Filtering;
using AirportTicketBookingSystem.Modules.Models;
using AirportTicketBookingSystem.Services;
using AirportTicketBookingSystem.Services.BookingServices;
using AirportTicketBookingSystem.Services.FlightServices;

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
        _bookingService = bookingService
            ?? throw new ArgumentNullException(nameof(bookingService));

        _flightService = flightService
            ?? throw new ArgumentNullException(nameof(flightService));

        _passengerStorage = passengerStorage
            ?? throw new ArgumentNullException(nameof(passengerStorage));

        _passengerPath = !string.IsNullOrWhiteSpace(passengerPath)
            ? passengerPath
            : throw new ArgumentException("Passenger path cannot be empty.",nameof(passengerPath));
    }

    public void Show()
    {
        _currentPassengerId = null;

        while (true)
        {
            Console.Clear();

            Console.WriteLine("===== PASSENGER MENU =====");
            Console.WriteLine($"Passenger: {GetCurrentPassengerDisplayName()}");
            Console.WriteLine();

            Console.WriteLine("1. Register Passenger");
            Console.WriteLine("2. Select Existing Passenger");
            Console.WriteLine("3. Search Available Flights");
            Console.WriteLine("4. Book a Flight");
            Console.WriteLine("5. View My Bookings");
            Console.WriteLine("6. Modify Booking");
            Console.WriteLine("7. Cancel Booking");
            Console.WriteLine("0. Back");
            Console.WriteLine();

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

    private string GetCurrentPassengerDisplayName()
    {
        if (!_currentPassengerId.HasValue)
            return "None";

        Passenger? passenger = _passengerStorage
            .Load(_passengerPath)
            .FirstOrDefault(p => p.Id == _currentPassengerId.Value);

        if (passenger == null)
            return "None";

        return $"{passenger.FirstName} {passenger.LastName}";
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
            FirstName = ConsoleHelper.ReadRequired("First name: "),
            LastName = ConsoleHelper.ReadRequired("Last name: "),
            Email = ConsoleHelper.ReadRequired("Email: "),
            PassportNumber = ConsoleHelper.ReadRequired("Passport number: ")
        };

        List<ValidationResult> results = new();

        if (!Validator.TryValidateObject(passenger, new ValidationContext(passenger), results, true))
        {
            foreach (ValidationResult error in results)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return;
        }

        List<Passenger> passengers = _passengerStorage.Load(_passengerPath);

        if (passengers.Any(p => 
                string.Equals(p.Email, passenger.Email, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("This email is already registered.");
            return;
        }

        if (passengers.Any(p =>
                string.Equals(p.PassportNumber, passenger.PassportNumber, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("This passport is already registered.");
            return;
        }
        passengers.Add(passenger);
        _passengerStorage.Save(_passengerPath, passengers);
        _currentPassengerId = passenger.Id;
        
        ConsoleHelper.ShowSuccess("Passenger registered successfully.");
    }

    private void SelectPassenger()
    {
        List<Passenger> passengers = _passengerStorage.Load(_passengerPath);

        if (passengers.Count == 0)
        {
            Console.WriteLine("No passengers found.");
            return;
        }

        Console.WriteLine();

        for (int i = 0; i < passengers.Count; i++)
        {
            Passenger passenger = passengers[i];
            Console.WriteLine($"{i + 1}. {passenger.Email}");
        }

        int choice = ConsoleHelper.ReadInt("Choose passenger: ", 1, passengers.Count);

        Passenger selectedPassenger = passengers[choice - 1];

        _currentPassengerId = selectedPassenger.Id;

        ConsoleHelper.ShowSuccess($"Selected: {selectedPassenger.FirstName} {selectedPassenger.LastName}");
    }

    private void SearchFlights()
    {
        FlightSearchCriteria criteria = new()
        {
            DepartureCountry = ConsoleHelper.ReadOptional("Departure country: "),
            DestinationCountry = ConsoleHelper.ReadOptional("Destination country: "),
            DepartureAirport = ConsoleHelper.ReadOptional("Departure airport: "),
            ArrivalAirport = ConsoleHelper.ReadOptional("Arrival airport: "),
            DepartureDate = ConsoleHelper.ReadOptionalDate("Departure date (yyyy-MM-dd): "),
            Class = ConsoleHelper.ReadFlightClass(true),
            MaxPrice = ConsoleHelper.ReadOptionalDecimal("Maximum price: ")
        };

        List<Flight> flights = _flightService.SearchFlights(criteria);

        if (flights.Count == 0)
        {
            Console.WriteLine("No flights found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Found {flights.Count} flight(s).");

        foreach (Flight flight in flights)
        {
            decimal price = _flightService.CalculateTotalPrice(
                flight,
                criteria.Class ?? FlightClass.Economy);

            Console.WriteLine("--------------------------------");
            Console.WriteLine($"ID: {flight.Id}");
            Console.WriteLine($"Flight: {flight.FlightNumber}");
            Console.WriteLine($"{flight.DepartureCountry} -> {flight.DestinationCountry}");
            Console.WriteLine($"{flight.DepartureAirport} -> {flight.ArrivalAirport}");
            Console.WriteLine($"Departure (UTC): {flight.DepartureDate:u}");
            Console.WriteLine($"Price: {price}");
            Console.WriteLine($"Available Seats: {_flightService.GetAvailableSeats(flight.Id)}");
        }
    }

    private void BookFlight()
    {
        Guid passengerId = RequirePassenger();
        

        var flights=_flightService.GetAllFlights();
        if (flights.Count == 0)
        {
            Console.WriteLine("No flights found.");
            return;
        }
        Console.WriteLine();
        
        Console.WriteLine($"Found {flights.Count} flight(s).");

        int index = 1;
        foreach (var flight in flights)
        {
            Console.WriteLine($"{index++} - {flight.FlightNumber}");
            Console.WriteLine($"Departure : " +
                              $"{flight.DepartureCountry} - {flight.DepartureAirport}  - {flight.DepartureDate}");
            Console.WriteLine($"Destination : " +
                              $"{flight.DestinationCountry} - {flight.ArrivalAirport} - {flight.ArrivalDate}");
            Console.WriteLine($"Available Seats: {_flightService.GetAvailableSeats(flight.Id)}");
            Console.WriteLine();
        }
        
        int flightId = ConsoleHelper.ReadInt("Flight ID: ",1,index-1);
        FlightClass flightClass = ConsoleHelper.ReadFlightClass()!.Value;
        
        if (!ConsoleHelper.Confirm("Confirm booking?"))
            return;
        
        var chosenFlight=flights[flightId-1];
        Booking booking = _bookingService.BookFlight(chosenFlight.Id, passengerId, flightClass);

        ConsoleHelper.ShowSuccess("Booking created successfully.");
        Console.WriteLine($"Booking ID: {booking.Id}");
        Console.WriteLine($"Total Price: {booking.TotalPrice}");
    }

    private void ViewBookings()
    {
        Guid passengerId = RequirePassenger();

        List<Booking> bookings = _bookingService.GetPassengerBookings(passengerId);

        if (bookings.Count == 0)
        {
            Console.WriteLine("No bookings found.");
            return;
        }

        Console.WriteLine($"Found {bookings.Count} booking(s).");

        foreach (Booking booking in bookings)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Booking ID: {booking.Id}");
            Console.WriteLine($"Flight ID: {booking.FlightId}");
            Console.WriteLine($"Class: {booking.Class}");
            Console.WriteLine($"Price: {booking.TotalPrice}");
            Console.WriteLine($"Status: {booking.Status}");
            Console.WriteLine($"Booked At (UTC): {booking.BookingDate:u}");
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

        if (!ConsoleHelper.Confirm("Apply these changes?"))
            return;

        bool modified = _bookingService.ModifyBooking(bookingId, passengerId, newFlightId, newClass);

        if (modified)
            ConsoleHelper.ShowSuccess("Booking modified successfully.");
        else
            Console.WriteLine("No changes were made.");
    }

    private void CancelBooking()
    {
        Guid passengerId = RequirePassenger();

        Guid bookingId = ConsoleHelper.ReadGuid("Booking ID: ");

        if (!ConsoleHelper.Confirm("Are you sure you want to cancel this booking?"))
            return;

        bool cancelled = _bookingService.CancelBooking(bookingId, passengerId);

        if (cancelled)
            ConsoleHelper.ShowSuccess("Booking cancelled successfully.");
        else
            Console.WriteLine("Booking is already cancelled.");
    }
}