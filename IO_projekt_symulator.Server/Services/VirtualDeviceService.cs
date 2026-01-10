using IO_projekt_symulator.Server.Contracts;
using IO_projekt_symulator.Server.DTOs;
using IO_projekt_symulator.Server.Hubs;
using IO_projekt_symulator.Server.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

namespace IO_projekt_symulator.Server.Services
{
    public class VirtualDeviceService : IVirtualDeviceService
    {
        private ConcurrentDictionary<Guid, Device> _devices = new();
        private readonly IHubContext<DevicesHub> _hubContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly string _filePath = "devices_db.json";

        public VirtualDeviceService(IHubContext<DevicesHub> hubContext, IPublishEndpoint publishEndpoint)
        {
            _hubContext = hubContext;
            _publishEndpoint = publishEndpoint;

            LoadData();

            if (_devices.IsEmpty)
            {
            }
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
            // --- POPRAWKA (Punkt 4): Zapisujemy natychmiast po dodaniu ---
            SaveData();
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
            // Próbujemy usunąć z pamięci RAM
            var removed = _devices.TryRemove(id, out _);

            // JEŚLI usunięto z RAMu, to NATYCHMIAST zapisz to do pliku!
            if (removed)
            {
                SaveData(); // <--- TEGO PRAWDOPODOBNIE CI BRAKUJE
            }

            return removed;
        }

        
        // --- ZMODYFIKOWANA METODA UPDATE ---
        public async Task<Device?> UpdateDeviceStateAsync(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false)
        {
            if (!_devices.TryGetValue(id, out var device)) return null;

            if (device.Config.Readonly && !bypassReadOnly) return null;

            // Logika wykrywania zmian (dla SignalR)
            bool changed = false;

            // Aktualizuj wartość jeśli podano
            if (newValue.HasValue)
            {
                double val = newValue.Value;

                // --- POPRAWKA (Punkt 2): Walidacja zakresu (Math.Clamp) ---
                // Sprawdzamy zakres tylko jeśli Min/Max są zdefiniowane w Configu
                if (device.Config.Min.HasValue && device.Config.Max.HasValue)
                {
                    val = Math.Clamp(val, device.Config.Min.Value, device.Config.Max.Value);
                }

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
                // Wysyłamy powiadomienie SignalR
                if (device.State.Value.HasValue)
                {
                    _hubContext.Clients.All.SendAsync("UpdateReceived", device.Id, device.State.Value.Value);
                }

                // --- POPRAWKA (Punkt 4): Zapisujemy natychmiast po aktualizacji ---
                SaveData();

                await _hubContext.Clients.All.SendAsync(
                    "DeviceUpdated",
                    new DeviceUpdatedEventDto
                    {
                        DeviceId = device.Id,
                        Value = device.State.Value,
                        Unit = device.State.Unit,
                        Malfunctioning = device.Malfunctioning
                    });

                await _publishEndpoint.Publish(new DeviceUpdatedEvent
                {
                    DeviceId = device.Id,
                    Value = device.State.Value,
                    Unit = device.State.Unit,
                    Malfunctioning = device.Malfunctioning
                });
            }

            return device;
        }

        // ... inne metody ...

        public async Task<bool> SetMalfunctionStateAsync(Guid id, bool isMalfunctioning)
        {
            if (!_devices.TryGetValue(id, out var device)) return false;

            // Aktualizujemy stan w pamięci
            device.Malfunctioning = isMalfunctioning;

            // 1. Zapisujemy zmianę do pliku (Persistence)
            SaveData();

            // 2. Powiadamiamy frontend przez SignalR (Real-time update)
            // Dzięki temu, jak jeden admin kliknie "Simulate Malfunction",
            // to wszystkim innym od razu zapali się czerwona lampka.
            // Używamy nazwy zdarzenia "DeviceUpdated" lub specyficznego "MalfunctionChanged"
            // Tutaj dla uproszczenia wysyłamy sygnał, że urządzenie się zmieniło.
            // Frontend prawdopodobnie nasłuchuje na zmiany lub odświeży listę.
            await _hubContext.Clients.All.SendAsync(
                "DeviceUpdated",
                new DeviceUpdatedEventDto
                {
                    DeviceId = id,
                    Value = device.State.Value,
                    Unit = device.State.Unit,
                    Malfunctioning = isMalfunctioning
                });

            await _publishEndpoint.Publish(new DeviceUpdatedEvent
            {
                DeviceId = id,
                Value = device.State.Value,
                Unit = device.State.Unit,
                Malfunctioning = isMalfunctioning
            });

            return true;
        }

        // Domyślnie symulacja działa (true)
        public bool IsSimulationEnabled { get; set; } = true;



        public void ToggleSimulation(bool enable)
        {
            IsSimulationEnabled = enable;
            // Opcjonalnie: Powiadom frontend, że symulacja stanęła (żeby przycisk zmienił kolor)
            _hubContext.Clients.All.SendAsync("SimulationStateChanged", enable);
        }
    }
}