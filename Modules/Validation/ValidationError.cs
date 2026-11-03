namespace AirportTicketBookingSystem.Modules.Validation;

public class ValidationError
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; }=string.Empty;
    public string RawValue { get; set; } = string.Empty;
    public string ErrorMessage { get; set; }=string.Empty;
}