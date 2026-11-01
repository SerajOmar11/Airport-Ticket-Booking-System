using System.ComponentModel.DataAnnotations;
using AirportTicketBookingSystem.Modules;
using AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;

namespace AirportTicketBookingSystem.Services;

public class FlightValidator
{
    public List<ValidationError> Validate(Flight flight, int rowNum)
    {
        ArgumentNullException.ThrowIfNull(flight);

        List<ValidationError> errors = new();
        List<ValidationResult> results = new();

        Validator.TryValidateObject(
            flight,
            new ValidationContext(flight),
            results,
            validateAllProperties:true
        );

        foreach (var result in results)
        {
            foreach (var field in result.MemberNames.DefaultIfEmpty(nameof(flight)))
            {
                errors.Add(new ValidationError
                {
                    RowNumber = rowNum,
                    FieldName = field,
                    ErrorMessage = result.ErrorMessage??"Invalid value."
                });
            }
        }

        if (flight.DepartureDate <= DateTime.UtcNow)
        {
            errors.Add(new ValidationError
            {
                RowNumber = rowNum,
                FieldName = nameof(flight.DepartureDate),
                ErrorMessage = "Departure date must be in the future."
            });
        }
        
        if (flight.ArrivalDate <= flight.DepartureDate)
        {
            errors.Add(new ValidationError
            {
                RowNumber = rowNum,
                FieldName = nameof(flight.ArrivalDate),
                ErrorMessage = "Arrival date must be after departure date."
            });
        }

        if (string.Equals(flight.DepartureAirport.Trim(),
                flight.ArrivalAirport.Trim(),
                StringComparison.OrdinalIgnoreCase)
            )
        {
            errors.Add(new ValidationError
            {
                RowNumber = rowNum,
                FieldName = nameof(flight.ArrivalAirport),
                ErrorMessage = "Arrival airport must differ from departure airport"
            });
        }
        
        return errors;
    }
}