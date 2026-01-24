using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using MassTransit;
using IO_projekt_symulator.Server.Services;
using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Hubs;

namespace IO_projekt_symulator.Tests
{
    public class VirtualDeviceServiceTests
    {
        [Theory]
        // 0=Switch, 1=Slider, 2=Sensor
        [InlineData(2, true, false, 25.0, null)]   // Haker -> NULL
        [InlineData(2, true, true, 25.0, 25.0)]    // Admin -> SUKCES
        [InlineData(0, false, false, 1.0, 1.0)]    // Switch -> SUKCES
        [InlineData(1, false, true, 150.0, 100.0)] // Clamp -> 100
        public void UpdateDeviceState_ShouldEnforceSecurityAndValidationRules(
            int deviceTypeInt,
            bool isReadOnly,
            bool bypassReadOnly,
            double valueToSet,
            double? expectedValue)
        {
            // --- ARRANGE ---

            // 1. Mockowanie SignalR (Ten fragment naprawia błąd NullReference!)
            var hubMock = new Mock<IHubContext<DevicesHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();

            // Konfigurujemy łańcuszek: Hub -> Clients -> All
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);

            // 2. Mockowanie RabbitMQ (Bus)
            var busMock = new Mock<IBus>();
            // Upewniamy się, że Publish nie zwróci nulla (ważne!)
            busMock.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            // 3. Tworzenie serwisu
            var service = new VirtualDeviceService(hubMock.Object, busMock.Object);

            var deviceId = Guid.NewGuid();
            var mockDevice = new Device
            {
                Id = deviceId,
                Type = (DeviceType)deviceTypeInt,
                Config = new DeviceConfig { Readonly = isReadOnly, Min = 0, Max = 100 },
                State = new DeviceState { Value = 0 }
            };

            // Wstrzyknięcie urządzenia
            service.AddDeviceForTest(mockDevice);

            // --- ACT ---
            var result = service.UpdateDeviceState(deviceId, valueToSet, null, bypassReadOnly);

            // --- ASSERT ---
            if (expectedValue == null)
            {
                Assert.Null(result);
            }
            else
            {
                Assert.NotNull(result);
                Assert.Equal(expectedValue.Value, result.State.Value);
            }
        }
    }
}