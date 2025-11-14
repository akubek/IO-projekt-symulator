using IO_projekt_symulator.Server.Models;

namespace IO_projekt_symulator.Server.Services
{
    public interface IVirtualDeviceService
    {
        // Zwracamy teraz nową klasę 'Device'
        IEnumerable<Device> GetDevices();
        Device? GetDeviceById(Guid id);

        // Ta metoda będzie teraz mądrzejsza
        Device AddDevice(string name, DeviceType type, string? location, string? description);

        bool RemoveDevice(Guid id);

        // Ta metoda też się zmienia - będziemy aktualizować tylko 'value'
        Device? UpdateDeviceState(Guid id, double newValue, bool bypassReadOnly = false);
    }
}