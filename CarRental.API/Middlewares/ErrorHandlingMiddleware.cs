using CarRental.Application.Exceptions;
using CarRental.Application.Responses;
using FluentValidation;
using Newtonsoft.Json;

namespace CarRental.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
        var response = context.Response;
        response.ContentType = "application/json";
        
        var baseResponse = new BaseResponse
        {
            Success = false
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                baseResponse.Message = validationEx.Message;
                baseResponse.ValidationErrors = validationEx.Errors
                    .Select(error => error.ErrorMessage)
                    .ToList();
                _logger.LogWarning("Validation error: {Message}", validationEx.Message);
                break;

            case BadRequestException badRequestEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                baseResponse.Message = badRequestEx.Message;
                _logger.LogWarning("Bad request: {Message}", badRequestEx.Message);
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = StatusCodes.Status404NotFound;
                baseResponse.Message = notFoundEx.Message;
                _logger.LogWarning("Resource not found: {Message}", notFoundEx.Message);
                break;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = StatusCodes.Status400BadRequest;
                baseResponse.Message = invalidOpEx.Message;
                _logger.LogWarning("Invalid operation: {Message}", invalidOpEx.Message);
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                baseResponse.Message = "An unexpected error occurred.";
                _logger.LogError(exception, "An unexpected error occurred: {Message}", exception.Message);
                break;
        }

        await response.WriteAsync(JsonConvert.SerializeObject(baseResponse));
    }
}