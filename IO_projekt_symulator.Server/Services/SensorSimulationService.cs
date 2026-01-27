using IO_projekt_symulator.Server.Models;

namespace IO_projekt_symulator.Server.Services
{
    /// <summary>
    /// Background service that periodically updates sensor values to simulate
    /// environmental changes (e.g., temperature fluctuation).
    /// </summary>
    public class SensorSimulationService : BackgroundService
    {
        private readonly ILogger<SensorSimulationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SensorSimulationService(ILogger<SensorSimulationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Sensor Simulation Service started.");
            var rand = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                // Simulation tick interval
                await Task.Delay(TimeSpan.FromSeconds(50), stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var deviceService = scope.ServiceProvider.GetRequiredService<IVirtualDeviceService>();

                    if (!deviceService.IsSimulationEnabled)
                    {
                        continue;
                    }

                    // Fetch all sensors
                    var sensors = deviceService.GetDevices()
                        .Where(d => d.Type == DeviceType.sensor);

                    foreach (var sensor in sensors)
                    {
                        // Skip malfunctioning devices
                        if (sensor.Malfunctioning)
                        {
                            continue;
                        }

                        double currentVal = sensor.State.Value ?? 0;

                        // Simulate small fluctuation
                        double change = (rand.NextDouble() - 0.5);
                        double newVal = Math.Round(currentVal + change, 1);

                        // Ensure value stays within config limits
                        if (sensor.Config.Min.HasValue && sensor.Config.Max.HasValue)
                        {
                            newVal = Math.Clamp(newVal, sensor.Config.Min.Value, sensor.Config.Max.Value);
                        }

                        // 5. Aktualizacja w serwisie
                        deviceService.UpdateDeviceStateAsync(sensor.Id, newVal, null, true);

                        string unit = sensor.State.Unit ?? "";
                        _logger.LogInformation($"[Simulation] '{sensor.Name}': {newVal}{unit}");
                    }
                }
            }
        }
    }
}