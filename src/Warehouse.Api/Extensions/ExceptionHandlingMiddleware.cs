using Microsoft.EntityFrameworkCore;
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
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Konflikt istovremene izmene.");
            await WriteAsync(context, 409,
                "Podatak je upravo izmenio drugi korisnik. Osvezite prikaz i pokusajte ponovo.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Upis odbijen zbog ogranicenja u bazi.");
            await WriteAsync(context, 409,
                "Operacija nije dozvoljena jer bi narusila integritet podataka.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neocekivana greska prilikom obrade zahteva.");
            await WriteAsync(context, 500, "Doslo je do neocekivane greske.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new { statusCode, message });
        await context.Response.WriteAsync(payload);
    }
}
