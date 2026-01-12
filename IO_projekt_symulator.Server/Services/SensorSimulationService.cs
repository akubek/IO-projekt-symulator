using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Services;

namespace IO_projekt_symulator.Server.Services
{
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
            _logger.LogInformation("Serwis symulacji czujnikow uruchomiony.");
            var rand = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                // Czekamy 5 lub 10 sekund (zależy jak wolisz)
                await Task.Delay(TimeSpan.FromSeconds(50), stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var deviceService = scope.ServiceProvider.GetRequiredService<IVirtualDeviceService>();

                    // 1. Sprawdź globalny włącznik symulacji
                    if (!deviceService.IsSimulationEnabled)
                    {
                        continue;
                    }

                    // 2. Pobierz tylko sensory
                    var sensors = deviceService.GetDevices()
                        .Where(d => d.Type == DeviceType.sensor);

                    foreach (var sensor in sensors)
                    {
                        // 3. --- WAŻNE --- Ignoruj zepsute urządzenia (Malfunction)
                        if (sensor.Malfunctioning)
                        {
                            continue;
                        }

                        double currentVal = sensor.State.Value ?? 0;

                        // 4. Oblicz nową wartość
                        double change = (rand.NextDouble() - 0.5);
                        double newVal = Math.Round(currentVal + change, 1);

                        // Walidacja zakresu (Clamp)
                        if (sensor.Config.Min.HasValue && sensor.Config.Max.HasValue)
                        {
                            newVal = Math.Clamp(newVal, sensor.Config.Min.Value, sensor.Config.Max.Value);
                        }

                        // 5. Aktualizacja w serwisie
                        deviceService.UpdateDeviceState(sensor.Id, newVal, null, true);

                        // 6. --- POPRAWKA LOGÓW ---
                        // Pobieramy jednostkę z urządzenia. Jeśli null, to pusty string.
                        string unit = sensor.State.Unit ?? "";

                        // Logujemy uniwersalny komunikat
                        _logger.LogInformation($"[Symulator] '{sensor.Name}': {newVal}{unit}");
                    }
                }
            }
        }
    }
}