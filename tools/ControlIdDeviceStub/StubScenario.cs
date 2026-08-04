using System.Text;

internal sealed record StubScenario(string Name, int DelayMs, string Endpoint, int ResponseBytes)
{
    public const int DefaultOversizedResponseBytes = 17 * 1024 * 1024;

    private static readonly HashSet<string> SupportedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "normal",
        "slow",
        "timeout",
        "bad-request",
        "unauthorized",
        "forbidden",
        "not-found",
        "conflict",
        "rate-limited",
        "server-error",
        "invalid-json",
        "truncated-json",
        "unexpected-json",
        "wrong-content-type",
        "oversized-response",
        "session-expired",
        "feature-unavailable",
        "network-drop"
    };

    public static StubScenario Normal { get; } = new("normal", 0, string.Empty, DefaultOversizedResponseBytes);
    public static IReadOnlyCollection<string> Names => SupportedNames.Order(StringComparer.OrdinalIgnoreCase).ToArray();

    public bool AppliesTo(string path)
    {
        return string.IsNullOrWhiteSpace(Endpoint) || string.Equals(Endpoint, path, StringComparison.OrdinalIgnoreCase);
    }

    public static StubScenario Create(string? name, int? delayMs, string? endpoint, int? responseBytes)
    {
        var normalizedName = string.IsNullOrWhiteSpace(name) ? "normal" : name.Trim().ToLowerInvariant();
        if (!SupportedNames.Contains(normalizedName))
            throw new ArgumentOutOfRangeException(nameof(name), $"Cenario desconhecido: {normalizedName}.");

        var normalizedEndpoint = string.IsNullOrWhiteSpace(endpoint)
            ? string.Empty
            : "/" + endpoint.Trim().TrimStart('/').ToLowerInvariant();
        var normalizedDelay = Math.Clamp(delayMs ?? (normalizedName == "timeout" ? 15_000 : 750), 0, 60_000);
        var normalizedBytes = Math.Clamp(responseBytes ?? DefaultOversizedResponseBytes, 1_048_577, 64 * 1024 * 1024);
        return new StubScenario(normalizedName, normalizedDelay, normalizedEndpoint, normalizedBytes);
    }
}

internal static class StubScenarioResponder
{
    public static async Task<IResult?> TryCreateAsync(HttpContext context, StubRuntimeState runtime, string path)
    {
        var scenario = runtime.Scenario;
        if (scenario.Name == "normal" || !scenario.AppliesTo(path))
            return null;

        if (scenario.Name is "slow" or "timeout")
        {
            await Task.Delay(scenario.DelayMs, context.RequestAborted);
            if (scenario.Name == "slow")
                return null;

            return Results.Json(new { error = "simulated_timeout" }, statusCode: StatusCodes.Status504GatewayTimeout);
        }

        if (scenario.Name == "session-expired")
        {
            if (path == "/session_is_valid.fcgi")
                return Results.Json(new { session_is_valid = false });

            if (context.Request.Query.ContainsKey("session"))
                return Error(StatusCodes.Status401Unauthorized, "simulated_session_expired");

            return null;
        }

        return scenario.Name switch
        {
            "bad-request" => Error(StatusCodes.Status400BadRequest, "simulated_bad_request"),
            "unauthorized" => Error(StatusCodes.Status401Unauthorized, "simulated_unauthorized"),
            "forbidden" => Error(StatusCodes.Status403Forbidden, "simulated_forbidden"),
            "not-found" or "feature-unavailable" => Error(StatusCodes.Status404NotFound, "simulated_feature_unavailable"),
            "conflict" => Error(StatusCodes.Status409Conflict, "simulated_conflict"),
            "rate-limited" => RateLimited(context),
            "server-error" => Error(StatusCodes.Status503ServiceUnavailable, "simulated_service_unavailable"),
            "invalid-json" => Results.Text("not-json", "application/json", Encoding.UTF8),
            "truncated-json" => Results.Text("{\"success\":", "application/json", Encoding.UTF8),
            "unexpected-json" => Results.Json(new { success = true, unexpected = new { nested = true, version = 999 } }),
            "wrong-content-type" => Results.Text("{\"success\":true}", "application/octet-stream", Encoding.UTF8),
            "oversized-response" => new StubOversizedResult(scenario.ResponseBytes),
            "network-drop" => new StubNetworkDropResult(),
            _ => null
        };
    }

    private static IResult Error(int statusCode, string code) => Results.Json(new { error = code }, statusCode: statusCode);

    private static IResult RateLimited(HttpContext context)
    {
        context.Response.Headers.RetryAfter = "2";
        return Error(StatusCodes.Status429TooManyRequests, "simulated_rate_limit");
    }
}

internal sealed class StubOversizedResult(int responseBytes) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = "application/octet-stream";
        httpContext.Response.ContentLength = responseBytes;
        var chunk = new byte[81920];
        Array.Fill(chunk, (byte)'A');

        var remaining = responseBytes;
        while (remaining > 0)
        {
            var length = Math.Min(remaining, chunk.Length);
            await httpContext.Response.Body.WriteAsync(chunk.AsMemory(0, length), httpContext.RequestAborted);
            remaining -= length;
        }
    }
}

internal sealed class StubNetworkDropResult : IResult
{
    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Abort();
        return Task.CompletedTask;
    }
}
