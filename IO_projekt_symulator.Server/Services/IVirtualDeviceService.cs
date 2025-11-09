using IO_projekt_symulator.Server.Models;

namespace IO_projekt_symulator.Server.Services
{
    public interface IVirtualDeviceService
    {
        // Pobiera wszystkie urzadzenia
        IEnumerable<VirtualDevice> GetDevices();

        // Pobiera jedno urzadzenie po ID
        VirtualDevice? GetDeviceById(Guid id);

        // Dodaje nowe urzadzenie (dla Twojego kolegi z frontendu Symulatora)
        VirtualDevice AddDevice(string name, DeviceType type);

        // Usuwa urzadzenie
        bool RemoveDevice(Guid id);

        // Kluczowa metoda: Aktualizuje stan urzadzenia (dla Panelu Sterowania)
        bool UpdateDeviceState(Guid id, Dictionary<string, object> newState);
    }
}
