using System.Text.Json;
using System.Text.Json.Serialization;

namespace AirportTicketBookingSystem.Services;

public class StorageService<T>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public List<T> Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.",nameof(filePath));
        
        if (!File.Exists(filePath))
            return new List<T>();

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();

        return JsonSerializer.Deserialize<List<T>>(json,Options) ?? throw new JsonException("Expected a JSON array, but found null.");
    }

    public void Save(string filePath,List<T> data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty.",nameof(filePath));
        
        ArgumentNullException.ThrowIfNull(data);

        string json = JsonSerializer.Serialize(data, Options);
        string? directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(filePath,json);
    }
}