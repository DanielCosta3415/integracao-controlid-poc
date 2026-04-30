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
using Integracao.ControlID.PoC.Services.ProductSpecific;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configura Serilog
SeriLogConfiguration.ConfigureSerilog(builder.Host, builder.Configuration);

// Configura contexto do banco de dados SQLite
builder.Services.AddDbContext<IntegracaoControlIDContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services MVC
builder.Services.AddControllersWithViews();
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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
});

// Sessão ASP.NET Core (30 min timeout, seguro)
builder.Services.AddSession(options =>
{
    options.Cookie.Name = builder.Configuration["Session:CookieName"] ?? ".IntegracaoControlID.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
        : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(int.Parse(builder.Configuration["Session:IdleTimeout"] ?? "30"));
});

// Registro da camada oficial Control iD (injeção de dependência)
builder.Services.AddHttpClient(); // HttpClientFactory
builder.Services.Configure<CallbackSecurityOptions>(builder.Configuration.GetSection("CallbackSecurity"));
builder.Services.Configure<ControlIdCircuitBreakerOptions>(builder.Configuration.GetSection("ControlIDApi:CircuitBreaker"));
builder.Services.AddScoped<CallbackSecurityEvaluator>();
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

// Middlewares customizados (ordem: tratamento de erro → logging de request → sessão → session API)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// Ativa arquivos estáticos e roteamento padrão
app.UseResponseCompression();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("OpenApi:Enabled"))
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
}
