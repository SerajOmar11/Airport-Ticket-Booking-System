using System.Globalization;
using System.Text;
using AirportTicketBookingSystem.Modules.Models;

namespace AirportTicketBookingSystem.Services.FlightServices;

public class CsvFlightParser
{
    private static readonly string[] RequiredHeaders =
    {
        "FlightNumber",
        "DepartureCountry",
        "DestinationCountry",
        "DepartureAirport",
        "ArrivalAirport",
        "DepartureDate",
        "ArrivalDate",
        "BasePrice",
        "TotalSeats"
    };

    public List<Flight> Parse(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("CSV file not found.", filePath);

        string content = File.ReadAllText(filePath);

        List<List<string>> records = ParseRecords(content);

        if (records.Count==0)
            throw new FormatException("CSV file is empty.");

        Dictionary<string,int> headers=BuildHeaders(records[0]);

        List<Flight>flights=new();

        for (int i=1;i<records.Count;i++)
        {
            List<string>fields=records[i];

            if (fields.Count != headers.Count)
            {
                throw new FormatException(
                    $"Row {i + 1}: Expected {headers.Count} fields, " + 
                    $"but found {fields.Count}.");
            }

            try 
            {
                Flight flight = ParseFlight(fields, headers);
                flights.Add(flight);
            }
            catch (FormatException ex) 
            {
                throw new FormatException($"Row {i + 1}: {ex.Message}", ex);
            }
            catch (OverflowException ex)
            {
                throw new FormatException($"Row {i + 1}: Numeric value is out of range.", ex);
            }
        }
        return flights;
    }

    private Dictionary<string, int> BuildHeaders(List<string> fields)
    {
        Dictionary<string,int>headers=new(StringComparer.OrdinalIgnoreCase);

        for (int i=0;i<fields.Count;i++)
        {
            string header=fields[i].Trim();

            if (string.IsNullOrWhiteSpace(header))
                throw new FormatException($"Header at column {i + 1} is empty.");

            if (!headers.TryAdd(header, i))
                throw new FormatException($"Duplicate header: {header}");
        }

        foreach (var header in RequiredHeaders)
        {
            if (!headers.ContainsKey(header))
                throw new FormatException($"Missing required header: {header}");
        }

        return headers;
    }

    private List<List<string>> ParseRecords(string content)
    {
        List<List<string>>records=new();
        List<string>fields=new();
        StringBuilder currentField=new();

        bool insideQuotes=false;
        bool fieldStarted=false;
        bool quoteClosed=false;

        void AddField()
        {
            fields.Add(currentField.ToString());
            currentField.Clear();
            fieldStarted = false;
            quoteClosed = false;
        }

        void AddRecord()
        {
            AddField();
            records.Add(fields);
            fields = new List<string>();
        }

        for (int i=0;i<content.Length;i++)
        {
            char c=content[i];

            if(insideQuotes)
            {
                if(c=='"')
                {
                    if (i+1<content.Length && content[i + 1]=='"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = false;
                        quoteClosed = true;
                    }
                }
                else
                    currentField.Append(c);

                continue;
            }

            if (c==',')
            {
                AddField();
            }
            else if (c=='\r' || c=='\n')
            {
                AddRecord();

                if (c=='\r' && i+1<content.Length && content[i + 1] == '\n')
                    i++;
            }
            else if (c == '"')
            {
                if (fieldStarted || quoteClosed || currentField.Length>0)
                    throw new FormatException("Unexpected quote in CSV field.");

                insideQuotes = true;
                fieldStarted = true;
            }
            else
            {
                if (quoteClosed)
                    throw new FormatException("Unexpected character after closing quote.");

                currentField.Append(c);
                fieldStarted = true;
            }
        }

        if (insideQuotes)
            throw new FormatException("CSV contains an unclosed quoted field.");

        if (fields.Count > 0 || currentField.Length > 0 || fieldStarted || quoteClosed)
            AddRecord();

        return records;
    }

    private Flight ParseFlight(List<string> fields, Dictionary<string, int> headers)
    {
        string Get(string name) => fields[headers[name]];

        return new Flight
        {
            FlightNumber = Get("FlightNumber").Trim(),
            DepartureCountry = Get("DepartureCountry").Trim(),
            DestinationCountry = Get("DestinationCountry").Trim(),
            DepartureAirport = Get("DepartureAirport").Trim(),
            ArrivalAirport = Get("ArrivalAirport").Trim(),

            DepartureDate = ParseDate(Get("DepartureDate")),
            ArrivalDate = ParseDate(Get("ArrivalDate")),

            BasePrice = decimal.Parse(
                Get("BasePrice"),
                NumberStyles.Number,
                CultureInfo.InvariantCulture),

            TotalSeats = int.Parse(
                Get("TotalSeats"),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture)
        };
    }

    private DateTime ParseDate(string value)
    {
        return DateTime.ParseExact(
            value,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind
        );
    }
}
