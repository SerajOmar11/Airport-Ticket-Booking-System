namespace AirportTicketBookingSystem.Modules.Filtering_and_Validation_Modules;

public class ValidationError
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; }=string.Empty;
    public string ErrorMessage { get; set; }=string.Empty;
}