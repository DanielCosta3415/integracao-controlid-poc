using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("forwarder", client =>
{
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(
        builder.Configuration.GetValue<int?>("Proxy:ForwardTimeoutSeconds") ?? 15,
        1,
        120));
});

var app = builder.Build();

var options = SigningProxyOptions.FromConfiguration(app.Configuration);
options.Validate();

app.MapMethods(
    "{**path}",
    ["GET", "POST"],
    async (
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
    {
        if (!options.IsPathAllowed(context.Request.Path))
            return Results.NotFound();

        if (!options.IsRemoteAddressAllowed(context.Connection.RemoteIpAddress))
            return Results.StatusCode(StatusCodes.Status403Forbidden);

        if (!options.HasInboundSharedKeyMatch(context.Request))
            return Results.StatusCode(StatusCodes.Status401Unauthorized);

        var bodyBytes = await ReadBodyAsync(context.Request, options.MaxBodyBytes, cancellationToken);
        if (bodyBytes == null)
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);

        var targetUri = options.BuildTargetUri(context.Request);
        using var forwardRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri)
        {
            Content = RequestHasBody(context.Request.Method)
                ? new ByteArrayContent(bodyBytes)
                : null
        };

        CopySafeHeaders(context.Request, forwardRequest);
        if (forwardRequest.Content != null &&
            !string.IsNullOrWhiteSpace(context.Request.ContentType))
        {
            forwardRequest.Content.Headers.ContentType =
                System.Net.Http.Headers.MediaTypeHeaderValue.Parse(context.Request.ContentType);
        }

        RemoveForwardedHeaders(
            forwardRequest,
            options.SharedKeyHeaderName,
            options.SignatureHeaderName,
            options.TimestampHeaderName,
            options.NonceHeaderName,
            options.InboundSharedKeyHeaderName);

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18));
        var body = Encoding.UTF8.GetString(bodyBytes);
        var signature = ComputeSignature(
            options.SharedKey,
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty,
            context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty,
            timestamp,
            nonce,
            body);

        forwardRequest.Headers.TryAddWithoutValidation(options.SharedKeyHeaderName, options.SharedKey);
        forwardRequest.Headers.TryAddWithoutValidation(options.SignatureHeaderName, signature);
        forwardRequest.Headers.TryAddWithoutValidation(options.TimestampHeaderName, timestamp);
        forwardRequest.Headers.TryAddWithoutValidation(options.NonceHeaderName, nonce);

        using var response = await httpClientFactory
            .CreateClient("forwarder")
            .SendAsync(forwardRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        var responseBody = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new ProxyResponseResult(
            (int)response.StatusCode,
            response.Content.Headers.ContentType?.ToString(),
            responseBody);
    });

app.Run();

static async Task<byte[]?> ReadBodyAsync(HttpRequest request, int maxBodyBytes, CancellationToken cancellationToken)
{
    if (!RequestHasBody(request.Method))
        return Array.Empty<byte>();

    if (request.ContentLength > maxBodyBytes)
        return null;

    using var memoryStream = new MemoryStream();
    var buffer = new byte[81920];
    var totalBytes = 0;
    int read;
    while ((read = await request.Body.ReadAsync(buffer, cancellationToken)) > 0)
    {
        totalBytes += read;
        if (totalBytes > maxBodyBytes)
            return null;

        await memoryStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
    }

    return memoryStream.ToArray();
}

static bool RequestHasBody(string method)
{
    return HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method);
}

static void CopySafeHeaders(HttpRequest request, HttpRequestMessage forwardRequest)
{
    foreach (var header in request.Headers)
    {
        if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
            header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase) ||
            header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        if (!forwardRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) &&
            forwardRequest.Content != null)
        {
            forwardRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }
}

static void RemoveForwardedHeaders(HttpRequestMessage forwardRequest, params string[] headerNames)
{
    foreach (var headerName in headerNames.Where(headerName => !string.IsNullOrWhiteSpace(headerName)))
    {
        forwardRequest.Headers.Remove(headerName);
        forwardRequest.Content?.Headers.Remove(headerName);
    }
}

static string ComputeSignature(
    string sharedKey,
    string method,
    string path,
    string queryString,
    string timestamp,
    string nonce,
    string body)
{
    var bodyHash = SHA256.HashData(Encoding.UTF8.GetBytes(body ?? string.Empty));
    var canonical = string.Join(
        "\n",
        method.ToUpperInvariant(),
        path,
        queryString,
        timestamp,
        nonce,
        Convert.ToBase64String(bodyHash));

    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedKey));
    return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
}

internal sealed class SigningProxyOptions
{
    public Uri ForwardBaseUrl { get; private init; } = new("http://localhost:5000");
    public string SharedKey { get; private init; } = string.Empty;
    public string SharedKeyHeaderName { get; private init; } = "X-ControlID-Callback-Key";
    public string SignatureHeaderName { get; private init; } = "X-ControlID-Signature";
    public string TimestampHeaderName { get; private init; } = "X-ControlID-Timestamp";
    public string NonceHeaderName { get; private init; } = "X-ControlID-Nonce";
    public string InboundSharedKey { get; private init; } = string.Empty;
    public string InboundSharedKeyHeaderName { get; private init; } = "X-ControlID-Proxy-Key";
    public int MaxBodyBytes { get; private init; } = 1024 * 1024;
    public bool AllowLoopback { get; private init; } = true;
    public IReadOnlyList<string> AllowedRemoteIps { get; private init; } = [];
    public IReadOnlyList<string> AllowedPathPrefixes { get; private init; } = DefaultAllowedPathPrefixes;

    private static readonly string[] DefaultAllowedPathPrefixes =
    [
        "/new_biometric_image.fcgi",
        "/new_biometric_template.fcgi",
        "/new_card.fcgi",
        "/new_qrcode.fcgi",
        "/new_uhf_tag.fcgi",
        "/new_user_id_and_password.fcgi",
        "/new_user_identified.fcgi",
        "/new_rex_log.fcgi",
        "/device_is_alive.fcgi",
        "/card_create.fcgi",
        "/fingerprint_create.fcgi",
        "/template_create.fcgi",
        "/face_create.fcgi",
        "/pin_create.fcgi",
        "/password_create.fcgi",
        "/api/notifications",
        "/user_get_image.fcgi",
        "/push",
        "/result",
        "/Push/Receive"
    ];

    public static SigningProxyOptions FromConfiguration(IConfiguration configuration)
    {
        var allowedPathPrefixes = configuration
            .GetSection("Proxy:AllowedPathPrefixes")
            .Get<string[]>();

        return new SigningProxyOptions
        {
            ForwardBaseUrl = new Uri(configuration["Proxy:ForwardBaseUrl"] ?? "http://localhost:5000"),
            SharedKey = ReadConfigValue(configuration, "Proxy:SharedKey"),
            SharedKeyHeaderName = configuration["Proxy:SharedKeyHeaderName"] ?? "X-ControlID-Callback-Key",
            SignatureHeaderName = configuration["Proxy:SignatureHeaderName"] ?? "X-ControlID-Signature",
            TimestampHeaderName = configuration["Proxy:TimestampHeaderName"] ?? "X-ControlID-Timestamp",
            NonceHeaderName = configuration["Proxy:NonceHeaderName"] ?? "X-ControlID-Nonce",
            InboundSharedKey = ReadConfigValue(configuration, "Proxy:InboundSharedKey"),
            InboundSharedKeyHeaderName = configuration["Proxy:InboundSharedKeyHeaderName"] ?? "X-ControlID-Proxy-Key",
            MaxBodyBytes = Math.Clamp(configuration.GetValue<int?>("Proxy:MaxBodyBytes") ?? 1024 * 1024, 1024, 10 * 1024 * 1024),
            AllowLoopback = configuration.GetValue<bool?>("Proxy:AllowLoopback") ?? true,
            AllowedRemoteIps = configuration
                .GetSection("Proxy:AllowedRemoteIps")
                .Get<string[]>() ?? [],
            AllowedPathPrefixes = allowedPathPrefixes is { Length: > 0 }
                ? allowedPathPrefixes
                : DefaultAllowedPathPrefixes
        };
    }

    private static string ReadConfigValue(IConfiguration configuration, string key)
    {
        return configuration[key] ?? string.Empty;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SharedKey))
            throw new InvalidOperationException("Proxy:SharedKey must be configured outside the repository.");

        if (AllowedPathPrefixes.Count == 0)
            throw new InvalidOperationException("Proxy:AllowedPathPrefixes must not be empty.");
    }

    public bool IsPathAllowed(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return AllowedPathPrefixes.Any(prefix =>
            value.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase));
    }

    public bool IsRemoteAddressAllowed(IPAddress? remoteAddress)
    {
        if (remoteAddress == null)
            return false;

        if (AllowLoopback && IPAddress.IsLoopback(remoteAddress))
            return true;

        if (AllowedRemoteIps.Count == 0)
            return false;

        foreach (var allowedIp in AllowedRemoteIps)
        {
            if (IPAddress.TryParse(allowedIp, out var parsedAllowedIp) &&
                parsedAllowedIp.Equals(remoteAddress))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasInboundSharedKeyMatch(HttpRequest request)
    {
        if (string.IsNullOrWhiteSpace(InboundSharedKey))
            return true;

        var provided = request.Headers[InboundSharedKeyHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(provided))
            return false;

        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(InboundSharedKey);
        return providedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }

    public Uri BuildTargetUri(HttpRequest request)
    {
        var builder = new UriBuilder(ForwardBaseUrl)
        {
            Path = request.Path.Value ?? string.Empty,
            Query = request.QueryString.HasValue
                ? request.QueryString.Value!.TrimStart('?')
                : string.Empty
        };

        return builder.Uri;
    }
}

internal sealed class ProxyResponseResult : IResult
{
    private readonly int _statusCode;
    private readonly string? _contentType;
    private readonly byte[] _body;

    public ProxyResponseResult(int statusCode, string? contentType, byte[] body)
    {
        _statusCode = statusCode;
        _contentType = contentType;
        _body = body;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;
        if (!string.IsNullOrWhiteSpace(_contentType))
            httpContext.Response.ContentType = _contentType;

        await httpContext.Response.Body.WriteAsync(_body);
    }
}
