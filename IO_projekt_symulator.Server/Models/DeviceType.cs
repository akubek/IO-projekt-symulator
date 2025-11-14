using System.Text.Json.Serialization;

namespace IO_projekt_symulator.Server.Models
{
    // Ta linia automatycznie konwertuje nasz C# enum na tekst (string) w JSON-ie
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceType
    {
        // Używamy '@', ponieważ 'switch' to słowo kluczowe w C#
        @switch,
        slider,
        sensor
    }
}
