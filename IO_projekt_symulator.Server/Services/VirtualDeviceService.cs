using IO_projekt_symulator.Server.Models;
using System.Collections.Concurrent;

namespace IO_projekt_symulator.Server.Services
{
    public class VirtualDeviceService : IVirtualDeviceService
    {
        // Używamy nowej klasy 'Device'
        private readonly ConcurrentDictionary<Guid, Device> _devices = new();

        public VirtualDeviceService()
        {
            // Dodajmy urządzenia testowe pasujące do nowego schematu
            AddDevice("Światło Salon", DeviceType.@switch, "Salon", "Główne światło");
            AddDevice("Roleta Duża", DeviceType.slider, "Salon", "Roleta okienna");
            AddDevice("Temperatura", DeviceType.sensor, "Sypialnia", "Czujnik temperatury");
        }

        public Device AddDevice(string name, DeviceType type, string? location, string? description)
        {
            var device = new Device
            {
                Name = name,
                Type = type,
                Location = location,
                Description = description
            };

            // TUTAJ BUDUJEMY URZĄDZENIE ZGODNIE ZE SCHEMATEM
            switch (type)
            {
                case DeviceType.@switch:
                    device.State.Value = 0; // 0 = OFF, 1 = ON
                    device.Config.Min = 0;
                    device.Config.Max = 1;
                    device.Config.Step = 1;
                    device.Config.Readonly = false;
                    break;

                case DeviceType.slider:
                    device.State.Value = 0; // 0%
                    device.Config.Min = 0;
                    device.Config.Max = 100;
                    device.Config.Step = 5;
                    device.Config.Readonly = false;
                    break;

                case DeviceType.sensor:
                    device.State.Value = 21.5; // Domyślna temp.
                    device.State.Unit = "°C";
                    device.Config.Min = -10;
                    device.Config.Max = 50;
                    device.Config.Readonly = true;
                    break;
            }

            _devices.TryAdd(device.Id, device);
            return device;
        }

        public Device? GetDeviceById(Guid id)
        {
            _devices.TryGetValue(id, out var device);
            return device;
        }

        public IEnumerable<Device> GetDevices()
        {
            return _devices.Values;
        }

        public bool RemoveDevice(Guid id)
        {
            return _devices.TryRemove(id, out _);
        }

        // Nowa, prostsza metoda aktualizacji
        public Device? UpdateDeviceState(Guid id, double newValue, bool bypassReadOnly = false)
        {
            if (!_devices.TryGetValue(id, out var device))
            {
                return null; // Nie ma urządzenia
            }

            // Ta linia jest kluczowa:
            // Blokujemy, TYLKO jeśli jest readonly I JEDNOCZEŚNIE nie chcemy tego ominąć
            if (device.Config.Readonly && !bypassReadOnly)
            {
                return null; // Zablokowane
            }

            device.State.Value = newValue;
            return device;
        }
    }
}