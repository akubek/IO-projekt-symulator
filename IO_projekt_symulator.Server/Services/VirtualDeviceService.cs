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
    /// <summary>
    /// Core service responsible for managing the lifecycle and state of virtual devices.
    /// Handles persistence (JSON file), validation logic, and broadcasting updates via SignalR and RabbitMQ.
    /// </summary>
    public class VirtualDeviceService : IVirtualDeviceService
    {
        private ConcurrentDictionary<Guid, Device> _devices = new();
<<<<<<< HEAD
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
=======

        // SignalR context used to broadcast real-time updates to connected frontend clients.
        private readonly IHubContext<DevicesHub> _hubContext;

        // MassTransit bus for publishing integration events to other microservices (e.g. Control Panel).
        private readonly IBus _bus;

        private readonly string _filePath = "devices_db.json";

        public VirtualDeviceService(IHubContext<DevicesHub> hubContext, IBus bus)
        {
            _hubContext = hubContext;
            _bus = bus;
            LoadData();
>>>>>>> master
        }

        /// <summary>
        /// Persists the current state of devices to a local JSON file.
        /// </summary>
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
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads device data from the local JSON file upon startup.
        /// </summary>
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
                Console.WriteLine($"Error reading data: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new device based on the provided DTO and adds it to the collection.
        /// </summary>
        /// <param name="dto">Data transfer object containing device initialization details.</param>
        /// <returns>The created Device object.</returns>
        public Device AddDevice(CreateDeviceDto dto)
        {
            // Parse device type string to enum
            if (!Enum.TryParse<DeviceType>(dto.Type, true, out var deviceType))
            {
                deviceType = DeviceType.@switch; // Fallback default
            }

            var device = new Device
            {
                Name = dto.Name,
                Type = deviceType,
                Location = dto.Location,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
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
                // Initialize default configuration based on device type
                switch (deviceType)
                {
                    case DeviceType.@switch:
                        device.State.Value = 0;
                        device.Config.Min = 0;
                        device.Config.Max = 1;
                        device.Config.Step = 1;
                        device.Config.Readonly = false;
                        break;

                    case DeviceType.slider:
                        device.State.Value = 0;
                        device.Config.Min = 0;
                        device.Config.Max = 100;
                        device.Config.Step = 5;
                        device.Config.Readonly = false;
                        break;

                    case DeviceType.sensor:
                        device.State.Value = 21.5;
                        device.State.Unit = "°C";
                        device.Config.Min = -10;
                        device.Config.Max = 50;
                        device.Config.Readonly = true;
                        break;
                }
            }

            if (dto.State != null)
            {
                if (dto.State.Value.HasValue)
                    device.State.Value = dto.State.Value.Value;

                device.State.Unit = dto.State.Unit;
            }

            _devices.TryAdd(device.Id, device);
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

        /// <summary>
        /// Removes a device from the system by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the device to remove.</param>
        /// <returns>True if removal was successful; otherwise, false.</returns>
        public bool RemoveDevice(Guid id)
        {
            var removed = _devices.TryRemove(id, out _);
            if (removed)
            {
                SaveData();
            }
            return removed;
        }

<<<<<<< HEAD
        
        // --- ZMODYFIKOWANA METODA UPDATE ---
        public async Task<Device?> UpdateDeviceStateAsync(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false)
        {
            if (!_devices.TryGetValue(id, out var device)) return null;

            if (device.Malfunctioning) return null;

=======
        /// <summary>
        /// Updates the state of a specific device. Includes validation logic and security checks.
        /// </summary>
        /// <param name="id">Device ID.</param>
        /// <param name="newValue">New numeric value to set.</param>
        /// <param name="newUnit">New unit string (optional).</param>
        /// <param name="bypassReadOnly">If true, allows modification of ReadOnly devices (e.g. Admin override).</param>
        /// <returns>The updated Device object, or null if update failed/rejected.</returns>
        public Device? UpdateDeviceState(Guid id, double? newValue, string? newUnit, bool bypassReadOnly = false)
        {
            if (!_devices.TryGetValue(id, out var device)) return null;

            // Security check: Prevent modification of ReadOnly sensors unless authorized (bypassReadOnly)
>>>>>>> master
            if (device.Config.Readonly && !bypassReadOnly) return null;

            bool changed = false;

            if (newValue.HasValue)
            {
                double val = newValue.Value;

                // Validation: Clamp value within defined Min/Max limits
                if (device.Config.Min.HasValue && device.Config.Max.HasValue)
                {
                    val = Math.Clamp(val, device.Config.Min.Value, device.Config.Max.Value);
                }

                double oldValue = device.State.Value ?? 0;
                // Check if value actually changed (with tolerance for floating point)
                if (Math.Abs(oldValue - val) > 0.001)
                {
                    device.State.Value = val;
                    changed = true;
                }
            }

            if (!string.IsNullOrEmpty(newUnit) && device.State.Unit != newUnit)
            {
                device.State.Unit = newUnit;
                changed = true;
            }

            if (changed)
            {
                // Notify frontend clients via SignalR
                if (device.State.Value.HasValue)
                {
                    _hubContext.Clients.All.SendAsync("UpdateReceived", device.Id, device.State.Value.Value);
                }

                SaveData();

<<<<<<< HEAD
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
=======
                // Publish event to RabbitMQ for external subscribers
                _bus.Publish(new DeviceUpdatedEvent
                {
                    DeviceId = device.Id,
                    NewValue = device.State.Value ?? 0
>>>>>>> master
                });
            }

            return device;
        }

<<<<<<< HEAD
        // ... inne metody ...

        public async Task<bool> SetMalfunctionStateAsync(Guid id, bool isMalfunctioning)
=======
        /// <summary>
        /// Sets the malfunction state of a device (simulated breakdown).
        /// </summary>
        public bool SetMalfunctionState(Guid id, bool isMalfunctioning)
>>>>>>> master
        {
            if (!_devices.TryGetValue(id, out var device)) return false;

            device.Malfunctioning = isMalfunctioning;
            SaveData();

<<<<<<< HEAD
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
=======
            // Notify frontend about malfunction status change
            _hubContext.Clients.All.SendAsync("MalfunctionUpdate", id, isMalfunctioning);
>>>>>>> master

            return true;
        }

        /// <summary>
        /// Global flag to enable or disable the background simulation service.
        /// </summary>
        public bool IsSimulationEnabled { get; set; } = true;

        public void ToggleSimulation(bool enable)
        {
            IsSimulationEnabled = enable;
            _hubContext.Clients.All.SendAsync("SimulationStateChanged", enable);
        }

        /// <summary>
        /// Helper method for Unit Tests to inject devices without using DTOs.
        /// </summary>
        public void AddDeviceForTest(Device device)
        {
            _devices.TryAdd(device.Id, device);
        }
    }
}