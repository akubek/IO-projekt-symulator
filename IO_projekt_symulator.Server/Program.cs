using IO_projekt_symulator.Server.Hubs;
using IO_projekt_symulator.Server.Consumers;
using IO_projekt_symulator.Server.Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // <-- React
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
// 1.SIGNALR DO KONTENERA
builder.Services.AddSignalR();

// --- KONFIGURACJA MASSTRANSIT (RABBITMQ) ---
builder.Services.AddMassTransit(x =>
{
    // 1. Mówimy, ¿e mamy takiego konsumenta
    x.AddConsumer<DeviceCommandConsumer>();

    // 2. Konfigurujemy po³¹czenie z RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
<<<<<<< HEAD
        cfg.Host("localhost", "/", h =>
        {
=======
        // Tutaj podajemy namiary na serwer RabbitMQ.
        // lokalnie na Dockerze, to s¹ domyœlne ustawienia:
        cfg.Host("localhost", "/", h => {
>>>>>>> master
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

app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

//app.UseHttpsRedirection();

app.MapControllers();

// 1. Najpierw konkretne adresy (SignalR)
app.MapHub<DevicesHub>("/devicesHub");

// 2. Na samym koñcu "wszystko inne" (React)
app.MapFallbackToFile("/index.html");

app.Run();
