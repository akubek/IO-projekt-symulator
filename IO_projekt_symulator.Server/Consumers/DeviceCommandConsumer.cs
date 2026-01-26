using IO_projekt_symulator.Server.Contracts;
using IO_projekt_symulator.Server.Services;
using MassTransit;

namespace IO_projekt_symulator.Server.Consumers
{
    /// <summary>
    /// Consumes commands from the message bus (RabbitMQ) to update device states.
    /// Handles requests from external systems (e.g. Control Panel).
    /// </summary>
    public class DeviceCommandConsumer : IConsumer<SetDeviceStateCommand>
    {
        private readonly IVirtualDeviceService _deviceService;
        private readonly ILogger<DeviceCommandConsumer> _logger;

        public DeviceCommandConsumer(IVirtualDeviceService deviceService, ILogger<DeviceCommandConsumer> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SetDeviceStateCommand> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"[RabbitMQ] Received command: ID={msg.DeviceId}, Val={msg.Value}");

            // Process update with bypassReadOnly = false.
            // This ensures external systems cannot modify ReadOnly sensors.
            var result = _deviceService.UpdateDeviceState(
                msg.DeviceId,
                msg.Value,
                null,
                bypassReadOnly: false
            );

            if (result == null)
            {
                _logger.LogWarning($"[RabbitMQ] Update rejected for device {msg.DeviceId} (ReadOnly or Not Found).");
            }
            else
            {
                _logger.LogInformation($"[RabbitMQ] Update successful.");
            }

            return Task.CompletedTask;
        }
    }
}