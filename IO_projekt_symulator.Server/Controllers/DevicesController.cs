using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Services;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace IO_projekt_symulator.Server.Controllers
{
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IVirtualDeviceService _deviceService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(IVirtualDeviceService deviceService, ILogger<DevicesController> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        // GET /api/devices
        // Ta metoda jest teraz zgodna z żądaniem frontendu 'loadDevices()'
        [HttpGet]
        public IActionResult GetAllDevices()
        {
            return Ok(_deviceService.GetDevices());
        }

        // GET /api/devices/{id}
        [HttpGet("{id}")]
        public IActionResult GetDevice(Guid id)
        {
            var device = _deviceService.GetDeviceById(id);
            if (device == null) return NotFound();
            return Ok(device);
        }

        // POST /api/devices
        // Endpoint do tworzenia urządzeń (dla Osoby 5)
        [HttpPost]
        public IActionResult CreateDevice([FromBody] CreateDeviceDto dto)
        {
            // Walidacja typu, aby frontend nie wysłał nam "lodowka"
            if (!Enum.TryParse<DeviceType>(dto.Type, true, out var deviceType))
            {
                return BadRequest("Invalid device type.");
            }

            var newDevice = _deviceService.AddDevice(dto.Name, deviceType, dto.Location, dto.Description);
            return CreatedAtAction(nameof(GetDevice), new { id = newDevice.Id }, newDevice);
        }

        // POST /api/devices/{id}/state
        // Endpoint do zmiany stanu (dla Panelu Sterowania)
        [HttpPost("{id}/state")]
        public IActionResult UpdateState(Guid id, [FromBody] UpdateStateDto dto)
        {
            _logger.LogInformation($"Otrzymano polecenie zmiany stanu dla {id}: {dto.Value}");

            var updatedDevice = _deviceService.UpdateDeviceState(id, dto.Value);
            if (updatedDevice == null)
            {
                return NotFound("Device not found or is read-only.");
            }

            return Ok(updatedDevice);
        }

        // DELETE /api/devices/{id}
        // Endpoint do usuwania urządzeń (dla Osoby 5)
        [HttpDelete("{id}")]
        public IActionResult DeleteDevice(Guid id)
        {
            if (_deviceService.RemoveDevice(id))
            {
                return NoContent(); // Sukces, brak treści
            }
            return NotFound();
        }
    }

    // --- NOWE KLASY DTO ---

    /// <summary>
    /// Dane, których oczekujemy od frontendu przy TWORZENIU urządzenia
    /// </summary>
    public class CreateDeviceDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty; // Przyjmujemy jako string
        public string? Location { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Dane, których oczekujemy od Panelu przy ZMIANIE STANU
    /// </summary>
    public class UpdateStateDto
    {
        [Required]
        public double Value { get; set; }
    }
}

/*
plik DevicesController.cs jest już kompletny i zawiera:

GET /api/devices (dla Panelu i frontendu)

GET /api/devices/{id} (dla Panelu i frontendu)

POST /api/devices/{id}/state (dla Panelu)

POST /api/devices (dla Twojego kolegi z frontendu, Osoby 5)
 
 */
