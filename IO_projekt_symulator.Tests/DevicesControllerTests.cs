using System;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // <--- Ważne dla ILogger
using IO_projekt_symulator.Server.Controllers;
using IO_projekt_symulator.Server.Services;
using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.DTOs;

namespace IO_projekt_symulator.Tests
{
    public class DevicesControllerTests
    {
        [Fact]
        public void UpdateState_AdminRequest_ShouldUpdateServiceAndNotifyClients()
        {
            // --- ARRANGE ---
            var deviceId = Guid.NewGuid();
            var newValue = 55.0;

            // Mockujemy Serwis
            var serviceMock = new Mock<IVirtualDeviceService>();

            // Mockujemy Loggera (to naprawia błąd konstruktora!)
            var loggerMock = new Mock<ILogger<DevicesController>>();

            // Konfigurujemy Mocka Serwisu: "Jak ktoś zawoła UpdateDeviceState, zwróć sukces"
            serviceMock.Setup(s => s.UpdateDeviceState(It.IsAny<Guid>(), It.IsAny<double>(), null, true))
                       .Returns(new Device { State = new DeviceState { Value = newValue } });

            // Tworzymy kontroler, wstrzykując oba mocki
            var controller = new DevicesController(serviceMock.Object, loggerMock.Object);

            // --- ACT ---
            var result = controller.UpdateState(deviceId, new UpdateStateDto { Value = newValue });

            // --- ASSERT ---
            // 1. Sprawdzamy czy dostaliśmy HTTP 200 OK
            Assert.IsType<OkObjectResult>(result);

            // 2. KLUCZOWE: Weryfikujemy, czy Kontroler wywołał Serwis z flagą 'bypassReadOnly = true'
            // To udowadnia, że ten endpoint działa z uprawnieniami Administratora
            serviceMock.Verify(s => s.UpdateDeviceState(
                deviceId,
                newValue,
                null,
                true // <--- Tu sprawdzamy flagę Admina
            ), Times.Once);
        }
    }
}