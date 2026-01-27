using System.Text.Json.Serialization;

namespace IO_projekt_symulator.Server.Models
{
    /// <summary>
    /// Enumeration defining the supported types of virtual devices.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DeviceType
    {
        /// <summary>
        /// Represents a binary state device (ON/OFF).
        /// </summary>
        @switch,

        /// <summary>
        /// Represents a device with a continuous range of values (e.g., dimmer, blinds).
        /// </summary>
        slider,

        /// <summary>
        /// Represents a read-only device that reports measurements (e.g., thermometer).
        /// </summary>
        sensor
    }
}