using Microsoft.AspNetCore.Mvc;
using MotorcycleRental.Application.DTOs.Driver;
using MotorcycleRental.Application.Interfaces;

namespace MotorcycleRental.Api.Controllers.Driver
{
    [Route("api/drivers")]
    public class DriversController : BaseController
    {
        private readonly IDriverService _driverService;

        public DriversController(IDriverService driverService)
        {
            _driverService = driverService;
        }

        /// <summary>
        /// Get driver by ID
        /// </summary>
        /// <param name="id">Driver ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _driverService.GetByIdAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get driver by identifier
        /// </summary>
        /// <param name="identifier">Driver identifier</param>
        [HttpGet("by-identifier/{identifier}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdentifier(string identifier)
        {
            var result = await _driverService.GetByIdentifierAsync(identifier);
            return HandleResponse(result);
        }

        /// <summary>
        /// Register a new driver
        /// </summary>
        /// <param name="request">Driver registration data</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] CreateDriverRequest request)
        {
            var result = await _driverService.CreateAsync(request);
            return HandleResponse(result);
        }

        /// <summary>
        /// Upload driver's CNH image
        /// </summary>
        /// <param name="id">Driver ID</param>
        /// <param name="file">CNH image file (PNG or BMP)</param>
        [HttpPost("{id}/cnh-image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCNHImage(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, errors = new[] { "File is required" } });
            }

            using var stream = file.OpenReadStream();
            var request = new UpdateDriverCNHImageRequest
            {
                ImageStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            var result = await _driverService.UpdateCNHImageAsync(id, request);
            return HandleResponse(result);
        }
    }
}