using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Logging;
using Integracao.ControlID.PoC.Middlewares;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.DocumentedFeatures;
using Integracao.ControlID.PoC.Services.Files;
using Integracao.ControlID.PoC.Services.Navigation;
using Integracao.ControlID.PoC.Services.OperationModes;
using Integracao.ControlID.PoC.Services.Observability;
using Integracao.ControlID.PoC.Services.Privacy;
using Integracao.ControlID.PoC.Services.Performance;
using Integracao.ControlID.PoC.Services.ProductSpecific;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.IO.Compression;
using System.Net;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configura Serilog
SeriLogConfiguration.ConfigureSerilog(builder.Host, builder.Configuration);

builder.Services.Configure<HostOptions>(options =>
{
    var shutdownSeconds = builder.Configuration.GetValue<int?>("Host:ShutdownTimeoutSeconds") ?? 30;
    options.ShutdownTimeout = TimeSpan.FromSeconds(Math.Clamp(shutdownSeconds, 5, 120));
});

var forwardedHeadersEnabled = builder.Configuration.GetValue<bool>("ForwardedHeaders:Enabled");
if (forwardedHeadersEnabled)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost;
        options.ForwardLimit = Math.Clamp(builder.Configuration.GetValue<int?>("ForwardedHeaders:ForwardLimit") ?? 1, 1, 10);

        var knownProxies = builder.Configuration
            .GetSection("ForwardedHeaders:KnownProxies")
            .Get<string[]>() ?? [];

        foreach (var proxy in knownProxies)
        {
            if (IPAddress.TryParse(proxy, out var address))
                options.KnownProxies.Add(address);
        }
    });
}

// Configura contexto do banco de dados SQLite
builder.Services.AddDbContext<IntegracaoControlIDContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services MVC
builder.Services.AddControllersWithViews(options =>
{
    var authenticatedUserPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(authenticatedUserPolicy));
});
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Integracao Control iD PoC",
        Version = "v1",
        Description = "Contratos HTTP locais da PoC para catalogo oficial, callbacks, monitoramento e push."
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<SqliteReadinessHealthCheck>("sqlite-runtime-state", tags: ["ready"]);
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
    [
        "application/json",
        "image/svg+xml"
    ]);
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
var callbackRateLimitPermitLimit = Math.Max(1, builder.Configuration.GetValue<int?>("CallbackSecurity:RateLimit:PermitLimit") ?? 120);
var callbackRateLimitWindowSeconds = Math.Clamp(builder.Configuration.GetValue<int?>("CallbackSecurity:RateLimit:WindowSeconds") ?? 60, 1, 3600);
var localAuthRateLimitPermitLimit = Math.Max(1, builder.Configuration.GetValue<int?>("Auth:RateLimit:PermitLimit") ?? 10);
var localAuthRateLimitWindowSeconds = Math.Clamp(builder.Configuration.GetValue<int?>("Auth:RateLimit:WindowSeconds") ?? 300, 30, 3600);
var interactiveRateLimitPermitLimit = Math.Max(1, builder.Configuration.GetValue<int?>("Security:InteractiveRateLimit:PermitLimit") ?? 600);
var interactiveRateLimitWindowSeconds = Math.Clamp(builder.Configuration.GetValue<int?>("Security:InteractiveRateLimit:WindowSeconds") ?? 60, 1, 3600);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.IsAuthenticated == true &&
            !string.IsNullOrWhiteSpace(httpContext.User.Identity.Name)
                ? $"user:{httpContext.User.Identity.Name}"
                : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = interactiveRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(interactiveRateLimitWindowSeconds),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.AddPolicy("CallbackIngress", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = callbackRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(callbackRateLimitWindowSeconds),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.AddPolicy("LocalAuth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = localAuthRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(localAuthRateLimitWindowSeconds),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
    options.AddPolicy("InteractiveUi", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = interactiveRateLimitPermitLimit,
                Window = TimeSpan.FromSeconds(interactiveRateLimitWindowSeconds),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = builder.Configuration["Auth:CookieName"] ?? ".IntegracaoControlID.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.LoginPath = "/Auth/LocalLogin";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(Math.Clamp(builder.Configuration.GetValue<int?>("Auth:IdleTimeoutMinutes") ?? 60, 5, 1440));
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole(AppSecurityRoles.Administrator));
});

// Sessão ASP.NET Core (30 min timeout, seguro)
builder.Services.AddSession(options =>
{
    options.Cookie.Name = builder.Configuration["Session:CookieName"] ?? ".IntegracaoControlID.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
        : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(int.Parse(builder.Configuration["Session:IdleTimeout"] ?? "30"));
});

// Registro da camada oficial Control iD (injeção de dependência)
builder.Services.AddHttpClient(); // HttpClientFactory
builder.Services.Configure<CallbackSecurityOptions>(builder.Configuration.GetSection("CallbackSecurity"));
builder.Services.Configure<ControlIdEgressOptions>(builder.Configuration.GetSection("ControlIDApi"));
builder.Services.Configure<ControlIdCircuitBreakerOptions>(builder.Configuration.GetSection("ControlIDApi:CircuitBreaker"));
builder.Services.AddScoped<CallbackSecurityEvaluator>();
builder.Services.AddSingleton<CallbackSignatureValidator>();
builder.Services.AddScoped<CallbackRequestBodyReader>();
builder.Services.AddScoped<CallbackIngressService>();
builder.Services.AddSingleton<OfficialApiCircuitBreaker>();
builder.Services.AddScoped<OfficialApiCatalogService>();
builder.Services.AddScoped<OfficialApiDocumentationSeedCatalog>();
builder.Services.AddScoped<OfficialApiQueryParameterStrategy>();
builder.Services.AddScoped<OfficialApiBodyParameterStrategy>();
builder.Services.AddScoped<OfficialApiContractDocumentationService>();
builder.Services.AddScoped<OfficialApiInvokerService>();
builder.Services.AddScoped<OfficialApiResultPresentationService>();
builder.Services.AddScoped<OfficialApiBinaryFileResultFactory>();
builder.Services.AddScoped<OfficialControlIdApiService>();
builder.Services.AddScoped<IOfficialControlIdApiService>(serviceProvider => serviceProvider.GetRequiredService<OfficialControlIdApiService>());
builder.Services.AddScoped<ControlIdInputSanitizer>();
builder.Services.AddSingleton<NavigationCatalogService>();
builder.Services.AddScoped<PageShellService>();
builder.Services.AddScoped<UploadedFileBase64Encoder>();
builder.Services.AddScoped<ProductSpecificConfigurationPayloadFactory>();
builder.Services.AddScoped<ProductSpecificJsonReader>();
builder.Services.AddScoped<ProductSpecificSnapshotService>();
builder.Services.AddScoped<ProductSpecificCommandService>();
builder.Services.AddScoped<OperationModesPayloadFactory>();
builder.Services.AddScoped<OperationModesProfileResolver>();
builder.Services.AddScoped<DocumentedFeaturesPayloadFactory>();
builder.Services.AddScoped<PushCommandWorkflowService>();
builder.Services.AddScoped<PushIdempotencyKeyResolver>();
builder.Services.AddScoped<PrivacySubjectReportService>();

// Repositórios de banco local
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<DeviceRepository>();
builder.Services.AddScoped<SessionRepository>();
builder.Services.AddScoped<BiometricTemplateRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<QRCodeRepository>();
builder.Services.AddScoped<GroupRepository>();
builder.Services.AddScoped<AccessLogRepository>();
builder.Services.AddScoped<ChangeLogRepository>();
builder.Services.AddScoped<AccessRuleRepository>();
builder.Services.AddScoped<ConfigRepository>();
builder.Services.AddScoped<PhotoRepository>();
builder.Services.AddScoped<LogoRepository>();
builder.Services.AddScoped<MonitorEventRepository>();
builder.Services.AddScoped<PushCommandRepository>();
builder.Services.AddScoped<LogRepository>();
builder.Services.AddScoped<SyncRepository>();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddSerilog();
});

var app = builder.Build();

ValidateRuntimeSecurity(app);

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (forwardedHeadersEnabled)
{
    app.UseForwardedHeaders();
}

// Middlewares customizados (ordem: correlacao -> tratamento de erro -> logging de request -> sessao -> session API)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// Ativa arquivos estáticos e roteamento padrão
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
        StaticAssetCachePolicy.ApplyVersionedAssetCacheHeaders(context.Context)
});
app.UseRouting();

if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Integracao Control iD PoC v1");
        options.RoutePrefix = "swagger";
    });
}

// Sessão ASP.NET Core (deve vir antes de endpoints)
app.UseSession();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseMiddleware<ApiSessionMiddleware>();

// Roda as migrações automáticas (opcional: apenas para desenvolvimento)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntegracaoControlIDContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS MonitorEvents (
            EventId TEXT NOT NULL PRIMARY KEY,
            ReceivedAt TEXT NOT NULL,
            RawJson TEXT NOT NULL,
            EventType TEXT NOT NULL,
            DeviceId TEXT NOT NULL,
            UserId TEXT NOT NULL,
            Payload TEXT NOT NULL,
            Status TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NULL
        );");
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS PushCommands (
            CommandId TEXT NOT NULL PRIMARY KEY,
            ReceivedAt TEXT NOT NULL,
            CommandType TEXT NOT NULL,
            RawJson TEXT NOT NULL,
            Status TEXT NOT NULL,
            Payload TEXT NOT NULL,
            DeviceId TEXT NOT NULL,
            UserId TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NULL
        );");

    var callbackSecurityOptions = scope.ServiceProvider.GetRequiredService<IOptions<CallbackSecurityOptions>>().Value;
    if (callbackSecurityOptions.RequireSharedKey && string.IsNullOrWhiteSpace(callbackSecurityOptions.SharedKey))
    {
        // SECURITY: callback ingress com validacao ativada e segredo vazio deve
        // ser tratado como configuracao insegura para evitar falsa sensacao de protecao.
        Log.Error("SECURITY: CallbackSecurity.RequireSharedKey esta habilitado, mas nenhum SharedKey foi configurado.");
    }
    else if (!callbackSecurityOptions.RequireSharedKey && !app.Environment.IsDevelopment())
    {
        Log.Warning("SECURITY: Callback ingress esta sem shared key fora de Development. Restrinja IPs ou habilite chave compartilhada.");
    }
}

// Mensagem de inicialização no log
Log.Information("Aplicação Integracao.ControlID.PoC inicializada em {Env}...", app.Environment.EnvironmentName);

var healthCheckResponseWriter = HealthCheckResponseWriter.WriteAsync;

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = healthCheckResponseWriter
})
.AllowAnonymous()
.DisableRateLimiting();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = healthCheckResponseWriter
})
.AllowAnonymous()
.DisableRateLimiting();

if (app.Configuration.GetValue<bool?>("Observability:Metrics:Enabled") ?? true)
{
    var metricsEndpoint = app.MapGet("/metrics", PrometheusMetricsWriter.WriteAsync)
        .DisableRateLimiting();

    if (app.Configuration.GetValue<bool>("Observability:Metrics:AllowAnonymous"))
    {
        metricsEndpoint.AllowAnonymous();
    }
    else
    {
        metricsEndpoint.RequireAuthorization("AdministratorOnly");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

static void ValidateRuntimeSecurity(WebApplication app)
{
    if (app.Environment.IsDevelopment())
        return;

    var allowedHosts = app.Configuration["AllowedHosts"];
    var configuredHosts = (allowedHosts ?? string.Empty)
        .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (configuredHosts.Length == 0 || configuredHosts.Any(static host => host == "*"))
    {
        throw new InvalidOperationException(
            "AllowedHosts must be explicitly configured for non-Development environments.");
    }

    if (configuredHosts.Any(IsPlaceholderValue))
    {
        throw new InvalidOperationException(
            "AllowedHosts must not contain placeholder values for non-Development environments.");
    }

    var callbackSecurityOptions = app.Services.GetRequiredService<IOptions<CallbackSecurityOptions>>().Value;
    if (!callbackSecurityOptions.RequireSharedKey)
    {
        throw new InvalidOperationException(
            "CallbackSecurity:RequireSharedKey must be true for non-Development environments.");
    }

    if (string.IsNullOrWhiteSpace(callbackSecurityOptions.SharedKey))
    {
        throw new InvalidOperationException(
            "CallbackSecurity:SharedKey must be configured for non-Development environments.");
    }

    if (callbackSecurityOptions.SharedKey.Trim().Length < 32 || IsPlaceholderValue(callbackSecurityOptions.SharedKey))
    {
        throw new InvalidOperationException(
            "CallbackSecurity:SharedKey must be a non-placeholder value with at least 32 characters for non-Development environments.");
    }

    if (!callbackSecurityOptions.RequireSignedRequests)
    {
        throw new InvalidOperationException(
            "CallbackSecurity:RequireSignedRequests must be true for non-Development environments.");
    }

    if (app.Configuration.GetValue<bool>("OpenApi:Enabled"))
    {
        throw new InvalidOperationException(
            "OpenApi:Enabled must be false for non-Development environments.");
    }

    if (app.Configuration.GetValue<bool>("Observability:Metrics:AllowAnonymous"))
    {
        throw new InvalidOperationException(
            "Observability:Metrics:AllowAnonymous must be false for non-Development environments.");
    }

    if (app.Configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
    {
        var knownProxies = app.Configuration
            .GetSection("ForwardedHeaders:KnownProxies")
            .Get<string[]>() ?? [];

        if (knownProxies.Length == 0 || knownProxies.Any(IsPlaceholderValue))
        {
            throw new InvalidOperationException(
                "ForwardedHeaders:KnownProxies must list trusted reverse proxy IPs when ForwardedHeaders:Enabled is true outside Development.");
        }
    }

    var egressOptions = app.Services.GetRequiredService<IOptions<ControlIdEgressOptions>>().Value;
    var allowedDeviceHosts = egressOptions.AllowedDeviceHosts
        .Where(static host => !string.IsNullOrWhiteSpace(host))
        .Select(static host => host.Trim())
        .ToArray();

    if (!egressOptions.RequireAllowedDeviceHosts ||
        allowedDeviceHosts.Length == 0 ||
        allowedDeviceHosts.Any(static host => host == "*"))
    {
        throw new InvalidOperationException(
            "ControlIDApi:RequireAllowedDeviceHosts must be true and ControlIDApi:AllowedDeviceHosts must list allowed device hosts for non-Development environments.");
    }

    if (allowedDeviceHosts.Any(IsPlaceholderValue))
    {
        throw new InvalidOperationException(
            "ControlIDApi:AllowedDeviceHosts must not contain placeholder values for non-Development environments.");
    }
}

static bool IsPlaceholderValue(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return true;

    var normalized = value.Trim();
    return normalized.Contains('<', StringComparison.Ordinal) ||
           normalized.Contains('>', StringComparison.Ordinal) ||
           normalized.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("changeme", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("replace", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("example", StringComparison.OrdinalIgnoreCase) ||
           normalized.Contains("localhost", StringComparison.OrdinalIgnoreCase);
}
