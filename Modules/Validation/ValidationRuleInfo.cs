namespace AirportTicketBookingSystem.Modules.Validation;

public class ValidationRuleInfo
{
    public string FieldName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public List<string> Constraints { get; set; } = new();
}