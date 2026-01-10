using IO_projekt_symulator.Server.Hubs;
using IO_projekt_symulator.Server.Consumers;
using IO_projekt_symulator.Server.Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. DODAJ SIGNALR DO KONTENERA
builder.Services.AddSignalR();

builder.Services.AddSignalR();

// --- KONFIGURACJA MASSTRANSIT (RABBITMQ) ---
builder.Services.AddMassTransit(x =>
{
    // 1. Mówimy, ¿e mamy takiego konsumenta
    x.AddConsumer<DeviceCommandConsumer>();

    // 2. Konfigurujemy po³¹czenie z RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Only configure endpoints for actual consumers in this service.
        cfg.ConfigureEndpoints(context);

        // Outgoing device updates (published event)
        cfg.Message<DeviceUpdatedEvent>(m =>
        {
            m.SetEntityName("device-updated"); // exchange name
        });

        cfg.Publish<DeviceUpdatedEvent>(p =>
        {
            p.ExchangeType = "fanout";
        });

        // NOTE:
        // Do NOT declare a ReceiveEndpoint for device updates in the simulator.
        // The simulator only publishes DeviceUpdatedEvent; the control panel app should consume it.
    });
});
// ---------------------------------------------


builder.Services.AddControllers();
builder.Services.AddScoped<IO_projekt_symulator.Server.Services.IVirtualDeviceService, IO_projekt_symulator.Server.Services.VirtualDeviceService>();
builder.Services.AddHostedService<IO_projekt_symulator.Server.Services.SensorSimulationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// 1. Najpierw konkretne adresy (SignalR)
app.MapHub<DevicesHub>("/devicesHub");

// 2. Na samym koñcu "wszystko inne" (React)
app.MapFallbackToFile("/index.html");

app.Run();
