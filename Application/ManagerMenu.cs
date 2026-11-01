
using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;
using AirportTicketBookingSystem.Services;

namespace AirportTicketBookingSystem.Application;

public class ManagerMenu
{
    private readonly FlightImportService _importService;
    private readonly BookingService _bookingService;
    private readonly FlightValidator _validator;

    public ManagerMenu(
        FlightImportService importService,
        BookingService bookingService,
        FlightValidator validator)
    {
        _importService = importService;
        _bookingService = bookingService;
        _validator = validator;
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
        string csvPath = ConsoleHelper.ReadRequired("CSV file path: ");

        ImportResult result = _importService.ImportFlights(csvPath);

        if (!result.IsSuccessful)
        {
            Console.WriteLine("\nImport failed:");

            foreach (ValidationError error in result.Errors)
            {
                Console.WriteLine($"Row {error.RowNumber} | " + $"{error.FieldName}: {error.ErrorMessage}");
            }

            return;
        }

        Console.WriteLine($"Successfully imported {result.Flights.Count} flights.");
    }

    private void FilterBookings()
    {
        BookingFilterCriteria criteria = new();

        if (ConsoleHelper.ReadInt("Filter by flight ID? (1 Yes, 0 No): ", 0, 1) == 1)
            criteria.FlightId = ConsoleHelper.ReadGuid("Flight ID: ");

        if (ConsoleHelper.ReadInt("Filter by passenger ID? (1 Yes, 0 No): ", 0, 1) == 1)
            criteria.PassengerId = ConsoleHelper.ReadGuid("Passenger ID: ");

        criteria.DepartureCountry = ConsoleHelper.ReadOptional("Departure country: ");
        criteria.DestinationCountry = ConsoleHelper.ReadOptional("Destination country: ");
        criteria.DepartureAirport = ConsoleHelper.ReadOptional("Departure airport: ");
        criteria.ArrivalAirport = ConsoleHelper.ReadOptional("Arrival airport: ");
        criteria.DepartureDate = ConsoleHelper.ReadOptionalDate("Departure date (yyyy-MM-dd, Enter to skip): ");
        criteria.Class = ConsoleHelper.ReadFlightClass(optional: true);
        criteria.MaxPrice = ConsoleHelper.ReadOptionalDecimal("Maximum booking price (Enter to skip): ");

        List<Booking> bookings = _bookingService.FilterBookings(criteria);

        Console.WriteLine($"\nFound {bookings.Count} bookings.");

        foreach (Booking booking in bookings)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Booking ID: {booking.Id}");
            Console.WriteLine($"Passenger ID: {booking.PassengerId}");
            Console.WriteLine($"Flight ID: {booking.FlightId}");
            Console.WriteLine($"Class: {booking.Class}");
            Console.WriteLine($"Price: {booking.TotalPrice}");
            Console.WriteLine($"Status: {booking.Status}");
            Console.WriteLine($"Booking date (UTC): {booking.BookingDate:u}");
        }
    }

    private void DisplayValidationRules()
    {
        List<ValidationRuleInfo> rules = _validator.GetValidationRules();

        foreach (ValidationRuleInfo rule in rules)
        {
            Console.WriteLine("--------------------------------");
            Console.WriteLine($"Field: {rule.FieldName}");
            Console.WriteLine($"Type: {rule.DataType}");

            if (rule.Constraints.Count == 0)
            {
                Console.WriteLine("Constraints: None declared");
                continue;
            }

            foreach (string constraint in rule.Constraints)
                Console.WriteLine($"- {constraint}");
        }
    }
}
