using System;
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
        // Przekazujemy typ jako INT (0=Switch, 1=Slider, 2=Sensor)
        // Parametry: (TypeInt, ReadOnly, BypassReadOnly, ValueToSet, ExpectedValue)

        // Scenariusz 1: Haker (Panel) próbuje zmienić Czujnik (Sensor=2) -> POWINNO SIĘ NIE UDAĆ (null)
        [InlineData(2, true, false, 25.0, null)]

        // Scenariusz 2: Admin zmienia Czujnik -> POWINNO SIĘ UDAĆ (zmiana na 25.0)
        [InlineData(2, true, true, 25.0, 25.0)]

        // Scenariusz 3: Panel zmienia Switch (Switch=0) -> POWINNO SIĘ UDAĆ
        [InlineData(0, false, false, 1.0, 1.0)]

        // Scenariusz 4: Walidacja zakresu (Slider=1) - próba ustawienia 150 przy max 100 -> CLAMP do 100
        [InlineData(1, false, true, 150.0, 100.0)]
        public void UpdateDeviceState_ShouldEnforceSecurityAndValidationRules(
            int deviceTypeInt,
            bool isReadOnly,
            bool bypassReadOnly,
            double valueToSet,
            double? expectedValue)
        {
            // --- ARRANGE ---
            var hubMock = new Mock<IHubContext<DevicesHub>>();
           
            var busMock = new Mock<IBus>();
          
            busMock.Setup(b => b.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            // Tworzymy serwis
            var service = new VirtualDeviceService(hubMock.Object, busMock.Object);

            var deviceId = Guid.NewGuid();
            var deviceType = (DeviceType)deviceTypeInt; // Rzutowanie int na Enum

            var mockDevice = new Device
            {
                Id = deviceId,
                Type = deviceType,
                Config = new DeviceConfig { Readonly = isReadOnly, Min = 0, Max = 100 },
                State = new DeviceState { Value = 0 }
            };

            // Używamy metody pomocniczej, którą dodałaś do VirtualDeviceService.cs
            service.AddDeviceForTest(mockDevice);

            // --- ACT ---
            var result = service.UpdateDeviceState(deviceId, valueToSet, null, bypassReadOnly);

            // --- ASSERT ---
            if (expectedValue == null)
            {
                Assert.Null(result); // Oczekujemy odrzucenia zmiany (zwraca null)
            }
            else
            {
                Assert.NotNull(result);
                Assert.Equal(expectedValue.Value, result.State.Value); // Sprawdzamy czy wartość jest poprawna
            }
        }
    }
}