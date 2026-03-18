using System.Text.Json;
using BancoAnchoas.Application.Common.Exceptions;
using BancoAnchoas.Application.Common.Models;

namespace BancoAnchoas.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        var (statusCode, response) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest,
                new { Succeeded = false, Message = "Error de validación.", Errors = ve.Errors }),

            NotFoundException nf => (StatusCodes.Status404NotFound,
                new { Succeeded = false, Message = nf.Message, Errors = (IDictionary<string, string[]>)null! }),

            ForbiddenException fe => (StatusCodes.Status403Forbidden,
                new { Succeeded = false, Message = fe.Message, Errors = (IDictionary<string, string[]>)null! }),

            _ => (StatusCodes.Status500InternalServerError,
                new { Succeeded = false, Message = "Ha ocurrido un error interno.", Errors = (IDictionary<string, string[]>)null! })
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
