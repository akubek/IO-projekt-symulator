using IO_projekt_symulator.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. DODAJ SIGNALR DO KONTENERA
builder.Services.AddSignalR();
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
