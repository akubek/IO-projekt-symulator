using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using IO_projekt_symulator.Server.Hubs;
using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Services;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

// ... inne importy
namespace IO_projekt_symulator.Tests
{
    public class VirtualDeviceServiceTests
    {
        [Theory]
        // Scenariusz 1: Haker (Panel) próbuje zmienić Czujnik -> POWINNO SIĘ NIE UDAĆ (null)
        [InlineData("Sensor", true, false, 25.0, null)]
        // Scenariusz 2: Admin zmienia Czujnik -> POWINNO SIĘ UDAĆ (zmiana na 25.0)
        [InlineData("Sensor", true, true, 25.0, 25.0)]
        // Scenariusz 3: Panel zmienia Switch -> POWINNO SIĘ UDAĆ
        [InlineData("Switch", false, false, 1.0, 1.0)]
        // Scenariusz 4: Walidacja zakresu (próba ustawienia 150 na dimmerze max 100) -> CLAMP do 100
        [InlineData("Dimmer", false, true, 150.0, 100.0)]
        public void UpdateDeviceState_ShouldEnforceSecurityAndValidationRules(
            string deviceType,
            bool isReadOnly,
            bool bypassReadOnly,
            double valueToSet,
            double? expectedValue)
        {
            // ARRANGE (Przygotowanie makiety urządzenia)
            var deviceId = Guid.NewGuid();
            var mockDevice = new Device
            {
                Id = deviceId.ToString(),
                Type = deviceType, // Np. Sensor
                Config = new DeviceConfig { Readonly = isReadOnly, Min = 0, Max = 100 },
                State = new DeviceState { Value = 0 }
            };

            // Mockujemy zależności (nie potrzebujemy prawdziwego SignalR ani plików)
            var hubMock = new Mock<IHubContext<DevicesHub>>();
            var busMock = new Mock<IBus>();

            // Tworzymy serwis wstrzykując mu nasze "udawane" zależności
            // (Zakładamy, że masz sposób na wstrzyknięcie listy urządzeń lub mock repozytorium)
            var service = new VirtualDeviceService(hubMock.Object, busMock.Object);
            service.ForceAddDeviceForTest(mockDevice); // Metoda pomocnicza w teście lub publiczna w serwisie

            // ACT (Wykonanie akcji)
            var result = service.UpdateDeviceState(deviceId, valueToSet, null, bypassReadOnly);

            // ASSERT (Sprawdzenie wyniku)
            if (expectedValue == null)
            {
                Assert.Null(result); // Oczekujemy odrzucenia zmiany
            }
            else
            {
                Assert.NotNull(result);
                Assert.Equal(expectedValue.Value, result.State.Value); // Sprawdzamy czy wartość jest taka jak oczekiwana (ew. przycięta)
            }
        }
    }
}
