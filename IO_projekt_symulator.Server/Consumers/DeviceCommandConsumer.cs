using MassTransit;
using IO_projekt_symulator.Server.Services; // Żeby widzieć Twój serwis
using IO_projekt_symulator.Server.Contracts; // Żeby widzieć kontrakt

namespace IO_projekt_symulator.Server.Consumers
{
    // IConsumer<T> to interfejs z MassTransit
    public class DeviceCommandConsumer : IConsumer<SetDeviceStateCommand>
    {
        private readonly IVirtualDeviceService _deviceService;
        private readonly ILogger<DeviceCommandConsumer> _logger;

        // Wstrzykujemy Twój główny serwis (Singleton)
        public DeviceCommandConsumer(IVirtualDeviceService deviceService, ILogger<DeviceCommandConsumer> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        // --- TU JEST TWOJA LOGIKA BIZNESOWA ---
        // Wywołujemy UpdateDeviceState z parametrem bypassReadOnly = FALSE.
        // To oznacza: "Jeśli Panel próbuje zmienić sensor, wyrzuć null/błąd".
        public Task Consume(ConsumeContext<SetDeviceStateCommand> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"[RABBITMQ] Otrzymano komendę: ID={msg.DeviceId}, Val={msg.Value}");

            var result = _deviceService.UpdateDeviceState(
                msg.DeviceId,
                msg.Value,
                null, // Unit (zakładamy null)
                bypassReadOnly: false // <--- BLOKADA BEZPIECZEŃSTWA
            );

            if (result == null)
            {
                _logger.LogWarning($"[RABBITMQ] ODMOWA. Nie można zmienić urządzenia {msg.DeviceId} (może jest ReadOnly?)");
            }
            else
            {
                _logger.LogInformation($"[RABBITMQ] Zaktualizowano pomyślnie.");
            }

            return Task.CompletedTask;
        }
    }
}