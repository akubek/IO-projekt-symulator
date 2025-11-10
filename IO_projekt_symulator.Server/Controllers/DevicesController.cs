using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace IO_projekt_symulator.Server.Controllers
{
    [ApiController]
    [Route("api/devices")] // Nasz adres bazowy: /api/devices
    public class DevicesController : ControllerBase
    {
        private readonly IVirtualDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        // Wstrzykujemy serwis, ktory przechowuje stan
        public DevicesController(IVirtualDeviceService deviceService, ILogger<DevicesController> logger)
        {
            _deviceService = deviceService; 
            _logger = logger;
        }

        // ENDPOINT DLA PANELU I FRONTENDU SYMULATORA
        // GET /api/devices
        [HttpGet]
        public IActionResult GetAllDevices()
        {
            return Ok(_deviceService.GetDevices());
        }

        // ENDPOINT DLA PANELU I FRONTENDU SYMULATORA
        // GET /api/devices/{id}
        [HttpGet("{id}")]
        public IActionResult GetDevice(Guid id)
        {
            var device = _deviceService.GetDeviceById(id);
            if (device == null)
            {
                return NotFound(); // Nie znaleziono
            }
            return Ok(device);
        }

        // *** KLUCZOWY ENDPOINT DLA PANELU STEROWANIA ***
        // POST /api/devices/{id}/state
        [HttpPost("{id}/state")]
        public IActionResult UpdateState(Guid id, [FromBody] Dictionary<string, object> newState)
        {
            _logger.LogInformation($"Otrzymano polecenie zmiany stanu dla {id}: {newState}");

            var success = _deviceService.UpdateDeviceState(id, newState);
            if (!success)
            {
                return NotFound(); // Nie ma takiego urzadzenia
            }

            // Zwroc zaktualizowany stan urzadzenia
            return Ok(_deviceService.GetDeviceById(id));
        }



        // ... Tutaj dodasz endpointy dla Person 5 (np. [HttpPost] do tworzenia, [HttpDelete] do usuwania) ...

        // Przykladowy DTO (Data Transfer Object) dla Person 5
        public class CreateDeviceDto
        {
            // Ta mała zmiana sprawi, że API automatycznie zwróci błąd,
            // jeśli frontend (Osoba 5) zapomni podać nazwy.
            [System.ComponentModel.DataAnnotations.Required]
            public string Name { get; set; } = string.Empty;

            public DeviceType Type { get; set; }
        }

        // ENDPOINT DLA FRONTENDU SYMULATORA (PERSON 5)
        // POST /api/devices
        [HttpPost]
        public IActionResult CreateDevice([FromBody] CreateDeviceDto dto)
        {
            var newDevice = _deviceService.AddDevice(dto.Name, dto.Type);
            // Zwracamy status 201 (Created) z lokalizacja nowego zasobu
            return CreatedAtAction(nameof(GetDevice), new { id = newDevice.Id }, newDevice);
        }
    }
}

/*
plik DevicesController.cs jest już kompletny i zawiera:

GET /api/devices (dla Panelu i frontendu)

GET /api/devices/{id} (dla Panelu i frontendu)

POST /api/devices/{id}/state (dla Panelu)

POST /api/devices (dla Twojego kolegi z frontendu, Osoby 5)
 
 */
