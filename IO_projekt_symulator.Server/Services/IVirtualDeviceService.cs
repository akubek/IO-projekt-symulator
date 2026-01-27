using IO_projekt_symulator.Server.DTOs;
using IO_projekt_symulator.Server.Models;

namespace IO_projekt_symulator.Server.Services
{
    /// <summary>
    /// Interface defining the contract for the Virtual Device Service.
    /// Manages device CRUD operations, state updates, and simulation control.
    /// </summary>
    public interface IVirtualDeviceService
    {
        /// <summary>
        /// Retrieves all registered devices.
        /// </summary>
        IEnumerable<Device> GetDevices();

        /// <summary>
        /// Retrieves a specific device by its unique identifier.
        /// </summary>
        Device? GetDeviceById(Guid id);

        /// <summary>
        /// Adds a new device to the system based on the provided DTO.
        /// </summary>
        Device AddDevice(CreateDeviceDto dto);

        /// <summary>
        /// Removes a device from the system.
        /// </summary>
        bool RemoveDevice(Guid id);

<<<<<<< HEAD
        // ZMIANA: Dodajemy 'unit' (może być null)
        Task<Device?> UpdateDeviceStateAsync(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false);
        Task<bool> SetMalfunctionStateAsync(Guid id, bool isMalfunctioning);
        // --- NOWOŚĆ: Sterowanie symulacją ---
        bool IsSimulationEnabled { get; set; } // Czy symulacja działa?
        void ToggleSimulation(bool enable);    // Włącznik/Wyłącznik
    }
=======
        /// <summary>
        /// Updates the value and unit of a device.
        /// </summary>
        /// <param name="id">Device ID.</param>
        /// <param name="newValue">The new numeric value.</param>
        /// <param name="newUnit">The new unit string (optional).</param>
        /// <param name="bypassReadOnly">Security flag to override ReadOnly restrictions (e.g. for Admin).</param>
        /// <returns>The updated device or null if not found/rejected.</returns>
        Device? UpdateDeviceState(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false);
>>>>>>> master

        /// <summary>
        /// Sets the malfunction status of a device.
        /// </summary>
        bool SetMalfunctionState(Guid id, bool isMalfunctioning);

        /// <summary>
        /// Gets or sets the global simulation state (Running/Stopped).
        /// </summary>
        bool IsSimulationEnabled { get; set; }

        /// <summary>
        /// Toggles the background simulation on or off.
        /// </summary>
        void ToggleSimulation(bool enable);
    }
}