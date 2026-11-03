using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AirportTicketBookingSystem.Modules.Models;
using AirportTicketBookingSystem.Modules.Validation;

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

        if (string.Equals(
                flight.DepartureAirport?.Trim(),
                flight.ArrivalAirport?.Trim(),
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
        
        if (string.Equals(
                flight.DepartureCountry?.Trim(),
                flight.DestinationCountry?.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationError
            {
                RowNumber = rowNum,
                FieldName = nameof(flight.DestinationCountry),
                ErrorMessage = "Destination country must differ from departure country."
            });
        }
        return errors;
    }

    public List<ValidationRuleInfo> GetValidationRules()
    {
        List<ValidationRuleInfo> rulesInfos = new();
        PropertyInfo[] propertyInfos = typeof(Flight).GetProperties();

        foreach (var property in propertyInfos)
        {
            ValidationRuleInfo ruleInfo = new()
            {
                FieldName = property.Name,
                DataType = property.PropertyType.Name,
            };

            var attributes = property.GetCustomAttributes<ValidationAttribute>();

            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case RequiredAttribute:
                        ruleInfo.Constraints.Add("Required");
                        break;
                    case RangeAttribute range:
                        ruleInfo.Constraints.Add($"Range: {range.Minimum}-{range.Maximum}");
                        break;
                    case StringLengthAttribute stringLength:
                        ruleInfo.Constraints.Add($"Length: {stringLength.MinimumLength}-{stringLength.MaximumLength}");
                        break;
                    default:
                        ruleInfo.Constraints.Add(attribute.GetType().Name.Replace("Attribute", ""));
                        break;
                }
            }
            if (property.Name == nameof(Flight.DepartureDate))
                ruleInfo.Constraints.Add("Must be in the future");

            if (property.Name == nameof(Flight.ArrivalDate))
                ruleInfo.Constraints.Add("Must be after DepartureDate");

            if (property.Name == nameof(Flight.ArrivalAirport))
                ruleInfo.Constraints.Add("Must differ from DepartureAirport");

            if (property.Name == nameof(Flight.DestinationCountry))
                ruleInfo.Constraints.Add("Must differ from DepartureCountry");
            
            rulesInfos.Add(ruleInfo);
        }
        return rulesInfos;
    }
}