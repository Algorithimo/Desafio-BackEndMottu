using Microsoft.AspNetCore.Mvc;
using MotorcycleRental.Application.DTOs.Motorcycle;
using MotorcycleRental.Application.Interfaces;

namespace MotorcycleRental.Api.Controllers.Admin
{
    [Route("api/admin/motorcycles")]
    public class MotorcyclesController : BaseController
    {
        private readonly IMotorcycleService _motorcycleService;

        public MotorcyclesController(IMotorcycleService motorcycleService)
        {
            _motorcycleService = motorcycleService;
        }

        /// <summary>
        /// Get all motorcycles with optional plate filter
        /// </summary>
        /// <param name="plate">Optional plate filter</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? plate = null)
        {
            var result = await _motorcycleService.GetAllAsync(plate);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get motorcycle by ID
        /// </summary>
        /// <param name="id">Motorcycle ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _motorcycleService.GetByIdAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Create a new motorcycle
        /// </summary>
        /// <param name="request">Motorcycle data</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMotorcycleRequest request)
        {
            var result = await _motorcycleService.CreateAsync(request);
            return HandleResponse(result);
        }

        /// <summary>
        /// Update motorcycle plate
        /// </summary>
        /// <param name="id">Motorcycle ID</param>
        /// <param name="request">New plate data</param>
        [HttpPut("{id}/plate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlate(Guid id, [FromBody] UpdateMotorcyclePlateRequest request)
        {
            var result = await _motorcycleService.UpdatePlateAsync(id, request);
            return HandleResponse(result);
        }

        /// <summary>
        /// Delete a motorcycle
        /// </summary>
        /// <param name="id">Motorcycle ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _motorcycleService.DeleteAsync(id);
            return HandleResponse(result);
        }
    }
}