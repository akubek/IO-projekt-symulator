using IO_projekt_symulator.Server.Models;
using System.Collections.Concurrent;

namespace IO_projekt_symulator.Server.Services
{
    public class VirtualDeviceService : IVirtualDeviceService
    {
        // Uzywamy ConcurrentDictionary, poniewaz jest to Singleton i wiele
        // watkow (np. rozne zapytania API) moze jednoczesnie z niego korzystac.
        private readonly ConcurrentDictionary<Guid, VirtualDevice> _devices = new();

        public VirtualDeviceService()
        {
            // Dodajmy jakies testowe urzadzenia na start
            AddDevice("Swiatlo Salon", DeviceType.Light);
            AddDevice("Czujnik Temp. Sypialnia", DeviceType.TemperatureSensor);
        }

        public VirtualDevice AddDevice(string name, DeviceType type)
        {
            var device = new VirtualDevice { Name = name, Type = type };

            // Inicjalizacja stanu poczatkowego
            switch (type)
            {
                case DeviceType.Light:
                    device.State["power"] = "off";
                    device.State["brightness"] = 100;
                    break;
                case DeviceType.TemperatureSensor:
                    device.State["temperature"] = 20.0;
                    break;
                    // ... itd. dla innych typow
            }

            _devices.TryAdd(device.Id, device);
            return device;
        }

        public VirtualDevice? GetDeviceById(Guid id)
        {
            _devices.TryGetValue(id, out var device);
            return device;
        }

        public IEnumerable<VirtualDevice> GetDevices()
        {
            return _devices.Values.OrderBy(d => d.Name);
        }

        public bool RemoveDevice(Guid id)
        {
            return _devices.TryRemove(id, out _);
        }

        public bool UpdateDeviceState(Guid id, Dictionary<string, object> newState)
        {
            if (!_devices.TryGetValue(id, out var device))
            {
                return false; // Nie ma takiego urzadzenia
            }

            // Aktualizujemy stan urzadzenia
            foreach (var entry in newState)
            {
                // Aktualizujemy istniejacy klucz lub dodajemy nowy
                device.State[entry.Key] = entry.Value;
            }
            return true;
        }
    }
}
