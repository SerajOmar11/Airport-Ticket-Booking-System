
using System.Globalization;
using AirportTicketBookingSystem.Modules;

namespace AirportTicketBookingSystem.Application;

public static class ConsoleHelper
{
    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(value))
                return value;

            Console.WriteLine("This field is required.");
        }
    }

    public static string? ReadOptional(string prompt)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine()?.Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            string input = ReadRequired(prompt);

            if (int.TryParse(input, out int number) && number >= min && number <= max)
                return number;

            Console.WriteLine($"Enter a number between {min} and {max}.");
        }
    }

    public static Guid ReadGuid(string prompt)
    {
        while (true)
        {
            string input = ReadRequired(prompt);

            if (Guid.TryParse(input, out Guid id) && id != Guid.Empty)
                return id;

            Console.WriteLine("Invalid GUID.");
        }
    }

    public static decimal? ReadOptionalDecimal(string prompt)
    {
        while (true)
        {
            string? input = ReadOptional(prompt);
            if (input == null) return null;

            if (decimal.TryParse(input, NumberStyles.Number,
                    CultureInfo.InvariantCulture, out decimal value)
                && value >= 0)
                return value;

            Console.WriteLine("Enter a non-negative decimal number.");
        }
    }

    public static DateTime? ReadOptionalDate(string prompt)
    {
        while (true)
        {
            string? input = ReadOptional(prompt);
            if (input == null) return null;

            if (DateTime.TryParseExact(
                    input, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime date))
                return date;

            Console.WriteLine("Use yyyy-MM-dd.");
        }
    }

    public static FlightClass? ReadFlightClass(bool optional = false)
    {
        while (true)
        {
            Console.WriteLine("1. Economy");
            Console.WriteLine("2. Business");
            Console.WriteLine("3. First Class");

            string? input = optional
                ? ReadOptional("Class (Enter to skip): ")
                : ReadRequired("Class: ");

            if (optional && input == null)
                return null;

            if (int.TryParse(input, out int number) &&
                Enum.IsDefined(typeof(FlightClass), number))
                return (FlightClass)number;

            Console.WriteLine("Invalid flight class.");
        }
    }

    public static void Pause()
    {
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }

    public static void ShowError(Exception ex)
    {
        Console.WriteLine($"\nError: {ex.Message}");
    }
}
