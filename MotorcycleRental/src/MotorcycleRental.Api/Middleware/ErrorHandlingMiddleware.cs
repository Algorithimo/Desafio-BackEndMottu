using System.Net;
using System.Text.Json;
using MotorcycleRental.Application.DTOs.Common;
using MotorcycleRental.Domain.Exceptions;

namespace MotorcycleRental.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ApiResponse<object>();

            switch (exception)
            {
                case BusinessRuleException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = ApiResponse<object>.FailureResponse($"{businessEx.Code}: {businessEx.Message}");
                    break;

                case EntityNotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse = ApiResponse<object>.FailureResponse(notFoundEx.Message);
                    break;

                case InvalidDomainOperationException invalidEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse = ApiResponse<object>.FailureResponse(invalidEx.Message);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = ApiResponse<object>.FailureResponse(
                        _environment.IsDevelopment()
                            ? exception.Message
                            : "An error occurred while processing your request"
                    );
                    break;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
            await response.WriteAsync(json);
        }
    }
}