using IO_projekt_symulator.Server.Controllers;
using IO_projekt_symulator.Server.DTOs;
using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_projekt_symulator.Tests
{
    internal class DevicesControllerTests
    {
        [Fact]
        public void UpdateState_AdminRequest_ShouldUpdateServiceAndNotifyClients()
        {
            // ARRANGE
            var deviceId = Guid.NewGuid();
            var newValue = 55.0;

            // Mockujemy Serwis (nie chcemy, żeby grzebał w plikach, chcemy tylko wiedzieć, czy został wywołany)
            var serviceMock = new Mock<IVirtualDeviceService>();

            // Konfigurujemy Mocka: "Jak ktoś zawoła UpdateDeviceState, zwróć mu sukces (obiekt Device)"
            serviceMock.Setup(s => s.UpdateDeviceState(deviceId, newValue, null, true))
                       .Returns(new Device { State = new DeviceState { Value = newValue } })
                       .Verifiable(); // Ważne: zaznaczamy, że będziemy weryfikować to wywołanie

            var controller = new DevicesController(serviceMock.Object);

            // ACT
            var result = controller.UpdateState(deviceId, new UpdateStateDto { Value = newValue });

            // ASSERT
            // 1. Sprawdzamy, czy dostaliśmy HTTP 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);

            // 2. KLUCZOWE: Weryfikujemy, czy Kontroler wywołał Serwis z flagą 'bypassReadOnly = true'
            // To udowadnia, że kontroler działa w trybie Administratora
            serviceMock.Verify(s => s.UpdateDeviceState(
                It.Is<Guid>(g => g == deviceId),
                newValue,
                null,
                true // <--- Tu sprawdzamy, czy kontroler wymusza uprawnienia Admina
            ), Times.Once);
        }
    }
}
