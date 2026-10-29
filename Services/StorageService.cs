using System.Text.Json;

namespace AirportTicketBookingSystem.Services;

public class StorageService<T>
{
    public List<T> Load(string filePath)
    {
        if (!File.Exists(filePath))
            return new List<T>();

        string json=File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();

        return JsonSerializer.Deserialize<List<T>>(json, _options) ?? throw new JsonException("Expected a JSON array, but found null.");
    }

    public void Save(string filePath, List<T> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        string json=JsonSerializer.Serialize(data, _options);
        string? directory=Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(filePath, json);
    }
}