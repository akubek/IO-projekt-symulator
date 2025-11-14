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
                // Czekamy 10 sekund
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var deviceService = scope.ServiceProvider.GetRequiredService<IVirtualDeviceService>();

                    // 1. Znajdź wszystkie urządzenia, które są typu 'sensor'
                    var sensors = deviceService.GetDevices()
                        .Where(d => d.Type == DeviceType.sensor);

                    foreach (var sensor in sensors)
                    {
                        // 2. Odczytaj aktualną wartość (teraz z 'State.Value')
                        // Używamy '?? 0', aby bezpiecznie obsłużyć 'null'
                        double currentTemp = sensor.State.Value ?? 0;

                        // 3. Oblicz nową losową wartość
                        double change = (rand.NextDouble() - 0.5); // Losowa zmiana +/- 0.5
                        double newTemp = Math.Round(currentTemp + change, 1);

                        // 4. Zaktualizuj stan, omijając zabezpieczenie "readonly"
                        deviceService.UpdateDeviceState(sensor.Id, newTemp, true); // <-- Ustawiamy bypassReadOnly na true

                        _logger.LogInformation($"Symulator zmienil temperature dla '{sensor.Name}' na {newTemp}°C");
                    }
                }
            }
        }
    }
}