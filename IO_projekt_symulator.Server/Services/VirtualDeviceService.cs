using IO_projekt_symulator.Server.Controllers;
using IO_projekt_symulator.Server.Hubs;
using IO_projekt_symulator.Server.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IO_projekt_symulator.Server.Services
{
    public class VirtualDeviceService : IVirtualDeviceService
    {
        // Używamy nowej klasy 'Device'
        private  ConcurrentDictionary<Guid, Device> _devices = new();
        // To jest Twój "Nadajnik". Pozwala wysłać wiadomość do wszystkich podłączonych klientów.
        private readonly IHubContext<DevicesHub> _hubContext;
        private readonly string _filePath = "devices_db.json"; // Nazwa pliku bazy
        public VirtualDeviceService(IHubContext<DevicesHub> hubContext, IHostApplicationLifetime lifetime)
        {
            _hubContext = hubContext;

            // Próba wczytania danych przy starcie
            LoadData();

            // Jeśli plik był pusty (pierwsze uruchomienie), dodaj startowe (opcjonalnie)
            if (_devices.IsEmpty)
            {
                // Tu możesz dodać te swoje startowe urządzenia testowe, jeśli chcesz
                // AddDevice("Test", DeviceType.@switch, "Salon", "Opis");
            }

            // Zarejestruj akcję zapisu przy zamykaniu aplikacji
            lifetime.ApplicationStopping.Register(SaveData);
        }

        // Metoda do zapisu do pliku
        private void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(_devices.Values, options);
                File.WriteAllText(_filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd zapisu danych: {ex.Message}");
            }
        }

        // Metoda do odczytu z pliku
        private void LoadData()
        {
            if (!File.Exists(_filePath)) return;

            try
            {
                var jsonString = File.ReadAllText(_filePath);
                var devicesList = JsonSerializer.Deserialize<List<Device>>(jsonString);

                if (devicesList != null)
                {
                    _devices = new ConcurrentDictionary<Guid, Device>(
                        devicesList.ToDictionary(d => d.Id, d => d)
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd odczytu danych: {ex.Message}");
            }
            // Dodajmy urządzenia testowe pasujące do nowego schematu
            //AddDevice("Światło Salon", DeviceType.@switch, "Salon", "Główne światło");
            //AddDevice("Roleta Duża", DeviceType.slider, "Salon", "Roleta okienna");
            //AddDevice("Temperatura", DeviceType.sensor, "Sypialnia", "Czujnik temperatury");
        }

        public Device AddDevice(CreateDeviceDto dto)
        {
            // Parsowanie typu (string -> enum)
            if (!Enum.TryParse<DeviceType>(dto.Type, true, out var deviceType))
            {
                deviceType = DeviceType.@switch; // Fallback, jeśli typ jest błędny
            }
            var device = new Device
            {
                Name = dto.Name,
                Type = deviceType,
                Location = dto.Location,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow // <-- Ustawiamy czas serwera
            };
            if (dto.Config != null)
            {
                device.Config.Readonly = dto.Config.Readonly;
                device.Config.Min = dto.Config.Min;
                device.Config.Max = dto.Config.Max;
                device.Config.Step = dto.Config.Step;
            }
            else
            {
                // TUTAJ BUDUJEMY URZĄDZENIE ZGODNIE ZE SCHEMATEM
                switch (deviceType)
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
            }
            if (dto.State != null)
            {
                // Jeśli frontend podał wartość startową, użyj jej
                if (dto.State.Value.HasValue)
                    device.State.Value = dto.State.Value.Value;

                device.State.Unit = dto.State.Unit;
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

        
        // --- ZMODYFIKOWANA METODA UPDATE ---
        public Device? UpdateDeviceState(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false)
        {
            if (!_devices.TryGetValue(id, out var device)) return null;

            if (device.Config.Readonly && !bypassReadOnly) return null;

            // Logika wykrywania zmian (dla SignalR)
            bool changed = false;

            // Aktualizuj wartość jeśli podano
            if (newValue.HasValue)
            {
                double oldValue = device.State.Value ?? 0;
                if (Math.Abs(oldValue - newValue.Value) > 0.001)
                {
                    device.State.Value = newValue.Value;
                    changed = true;
                }
            }

            // Aktualizuj jednostkę jeśli podano
            if (!string.IsNullOrEmpty(newUnit) && device.State.Unit != newUnit)
            {
                device.State.Unit = newUnit;
                changed = true;
            }

            // Wyślij powiadomienie tylko jeśli coś się zmieniło
            if (changed)
            {
                // UWAGA: Tutaj wysyłamy teraz cały obiekt stanu lub wartość, zależnie jak umówiliście się z frontendem.
                // Dla uproszczenia wysyłamy nową wartość, ale frontend może oczekiwać całego obiektu.
                // Jeśli frontend nasłuchuje (id, value), to zostawiamy tak:
                if (newValue.HasValue)
                    _hubContext.Clients.All.SendAsync("UpdateReceived", device.Id, newValue.Value);
            }

            return device;
        }
    }
}