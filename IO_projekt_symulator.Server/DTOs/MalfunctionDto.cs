

namespace IO_projekt_symulator.Server.DTOs
{
    /// <summary>
    /// Data Transfer Object used to toggle the simulated malfunction state of a device.
    /// </summary>
    public class MalfunctionDto
    {
        /// <summary>
        /// If true, the device enters a malfunction state and stops responding to standard commands.
        /// </summary>
        public bool Malfunctioning { get; set; }
    }
}