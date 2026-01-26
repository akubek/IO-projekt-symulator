using IO_projekt_symulator.Server.Hubs;
using MassTransit;
using IO_projekt_symulator.Server.Consumers;

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
        // Tutaj podajemy namiary na serwer RabbitMQ.
        // lokalnie na Dockerze, to s¹ domyœlne ustawienia:
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        // To automatycznie tworzy kolejki i podpina konsumenta
        cfg.ConfigureEndpoints(context);
    });
});
// ---------------------------------------------


builder.Services.AddControllers();
builder.Services.AddSingleton<IO_projekt_symulator.Server.Services.IVirtualDeviceService, IO_projekt_symulator.Server.Services.VirtualDeviceService>();
// Rejestrujemy nasza usluge dzialajaca w tle
builder.Services.AddHostedService<IO_projekt_symulator.Server.Services.SensorSimulationService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
