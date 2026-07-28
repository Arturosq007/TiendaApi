using System.Text.Json;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate _next,
    ILogger<ExceptionHandlingMiddleware> _logger)
{
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

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            NotFoundException e => (404, e.Message),
            DomainException e => (400, e.Message),
            UnauthorizedAccessException => (401, "No autorizado."),
            _ => (500, "Ocurrió un error interno.")
        };

        // Solo logueamos el stack trace en errores inesperados
        if (statusCode == 500)
            _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
        else
            _logger.LogWarning("Error controlado: {Message}", ex.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = statusCode,
            message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}