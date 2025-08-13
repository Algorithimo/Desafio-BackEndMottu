using Microsoft.AspNetCore.Mvc;
using MotorcycleRental.Application.DTOs.Rental;
using MotorcycleRental.Application.Interfaces;

namespace MotorcycleRental.Api.Controllers
{
    [Route("api/rentals")]
    public class RentalsController : BaseController
    {
        private readonly IRentalService _rentalService;

        public RentalsController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        /// <summary>
        /// Get rental by ID
        /// </summary>
        /// <param name="id">Rental ID</param>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _rentalService.GetByIdAsync(id);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get rentals by driver ID
        /// </summary>
        /// <param name="driverId">Driver ID</param>
        [HttpGet("driver/{driverId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDriverId(Guid driverId)
        {
            var result = await _rentalService.GetByDriverIdAsync(driverId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Create a new rental
        /// </summary>
        /// <param name="request">Rental data</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateRentalRequest request)
        {
            var result = await _rentalService.CreateAsync(request);
            return HandleResponse(result);
        }

        /// <summary>
        /// Simulate rental return to check costs
        /// </summary>
        /// <param name="id">Rental ID</param>
        /// <param name="request">Return date</param>
        [HttpPost("{id}/simulate-return")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SimulateReturn(Guid id, [FromBody] SimulateReturnRequest request)
        {
            var result = await _rentalService.SimulateReturnAsync(id, request);
            return HandleResponse(result);
        }

        /// <summary>
        /// Process rental return
        /// </summary>
        /// <param name="id">Rental ID</param>
        /// <param name="request">Return date</param>
        [HttpPost("{id}/return")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessReturn(Guid id, [FromBody] ProcessReturnRequest request)
        {
            var result = await _rentalService.ProcessReturnAsync(id, request);
            return HandleResponse(result);
        }
    }
}