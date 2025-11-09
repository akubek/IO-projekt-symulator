using IO_projekt_symulator.Server.Models;

namespace IO_projekt_symulator.Server.Services
{
    // Ta usluga bedzie dzialac w tle przez caly cykl zycia aplikacji
    public class SensorSimulationService : BackgroundService
    {
        private readonly ILogger<SensorSimulationService> _logger;
        // Potrzebujemy providera serwisow, aby pobrac instancje Singletona
        private readonly IServiceProvider _serviceProvider;

        public SensorSimulationService(ILogger<SensorSimulationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serwis symulacji czujnikow uruchomiony.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Czekamy 10 sekund przed nastepna aktualizacja
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                // Uzywamy 'scope' aby pobrac nasz serwis
                using (var scope = _serviceProvider.CreateScope())
                {
                    var deviceService = scope.ServiceProvider.GetRequiredService<IVirtualDeviceService>();
                    SimulateTemperatureChange(deviceService);
                }
            }
        }

        private void SimulateTemperatureChange(IVirtualDeviceService deviceService)
        {
            var sensors = deviceService.GetDevices()
                .Where(d => d.Type == DeviceType.TemperatureSensor);

            var rand = new Random();

            foreach (var sensor in sensors)
            {
                double currentTemp; // Zmienna na obecną temperaturę

                // --- KROK 1: Bezpiecznie odczytaj temperaturę ---
                if (sensor.State["temperature"] is System.Text.Json.JsonElement jsonElement)
                {
                    // Jeśli to "pudełko" z API, otwórz je
                    currentTemp = jsonElement.GetDouble();
                }
                else
                {
                    // Jeśli to wartość startowa, użyj starej metody
                    currentTemp = Convert.ToDouble(sensor.State["temperature"]);
                }

                // --- KROK 2: Oblicz NOWĄ temperaturę (robimy to zawsze) ---
                double change = (rand.NextDouble() - 0.5); // Losowa zmiana +/- 0.5 stopnia
                double newTemp = Math.Round(currentTemp + change, 1); // Zastosuj zmianę do odczytanej temperatury

                // --- KROK 3: Zaktualizuj stan ---
                var newState = new Dictionary<string, object> { { "temperature", newTemp } };
                deviceService.UpdateDeviceState(sensor.Id, newState);

                _logger.LogInformation($"Symulator zmienil temperature dla '{sensor.Name}' na {newTemp}°C");
            }
        }
    }
}
