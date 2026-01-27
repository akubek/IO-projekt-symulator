using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MassTransit; // Potrzebne do ConsumeContext
using Microsoft.Extensions.Logging;
using IO_projekt_symulator.Server.Consumers;
using IO_projekt_symulator.Server.Services;
using IO_projekt_symulator.Server.Contracts;
using IO_projekt_symulator.Server.Models;

namespace IO_projekt_symulator.Tests
{
    public class DeviceCommandConsumerTests
    {
        [Fact]
        public async Task Consume_ShouldCallService_WithPanelSecurityFlag()
        {
            // --- ARRANGE ---

            // 1. Mockujemy Serwis
            var serviceMock = new Mock<IVirtualDeviceService>();

            // 2. Mockujemy Loggera
            var loggerMock = new Mock<ILogger<DeviceCommandConsumer>>();

            // 3. Mockujemy Kontekst MassTransit (czyli "kopertę" z wiadomością)
            var contextMock = new Mock<ConsumeContext<SetDeviceStateCommand>>();

            // Przygotowujemy przykładową wiadomość od Panelu
            var command = new SetDeviceStateCommand
            {
                DeviceId = Guid.NewGuid(),
                Value = 50.0
            };

            // Mówimy mockowi kontekstu: "Jak ktoś zapyta o Message, daj mu naszą komendę"
            contextMock.Setup(c => c.Message).Returns(command);

            // Tworzymy Konsumenta (to jest klasa, którą testujemy)
            var consumer = new DeviceCommandConsumer(serviceMock.Object, loggerMock.Object);

            // --- ACT ---
            // Symulujemy przyjście wiadomości (wywołujemy metodę Consume ręcznie)
            await consumer.Consume(contextMock.Object);

            // --- ASSERT ---
            // KLUCZOWE: Sprawdzamy, czy serwis został wywołany z flagą 'bypassReadOnly: false'
            // To udowadnia, że wejście od strony Panelu jest traktowane jako "niezaufane"
            serviceMock.Verify(s => s.UpdateDeviceState(
                command.DeviceId,
                command.Value,
                null,
                false // <--- To jest najważniejsza część testu! (Panel = false)
            ), Times.Once);
        }
    }
}