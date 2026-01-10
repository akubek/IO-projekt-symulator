using IO_projekt_symulator.Server.Controllers;
using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.DTOs; // <--- Dodajemy ten using, żeby widział folder DTOs

namespace IO_projekt_symulator.Server.Services
{
    public interface IVirtualDeviceService
    {
        // Zwracamy teraz nową klasę 'Device'
        IEnumerable<Device> GetDevices();
        Device? GetDeviceById(Guid id);

        // ZMIANA: Przyjmujemy całe DTO
        Device AddDevice(CreateDeviceDto dto);

        bool RemoveDevice(Guid id);

        // ZMIANA: Dodajemy 'unit' (może być null)
        Task<Device?> UpdateDeviceStateAsync(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false);
        Task<bool> SetMalfunctionStateAsync(Guid id, bool isMalfunctioning);
        // --- NOWOŚĆ: Sterowanie symulacją ---
        bool IsSimulationEnabled { get; set; } // Czy symulacja działa?
        void ToggleSimulation(bool enable);    // Włącznik/Wyłącznik
    }

}