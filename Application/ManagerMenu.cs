using AirportTicketBookingSystem.Modules.Enums;
using AirportTicketBookingSystem.Modules.Filtering;
using AirportTicketBookingSystem.Modules.Models;
using AirportTicketBookingSystem.Modules.Validation;
using AirportTicketBookingSystem.Services.BookingServices;
using AirportTicketBookingSystem.Services.FlightServices;

namespace AirportTicketBookingSystem.Application;

public class ManagerMenu
{
    private readonly FlightImportService _importService;
    private readonly BookingService _bookingService;
    private readonly FlightValidator _validator;
    private readonly string _csvPath;

    public ManagerMenu(FlightImportService importService, BookingService bookingService,
        FlightValidator validator, string csvPath)
    {
        _importService = importService
            ?? throw new ArgumentNullException(nameof(importService));

        _bookingService = bookingService
            ?? throw new ArgumentNullException(nameof(bookingService));

        _validator = validator
            ?? throw new ArgumentNullException(nameof(validator));

        _csvPath = !string.IsNullOrWhiteSpace(csvPath)
            ? csvPath
            : throw new ArgumentException("Invalid CSV path.", nameof(csvPath));
    }

    public void Show()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("===== MANAGER MENU =====");
            Console.WriteLine("1. Import Flights from CSV");
            Console.WriteLine("2. Filter Bookings");
            Console.WriteLine("3. Display Flight Validation Rules");
            Console.WriteLine("0. Back");
            Console.WriteLine();

            int choice = ConsoleHelper.ReadInt("Choose: ", 0, 3);

            if (choice == 0)
                return;

            try
            {
                switch (choice)
                {
                    case 1:
                        ImportFlights();
                        break;
                    case 2:
                        FilterBookings();
                        break;
                    case 3:
                        DisplayValidationRules();
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

    private void ImportFlights()
    {
        ImportResult result = _importService.ImportFlights(_csvPath);

        if (!result.IsSuccessful)
        {
            Console.WriteLine();
            Console.WriteLine("Import failed:");
            Console.WriteLine();

            foreach (ValidationError error in result.Errors)
            {
                Console.WriteLine($"Row {error.RowNumber} | " + $"{error.FieldName}: {error.ErrorMessage}");
            }

            return;
        }

        ConsoleHelper.ShowSuccess($"Successfully imported {result.Flights.Count} flights.");
    }

    private void FilterBookings()
    {
        BookingFilterCriteria criteria = new();

        if (ConsoleHelper.Confirm("Filter by flight ID?"))
            criteria.FlightId = ConsoleHelper.ReadGuid("Flight ID: ");

        if (ConsoleHelper.Confirm("Filter by passenger ID?"))
            criteria.PassengerId = ConsoleHelper.ReadGuid("Passenger ID: ");

        criteria.DepartureCountry = ConsoleHelper.ReadOptional("Departure country: ");
        criteria.DestinationCountry = ConsoleHelper.ReadOptional("Destination country: ");
        criteria.DepartureAirport = ConsoleHelper.ReadOptional("Departure airport: ");
        criteria.ArrivalAirport = ConsoleHelper.ReadOptional("Arrival airport: ");
        criteria.DepartureDate = ConsoleHelper.ReadOptionalDate("Departure date (yyyy-MM-dd): ");
        criteria.Class = ConsoleHelper.ReadFlightClass(true);
        criteria.MaxPrice = ConsoleHelper.ReadOptionalDecimal("Maximum booking price: ");

        if (ConsoleHelper.Confirm("Filter by booking status?"))
        {
            Console.WriteLine("1. Confirmed");
            Console.WriteLine("2. Cancelled");

            int statusChoice = ConsoleHelper.ReadInt("Status: ", 1, 2);
            criteria.Status = (BookingStatus)statusChoice;
        }

        List<Booking> bookings = _bookingService.FilterBookings(criteria);

        Console.WriteLine();

        if (bookings.Count == 0)
        {
            Console.WriteLine("No bookings found.");
            return;
        }

        Console.WriteLine($"Found {bookings.Count} booking(s).");

        foreach (Booking booking in bookings)
        {
            Console.WriteLine();
            Console.WriteLine("================================");
            Console.WriteLine($"Booking ID: {booking.Id}");
            Console.WriteLine($"Passenger ID: {booking.PassengerId}");
            Console.WriteLine($"Flight ID: {booking.FlightId}");
            Console.WriteLine($"Class: {booking.Class}");
            Console.WriteLine($"Price: {booking.TotalPrice:C}");
            Console.WriteLine($"Status: {booking.Status}");
            Console.WriteLine($"Booking Date (UTC): {booking.BookingDate:u}");
        }
    }

    private void DisplayValidationRules()
    {
        List<ValidationRuleInfo> rules = _validator.GetValidationRules();

        Console.WriteLine();
        Console.WriteLine("FLIGHT VALIDATION RULES");
        Console.WriteLine();

        foreach (ValidationRuleInfo rule in rules)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"Field: {rule.FieldName}");
            Console.WriteLine($"Type: {rule.DataType}");
            Console.WriteLine("Constraints:");

            if (rule.Constraints.Count == 0)
                Console.WriteLine("- None");
            else
            {
                foreach (string constraint in rule.Constraints)
                {
                    Console.WriteLine($"- {constraint}");
                }
            }
            Console.WriteLine();
        }
    }
}