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
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

// Configura Serilog
SeriLogConfiguration.ConfigureSerilog(builder.Host, builder.Configuration);

// Configura contexto do banco de dados SQLite
builder.Services.AddDbContext<IntegracaoControlIDContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddScoped<CallbackSecurityEvaluator>();
builder.Services.AddScoped<CallbackRequestBodyReader>();
builder.Services.AddScoped<CallbackIngressService>();
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

// Middlewares customizados (ordem: tratamento de erro → logging de request → sessão → session API)
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// Ativa arquivos estáticos e roteamento padrão
app.UseStaticFiles();
app.UseRouting();

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
