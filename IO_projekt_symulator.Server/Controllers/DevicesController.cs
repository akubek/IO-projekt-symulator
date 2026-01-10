using IO_projekt_symulator.Server.Models;
using IO_projekt_symulator.Server.Services;
using Microsoft.AspNetCore.Mvc;
using IO_projekt_symulator.Server.DTOs; // <--- Dodajemy ten using, żeby widział folder DTOs

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
            // Przekazujemy całe DTO do serwisu
            var newDevice = _deviceService.AddDevice(dto);
            return CreatedAtAction(nameof(GetDevice), new { id = newDevice.Id }, newDevice);
        }

        // POST /api/devices/{id}/state
        // Endpoint do zmiany stanu (dla Panelu Sterowania)
        [HttpPost("{id}/state")]
        public async Task<IActionResult> UpdateState(Guid id, [FromBody] UpdateStateDto dto)
        {
            _logger.LogInformation($"Otrzymano aktualizację dla {id}");

            var device = _deviceService.GetDeviceById(id);
            if (device == null) return NotFound("Device not found.");

            if (device.Malfunctioning) return Conflict("Device is malfunctioning.");

            var updatedDevice = await _deviceService.UpdateDeviceStateAsync(id, dto.Value, dto.Unit, bypassReadOnly: true);
            if (updatedDevice == null) return NotFound("Device not found or readonly.");

            return Ok(updatedDevice);
        }

        // DELETE /api/devices/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteDevice(Guid id)
        {
            if (_deviceService.RemoveDevice(id))
            {
                return NoContent(); // Sukces, brak treści
            }
            return NotFound();
        }


        [HttpPost("{id}/malfunction")]
        public async Task<IActionResult> SetMalfunction(Guid id, [FromBody] MalfunctionDto dto)
        {
            var success = await _deviceService.SetMalfunctionStateAsync(id, dto.Malfunctioning);

            if (!success)
            {
                return NotFound($"Nie znaleziono urządzenia o ID: {id}");
            }

            return Ok(new { message = $"Stan awarii urządzenia {id} ustawiony na: {dto.Malfunctioning}" });
        }


        // POST /api/devices/simulation
        // Body: true (włącz) lub false (wyłącz)
        [HttpPost("simulation")]
        public IActionResult ToggleSimulation([FromBody] bool enable)
        {
            _deviceService.ToggleSimulation(enable);

            var status = enable ? "URUCHOMIONA" : "ZATRZYMANA";
            _logger.LogInformation($"Symulacja została {status} przez admina.");

            return Ok(new { message = $"Symulacja {status}", isEnabled = enable });
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
