using System.Security.Cryptography;
using System.Text;

namespace IntegratingWithSwagger.Middleware;

public sealed class ApiKeyAuthenticationMiddleware
{
    public const string HeaderName = "X-Api-Key";
    public const string ConfigurationKey = "Authentication:ApiKey";

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
    private readonly string? _expectedApiKey;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthenticationMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _expectedApiKey = configuration[ConfigurationKey];
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Fail closed: no configured key means no writes, not open writes.
        if (string.IsNullOrWhiteSpace(_expectedApiKey))
        {
            _logger.LogError(
                "No API key configured at '{ConfigurationKey}'. Rejecting {Method} {Path}.",
                ConfigurationKey,
                context.Request.Method,
                context.Request.Path);

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Server misconfigured.",
                "The API is not configured with an API key, so write operations cannot be authorised.");
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedValues) || providedValues.Count != 1)
        {
            _logger.LogWarning(
                "Rejected {Method} {Path}: missing or duplicated '{HeaderName}' header.",
                context.Request.Method,
                context.Request.Path,
                HeaderName);

            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                $"A single '{HeaderName}' header is required for {context.Request.Method} requests.");
            return;
        }

        if (!IsExpectedKey(providedValues[0], _expectedApiKey))
        {
            // Don't log the supplied value.
            _logger.LogWarning(
                "Rejected {Method} {Path}: invalid '{HeaderName}' header.",
                context.Request.Method,
                context.Request.Path,
                HeaderName);

            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                $"The supplied '{HeaderName}' header is not valid.");
            return;
        }

        await _next(context);
    }

    // Constant-time comparison so the timing doesn't leak how much of the key matched.
    private static bool IsExpectedKey(string? provided, string expected)
    {
        if (string.IsNullOrEmpty(provided))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(provided),
            Encoding.UTF8.GetBytes(expected));
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        return TypedResults
            .Problem(detail: detail, statusCode: statusCode, title: title, instance: context.Request.Path)
            .ExecuteAsync(context);
    }
}
