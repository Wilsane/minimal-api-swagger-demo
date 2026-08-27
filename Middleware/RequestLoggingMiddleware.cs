using System.Diagnostics;

namespace IntegratingWithSwagger.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTimestamp = Stopwatch.GetTimestamp();

        // Matches the traceId reported in error responses.
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogInformation(
            "--> {Method} {Path}{QueryString} (trace {TraceId})",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            traceId);

        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(startTimestamp);
            var endpointName = context.GetEndpoint()?.DisplayName ?? "(no endpoint matched)";

            _logger.LogInformation(
                "<-- {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:F2} ms [{Endpoint}]",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsed.TotalMilliseconds,
                endpointName);
        }
    }
}
