using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

public class UserIdLogActionFilter : IAsyncActionFilter
{
    private static readonly string[] UserIdClaimTypes = new[]
    {
        "sub",
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
        "nameidentifier",
        "userid"
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.Controller.GetType().Name;
        var action = context.ActionDescriptor.DisplayName;
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path;

        var userId = GetUserId(context.HttpContext);

        var stopwatch = Stopwatch.StartNew();

        Log.Information("Started {Controller}.{Action} | {Method} {Path} | UserID: {UserID}",
            controller, action, httpMethod, path, userId);

        var executedContext = await next();

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        if (executedContext.Exception != null)
        {
            Log.Error(executedContext.Exception,
                "Exception in {Controller}.{Action} | Elapsed: {Elapsed} ms | UserID: {UserID}",
                controller, action, elapsedMs, userId);
        }
        else
        {
            Log.Information("Finished {Controller}.{Action} | Elapsed: {Elapsed} ms | UserID: {UserID}",
                controller, action, elapsedMs, userId);
        }
    }

    private string GetUserId(HttpContext httpContext)
    {
        var user = httpContext.User;
        if (user?.Identity == null || !user.Identity.IsAuthenticated)
            return "Anonymous";

        foreach (var claimType in UserIdClaimTypes)
        {
            var claim = user.Claims.FirstOrDefault(c => string.Equals(c.Type, claimType, System.StringComparison.OrdinalIgnoreCase));
            if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                return claim.Value;
        }

        return "Authenticated(unknown-id)";
    }
}
