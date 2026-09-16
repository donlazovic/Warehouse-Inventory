using System.Text.Json;
using Warehouse.BusinessLayer.Common;

namespace Warehouse.Api.Extensions;

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
        catch (AppException ex)
        {
            await WriteAsync(context, ex.StatusCode, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(context, 401, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neocekivana greska prilikom obrade zahteva.");
            await WriteAsync(context, 500, "Doslo je do neocekivane greske.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new { statusCode, message });
        await context.Response.WriteAsync(payload);
    }
}
