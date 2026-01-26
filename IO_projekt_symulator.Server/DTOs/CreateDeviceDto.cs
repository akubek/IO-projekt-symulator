using System.ComponentModel.DataAnnotations;

namespace IO_projekt_symulator.Server.DTOs
{
    /// <summary>
    /// Data Transfer Object used for creating a new virtual device.
    /// Contains all necessary initial configuration and state data.
    /// </summary>
    public class CreateDeviceDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        public string? Location { get; set; }
        public string? Description { get; set; }
        public DeviceStateDto? State { get; set; }
        public DeviceConfigDto? Config { get; set; }
        public bool Malfunctioning { get; set; } = false;
    }

    /// <summary>
    /// Represents the initial state of the device during creation.
    /// </summary>
    public class DeviceStateDto
    {
        public double? Value { get; set; }
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Represents the initial configuration constraints of the device during creation.
    /// </summary>
    public class DeviceConfigDto
    {
        public bool Readonly { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public double? Step { get; set; }
    }
}