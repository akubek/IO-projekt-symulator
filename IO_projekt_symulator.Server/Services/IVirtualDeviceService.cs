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
        Device? UpdateDeviceState(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false);
    }
}