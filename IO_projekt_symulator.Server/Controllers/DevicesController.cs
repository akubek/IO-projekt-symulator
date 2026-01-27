using IO_projekt_symulator.Server.DTOs;
using IO_projekt_symulator.Server.Services;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
using IO_projekt_symulator.Server.DTOs; // <--- Dodajemy ten using, żeby widział folder DTOs

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
=======
>>>>>>> master

namespace IO_projekt_symulator.Server.Controllers
{
    /// <summary>
    /// API Controller for managing virtual IoT devices.
    /// Acts as the main entry point for the frontend application.
    /// </summary>
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

        /// <summary>
        /// Retrieves a list of all registered devices.
        /// </summary>
        [HttpGet]
        public IActionResult GetAllDevices()
        {
            return Ok(_deviceService.GetDevices());
        }

        /// <summary>
        /// Retrieves details of a specific device by ID.
        /// </summary>
        [HttpGet("{id}")]
        public IActionResult GetDevice(Guid id)
        {
            var device = _deviceService.GetDeviceById(id);
            if (device == null) return NotFound();
            return Ok(device);
        }

        /// <summary>
        /// Creates a new device in the simulator.
        /// </summary>
        [HttpPost]
        public IActionResult CreateDevice([FromBody] CreateDeviceDto dto)
        {
            var newDevice = _deviceService.AddDevice(dto);
            return CreatedAtAction(nameof(GetDevice), new { id = newDevice.Id }, newDevice);
        }

        /// <summary>
        /// Updates the state (value) of a device.
        /// This endpoint is used by the Admin Panel and bypasses ReadOnly restrictions.
        /// </summary>
        [HttpPost("{id}/state")]
        public async Task<IActionResult> UpdateState(Guid id, [FromBody] UpdateStateDto dto)
        {
            _logger.LogInformation($"Received state update request for device: {id}");

<<<<<<< HEAD
            var device = _deviceService.GetDeviceById(id);
            if (device == null) return NotFound("Device not found.");

            if (device.Malfunctioning) return Conflict("Device is malfunctioning.");

            var updatedDevice = await _deviceService.UpdateDeviceStateAsync(id, dto.Value, dto.Unit, bypassReadOnly: true);
            if (updatedDevice == null) return NotFound("Device not found or readonly.");
=======
            // Admin requests via API are trusted, so we set bypassReadOnly = true
            var updatedDevice = _deviceService.UpdateDeviceState(id, dto.Value, dto.Unit, bypassReadOnly: true);

            if (updatedDevice == null)
            {
                return NotFound("Device not found or readonly restriction applied.");
            }
>>>>>>> master

            return Ok(updatedDevice);
        }

        /// <summary>
        /// Deletes a device from the system.
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult DeleteDevice(Guid id)
        {
            if (_deviceService.RemoveDevice(id))
            {
                return NoContent();
            }
            return NotFound();
        }

        /// <summary>
        /// Toggles the simulated malfunction state of a device.
        /// </summary>
        [HttpPost("{id}/malfunction")]
        public async Task<IActionResult> SetMalfunction(Guid id, [FromBody] MalfunctionDto dto)
        {
            var success = await _deviceService.SetMalfunctionStateAsync(id, dto.Malfunctioning);

            if (!success)
            {
                return NotFound($"Device with ID {id} not found.");
            }

            return Ok(new { message = $"Malfunction state for {id} set to: {dto.Malfunctioning}" });
        }

        /// <summary>
        /// Globally enables or disables the background simulation service.
        /// </summary>
        [HttpPost("simulation")]
        public IActionResult ToggleSimulation([FromBody] bool enable)
        {
            _deviceService.ToggleSimulation(enable);
            var status = enable ? "STARTED" : "STOPPED";
            _logger.LogInformation($"Simulation {status} by admin.");
            return Ok(new { message = $"Simulation {status}", isEnabled = enable });
        }
    }
}