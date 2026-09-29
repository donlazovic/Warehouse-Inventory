using Microsoft.AspNetCore.Mvc.Filters;
using Warehouse.BusinessLayer.Realtime;

namespace Warehouse.Api.Realtime;

public class RealtimeFlushFilter : IAsyncActionFilter
{
    private readonly IRealtimeDispatcher _dispatcher;
    private readonly ILogger<RealtimeFlushFilter> _logger;

    public RealtimeFlushFilter(IRealtimeDispatcher dispatcher, ILogger<RealtimeFlushFilter> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        var succeeded = executed.Exception is null || executed.ExceptionHandled;
        if (!succeeded)
            return;

        try
        {
            await _dispatcher.FlushAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Objavljivanje real-time dogadjaja nije uspelo. Zahtev je ipak izvrsen.");
        }
    }
}
