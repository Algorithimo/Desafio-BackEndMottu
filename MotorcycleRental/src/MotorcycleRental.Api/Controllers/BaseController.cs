using Microsoft.AspNetCore.Mvc;
using MotorcycleRental.Application.DTOs.Common;

namespace MotorcycleRental.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }
    }
}