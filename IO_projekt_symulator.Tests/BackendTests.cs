using IO_projekt_symulator.Server.Contracts;
using IO_projekt_symulator.Server.Controllers;
using IO_projekt_symulator.Server.DTOs;
using IO_projekt_symulator.Server.Hubs;
using IO_projekt_symulator.Server.Models;
// UWAGA: Tu musisz wpisać poprawne namespace'y ze swojego projektu:
using IO_projekt_symulator.Server.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
// using IO_projekt_symulator.Server.Models; // <-- Odkomentuj jeśli tu masz klasę Device

namespace IO_projekt_symulator.Tests
{
    public class BackendTests
    {
        // TEST 1: Logika Biznesowa i Bezpieczeństwo
        [Theory]
        [InlineData("sensor", true, false, 25.0, null)]  // Haker atakuje sensor -> NULL (odmowa)
        [InlineData("sensor", true, true, 25.0, 25.0)]   // Admin zmienia sensor -> SUKCES
        [InlineData("switch", false, false, 1.0, 1.0)]   // Panel zmienia switch -> SUKCES
        public void UpdateDeviceState_SecurityCheck(string type, bool isReadOnly, bool bypass, double val, double? expected)
        {
            // 1. ARRANGE (Przygotowanie)
            var hubMock = new Mock<IHubContext<DevicesHub>>();
            var busMock = new Mock<IBus>();

            // Tworzymy Twój serwis z "udawanym" SignalR i RabbitMQ
            var service = new VirtualDeviceService(hubMock.Object, busMock.Object);

            // Tworzymy sztuczne urządzenie
            var devId = Guid.NewGuid().ToString();
            var device = new Device
            {
                Id = devId,
                Type = type,
                Config = new DeviceConfig { Readonly = isReadOnly, Min = 0, Max = 100 },
                State = new DeviceState { Value = 0 }
            };

            // Wstrzykujemy urządzenie do serwisu (wymaga metody pomocniczej, o której pisałem wyżej)
            service.AddDeviceForTest(device);

            // 2. ACT (Akcja)
            // Zakładam, że Twoja metoda przyjmuje string lub Guid ID
            var result = service.UpdateDeviceState(devId, val, null, bypass);

            // 3. ASSERT (Sprawdzenie)
            if (expected == null)
            {
                Assert.Null(result); // Ma odrzucić
            }
            else
            {
                Assert.NotNull(result);
                Assert.Equal(expected, result.State.Value); // Ma zmienić wartość
            }
        }

        // TEST 2: Kontroler (Czy Admin ma flagę TRUE?)
        [Fact]
        public void Controller_ShouldCallService_WithAdminFlag()
        {
            // 1. ARRANGE
            var serviceMock = new Mock<IVirtualDeviceService>();
            var controller = new DevicesController(serviceMock.Object);
            var id = Guid.NewGuid();

            // Konfigurujemy mocka: "Gdy ktoś zawoła Update, zwróć cokolwiek (np. null)"
            serviceMock.Setup(s => s.UpdateDeviceState(It.IsAny<string>(), It.IsAny<double>(), null, true))
                       .Returns(new Device());

            // 2. ACT
            controller.UpdateState(id, new UpdateStateDto { Value = 55 });

            // 3. ASSERT
            // Sprawdzamy czy kontroler wywołał serwis DOKŁADNIE z flagą 'true' (ostatni parametr)
            serviceMock.Verify(s => s.UpdateDeviceState(
                id.ToString(),
                55,
                null,
                true // <--- To jest klucz testu: wymuszenie Admina
            ), Times.Once);
        }
    }
}