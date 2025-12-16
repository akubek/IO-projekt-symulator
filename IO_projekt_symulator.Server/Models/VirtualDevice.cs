using System.Text.Json.Serialization;
using IO_projekt_symulator.Server.Models; // Upewnij się, że ta ścieżka do DeviceType jest poprawna

namespace IO_projekt_symulator.Server.Models
{
    /// <summary>
    /// Główna klasa urządzenia, która DOKŁADNIE pasuje
    /// do schematu JSON wymaganego przez frontend.
    /// </summary>
    public class Device
    {
        // Atrybuty [JsonPropertyName] zapewniają, że nazwy w JSON
        // będą pisane małymi literami, tak jak chce frontend.

        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public DeviceType Type { get; set; }

        [JsonPropertyName("state")]
        public DeviceState State { get; set; } = new();

        [JsonPropertyName("config")]
        public DeviceConfig Config { get; set; } = new();

        [JsonPropertyName("location")]
        public string? Location { get; set; } // '?' oznacza, że pole może być null

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("malfunctioning")]
        public bool Malfunctioning { get; set; } = false;
    }

    /// <summary>
    /// Reprezentuje obiekt "state" w schemacie JSON
    /// </summary>
    public class DeviceState
    {
        [JsonPropertyName("value")]
        public double? Value { get; set; } // Używamy double, bo JSON ma "number"

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

       
    }

    /// <summary>
    /// Reprezentuje obiekt "config" w schemacie JSON
    /// </summary>
    public class DeviceConfig
    {
        [JsonPropertyName("min")]
        public double? Min { get; set; }

        [JsonPropertyName("max")]
        public double? Max { get; set; }

        [JsonPropertyName("step")]
        public double? Step { get; set; }

        [JsonPropertyName("readonly")]
        public bool Readonly { get; set; } = false;

      
    }
}