using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;

namespace AirportTicketBookingSystem.Services;

public class FlightImportService
{
    private readonly CsvFlightParser _parser;
    private readonly FlightValidator _validator;
    private readonly StorageService<Flight> _flightStorage;
    private readonly string _flightPath;

    public FlightImportService(
        CsvFlightParser parser,
        FlightValidator validator,
        StorageService<Flight> flightStorage,
        string flightPath) 
    {
        _parser = parser?? throw new ArgumentNullException(nameof(parser));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _flightStorage = flightStorage ?? throw new ArgumentNullException(nameof(flightStorage));
        _flightPath = !string.IsNullOrWhiteSpace(flightPath)
            ? flightPath
            : throw new ArgumentException("Flight path cannot be empty.", nameof(flightPath));
    }

    public ImportResult ImportFlights(string filePath)
    {
        ImportResult result = new();

        List<Flight> importedFlights = new();

        try
        {
            importedFlights = _parser.Parse(filePath);
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ValidationError
            {
                RowNumber = 0,
                FieldName = "CSV",
                ErrorMessage = ex.Message
            });
            return result;
        }

        if (importedFlights.Count == 0)
        {
            result.Errors.Add(new ValidationError
            {
                RowNumber = 0,
                FieldName = "CSV",
                ErrorMessage = "CSV contains no flights"
            });
            return result;
        }

        var existingFlights = _flightStorage.Load(_flightPath);
        var importedFlightsKeys = new HashSet<(string, DateTime)>();

        for (int i = 0; i < importedFlights.Count; i++)
        {
            Flight flight = importedFlights[i];
            
            int rowNumber = i+2;

            List<ValidationError> errors = _validator.Validate(flight, rowNumber);
            result.Errors.AddRange(errors);

            var flightKey = (
                flight.FlightNumber.Trim().ToUpperInvariant(),
                flight.DepartureDate
            );
            
            if (!importedFlightsKeys.Add(flightKey))
            {
                result.Errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    FieldName = nameof(flight.FlightNumber),
                    ErrorMessage = "Duplicate flight in the imported CSV"
                });
            }

            bool alreadyExists = existingFlights.Any(f =>
                string.Equals(
                    f.FlightNumber?.Trim(),
                    flight.FlightNumber.Trim(),
                    StringComparison.OrdinalIgnoreCase) &&
                f.DepartureDate == flight.DepartureDate);

            if (alreadyExists)
            {
                result.Errors.Add(new ValidationError
                {
                    RowNumber = rowNumber,
                    FieldName = nameof(flight.FlightNumber),
                    ErrorMessage = "This flight already exists in storage"
                });
            }
        }
        if (result.Errors.Count > 0)
        {
            return result;
        }
        
        existingFlights.AddRange(importedFlights);
        _flightStorage.Save(_flightPath,existingFlights);
        result.Flights.AddRange(importedFlights);

        return result;
    }
}