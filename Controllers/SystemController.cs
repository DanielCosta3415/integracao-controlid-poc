using System;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Files;
using Integracao.ControlID.PoC.ViewModels.System;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class SystemController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly UploadedFileBase64Encoder _fileEncoder;
        private readonly ILogger<SystemController> _logger;

        public SystemController(
            OfficialControlIdApiService apiService,
            UploadedFileBase64Encoder fileEncoder,
            ILogger<SystemController> logger)
        {
            _apiService = apiService;
            _fileEncoder = fileEncoder;
            _logger = logger;
        }

        // GET: /System/Info
        public async Task<IActionResult> Info()
        {
            var model = new SystemInfoViewModel();
            string deviceAddress = _apiService.GetDeviceAddress();

            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                model.ErrorMessage = "É necessário conectar-se com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("system-information");
                if (!result.Success)
                {
                    model.ErrorMessage = BuildErrorMessage(result, "Erro ao consultar informações do sistema");
                    _logger.LogWarning("Erro ao consultar informações do sistema oficial: {StatusCode}", result.StatusCode);
                    return View(model);
                }

                if (document == null)
                {
                    model.ErrorMessage = "Resposta inesperada da API ao consultar informações do sistema.";
                    return View(model);
                }

                var root = document.RootElement;
                var sysInfo = new SystemInfoDto
                {
                    Serial = GetString(root, "serial"),
                    Version = GetString(root, "version"),
                    Model = GetString(root, "model", "product"),
                    Hostname = GetString(root, "hostname"),
                    Firmware = GetString(root, "version", "firmware"),
                    Uptime = FormatUptime(root),
                    CurrentTime = FormatUnixTime(root, "time"),
                    IpAddress = GetNestedString(root, "network", "ip"),
                    Gateway = GetNestedString(root, "network", "gateway"),
                    MacAddress = GetNestedString(root, "network", "mac"),
                    DeviceId = GetString(root, "device_id"),
                    OnlineMode = FormatOnlineMode(root),
                    RawJson = result.ResponseBody,
                    RetrievedAt = DateTime.UtcNow
                };

                model.SystemInfo = sysInfo;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar informações do sistema", ex);
                _logger.LogError(ex, "Erro ao consultar informações do sistema.");
            }

            return View(model);
        }

        // POST: /System/Reset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reset()
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Info));
            }

            try
            {
                var result = await _apiService.InvokeAsync("reboot");
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao reiniciar dispositivo"));

                TempData["StatusMessage"] = "Equipamento reiniciado com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao reiniciar dispositivo", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao reiniciar dispositivo.");
            }

            return RedirectToAction(nameof(Info));
        }

        // POST: /System/SetDateTime
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDateTime()
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Info));
            }

            try
            {
                var now = DateTime.Now;
                var result = await _apiService.InvokeAsync("set-system-time", new
                {
                    day = now.Day,
                    month = now.Month,
                    year = now.Year,
                    hour = now.Hour,
                    minute = now.Minute,
                    second = now.Second
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao sincronizar data e hora"));

                TempData["StatusMessage"] = "Data e hora sincronizadas com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao sincronizar data/hora", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao sincronizar data/hora.");
            }

            return RedirectToAction(nameof(Info));
        }

        // GET: /System/HashPassword
        public IActionResult HashPassword()
        {
            return View(new HashPasswordViewModel());
        }

        // GET: /System/LoginCredentials
        public IActionResult LoginCredentials()
        {
            return View(new SystemLoginCredentialsViewModel());
        }

        // POST: /System/HashPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HashPassword(HashPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string deviceAddress = _apiService.GetDeviceAddress();

            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("hash-password", new { password = model.Password });
                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage(result, "Erro ao gerar hash da senha"));
                    return View(model);
                }

                var hashResponse = document == null
                    ? null
                    : JsonSerializer.Deserialize<HashPasswordResponse>(document.RootElement.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (hashResponse != null)
                {
                    model.Hash = hashResponse.Password;
                    model.Salt = hashResponse.Salt;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Resposta inesperada da API ao gerar hash.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("Erro ao gerar hash", ex));
                _logger.LogError(ex, "Erro ao gerar hash de senha.");
            }

            return View(model);
        }

        // POST: /System/LoginCredentials
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginCredentials(SystemLoginCredentialsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("change-login", new
                {
                    login = model.Login,
                    password = model.Password
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao alterar credenciais de login"));

                model.ResultMessage = "Credenciais de login do equipamento alteradas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao alterar credenciais de login do equipamento.");
            }

            return View(model);
        }

        // GET: /System/Network
        public async Task<IActionResult> Network()
        {
            var model = new SystemNetworkViewModel();

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("system-information");
                if (result.Success && document != null)
                {
                    var root = document.RootElement;
                    model.IpAddress = GetNestedString(root, "network", "ip");
                    model.Netmask = GetNestedString(root, "network", "netmask");
                    model.Gateway = GetNestedString(root, "network", "gateway");
                    model.DeviceHostname = GetString(root, "hostname");
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao carregar dados de rede", ex);
                _logger.LogError(ex, "Erro ao carregar página de rede.");
            }

            return View(model);
        }

        // POST: /System/Network
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Network(SystemNetworkViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var payload = new Dictionary<string, object?>
                {
                    ["ip"] = model.IpAddress,
                    ["netmask"] = model.Netmask,
                    ["gateway"] = model.Gateway,
                    ["custom_hostname_enabled"] = model.CustomHostnameEnabled,
                    ["web_server_port"] = model.WebServerPort,
                    ["ssl_enabled"] = model.SslEnabled,
                    ["self_signed_certificate"] = model.SelfSignedCertificate,
                    ["dns_primary"] = model.DnsPrimary,
                    ["dns_secondary"] = model.DnsSecondary
                };

                if (model.CustomHostnameEnabled && !string.IsNullOrWhiteSpace(model.DeviceHostname))
                    payload["device_hostname"] = model.DeviceHostname;

                var result = await _apiService.InvokeAsync("set-system-network", payload);
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao atualizar configurações de rede"));

                model.ResultMessage = "Configurações de rede aplicadas com sucesso. Se o IP ou a porta foram alterados, reconecte a PoC usando o novo endereço.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao atualizar configurações de rede.");
            }

            return View(model);
        }

        // GET: /System/Vpn
        public async Task<IActionResult> Vpn()
        {
            var model = new SystemVpnViewModel();

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            await PopulateVpnStateAsync(model);
            return View(model);
        }

        // POST: /System/UploadSslCertificate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSslCertificate(SystemNetworkViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Network", model);
            }

            if (model.CertificateFile == null || model.CertificateFile.Length == 0)
            {
                model.ResultMessage = "Selecione um arquivo PEM válido para envio.";
                model.ResultStatusType = "danger";
                return View("Network", model);
            }

            try
            {
                var base64Certificate = await _fileEncoder.EncodeAsync(model.CertificateFile, "Selecione um certificado SSL para enviar.");
                var result = await _apiService.InvokeAsync("ssl-certificate-change", base64Certificate);
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao enviar certificado SSL"));

                model.ResultMessage = "Certificado SSL enviado com sucesso.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao enviar certificado SSL.");
            }

            return View("Network", model);
        }

        // POST: /System/Vpn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vpn(SystemVpnViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var payload = new Dictionary<string, object?>
                {
                    ["enabled"] = model.Enabled,
                    ["login_enabled"] = model.LoginEnabled
                };

                if (model.LoginEnabled)
                {
                    payload["login"] = model.Login;
                    payload["password"] = model.Password;
                }

                var result = await _apiService.InvokeAsync("set-vpn-information", payload);
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao atualizar configurações OpenVPN"));

                model.ResultMessage = "Configurações de OpenVPN atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao atualizar configurações OpenVPN.");
            }

            await PopulateVpnStateAsync(model);
            return View(model);
        }

        // POST: /System/UploadVpnFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadVpnFile(SystemVpnViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Vpn", model);
            }

            if (model.VpnFile == null || model.VpnFile.Length == 0)
            {
                model.ResultMessage = "Selecione um arquivo .conf ou .zip válido.";
                model.ResultStatusType = "danger";
                await PopulateVpnStateAsync(model);
                return View("Vpn", model);
            }

            try
            {
                var fileName = model.VpnFile.FileName ?? string.Empty;
                var fileType = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? "zip" : "config";
                var base64VpnFile = await _fileEncoder.EncodeAsync(model.VpnFile, "Selecione um arquivo OpenVPN para enviar.");

                var result = await _apiService.InvokeAsync(
                    "set-vpn-file",
                    base64VpnFile,
                    $"file_type={fileType}");

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao enviar arquivo OpenVPN"));

                model.ResultMessage = $"Arquivo OpenVPN enviado com sucesso como tipo `{fileType}`.";
                model.ResultStatusType = "success";
                model.LastResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao enviar arquivo OpenVPN.");
            }

            await PopulateVpnStateAsync(model);
            return View("Vpn", model);
        }

        // GET: /System/DownloadVpnFile
        public async Task<IActionResult> DownloadVpnFile()
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Vpn));
            }

            try
            {
                var result = await _apiService.InvokeAsync("get-vpn-file");
                if (!result.Success || !result.ResponseBodyIsBase64 || string.IsNullOrWhiteSpace(result.ResponseBody))
                {
                    TempData["StatusMessage"] = BuildErrorMessage(result, "Erro ao baixar arquivo exemplo do OpenVPN");
                    TempData["StatusType"] = "danger";
                    return RedirectToAction(nameof(Vpn));
                }

                var bytes = Convert.FromBase64String(result.ResponseBody);
                var contentType = string.IsNullOrWhiteSpace(result.ResponseContentType) ? "application/zip" : result.ResponseContentType;
                var extension = contentType.Contains("zip", StringComparison.OrdinalIgnoreCase) ? "zip" : "bin";
                return File(bytes, contentType, $"openvpn_example.{extension}");
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao baixar arquivo exemplo do OpenVPN", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao baixar arquivo exemplo do OpenVPN.");
                return RedirectToAction(nameof(Vpn));
            }
        }

        // POST: /System/FactoryReset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FactoryReset(bool keepNetworkInfo = true)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Info));
            }

            try
            {
                var result = await _apiService.InvokeAsync("reset-to-factory", new
                {
                    keep_network_info = keepNetworkInfo
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao restaurar configurações de fábrica"));

                TempData["StatusMessage"] = keepNetworkInfo
                    ? "Reset de fábrica iniciado preservando as informações de rede."
                    : "Reset de fábrica iniciado sem preservar as informações de rede.";
                TempData["StatusType"] = "warning";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao iniciar reset de fábrica", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao iniciar reset de fábrica.");
            }

            return RedirectToAction(nameof(Info));
        }

        // POST: /System/RebootRecovery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RebootRecovery()
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Info));
            }

            try
            {
                var result = await _apiService.InvokeAsync("reboot-recovery");
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao reiniciar em modo de update"));

                TempData["StatusMessage"] = "Equipamento reiniciando em modo de update.";
                TempData["StatusType"] = "warning";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao reiniciar em modo de update", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao reiniciar em modo de update.");
            }

            return RedirectToAction(nameof(Info));
        }

        // POST: /System/DeleteAdmins
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdmins()
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Info));
            }

            try
            {
                var result = await _apiService.InvokeAsync("delete-admins");
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao remover administradores"));

                TempData["StatusMessage"] = "Administradores removidos com sucesso.";
                TempData["StatusType"] = "warning";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao remover administradores", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao remover administradores.");
            }

            return RedirectToAction(nameof(Info));
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string prefix)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var value))
                    return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
            }

            return string.Empty;
        }

        private static string GetNestedString(JsonElement element, string objectName, string propertyName)
        {
            if (element.TryGetProperty(objectName, out var nestedElement) && nestedElement.ValueKind == JsonValueKind.Object)
                return GetString(nestedElement, propertyName);

            return string.Empty;
        }

        private static string FormatUptime(JsonElement element)
        {
            if (!element.TryGetProperty("uptime", out var uptime) || uptime.ValueKind != JsonValueKind.Object)
                return string.Empty;

            var days = uptime.TryGetProperty("days", out var daysValue) ? daysValue.GetInt32() : 0;
            var hours = uptime.TryGetProperty("hours", out var hoursValue) ? hoursValue.GetInt32() : 0;
            var minutes = uptime.TryGetProperty("minutes", out var minutesValue) ? minutesValue.GetInt32() : 0;
            var seconds = uptime.TryGetProperty("seconds", out var secondsValue) ? secondsValue.GetInt32() : 0;

            return $"{days}d {hours:D2}h {minutes:D2}m {seconds:D2}s";
        }

        private static string FormatUnixTime(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
                return string.Empty;

            return DateTimeOffset.FromUnixTimeSeconds(value.GetInt64()).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss");
        }

        private static string FormatOnlineMode(JsonElement element)
        {
            if (!element.TryGetProperty("online", out var onlineValue))
                return string.Empty;

            return onlineValue.ValueKind == JsonValueKind.True ? "Online" :
                   onlineValue.ValueKind == JsonValueKind.False ? "Standalone" :
                   onlineValue.ToString();
        }

        private async Task PopulateVpnStateAsync(SystemVpnViewModel model)
        {
            try
            {
                var (infoResult, infoDocument) = await _apiService.InvokeJsonAsync("get-vpn-information");
                if (infoResult.Success)
                {
                    model.InformationJson = FormatJson(infoResult.ResponseBody, infoDocument);

                    if (infoDocument != null)
                    {
                        var root = infoDocument.RootElement;
                        model.Enabled = GetBoolean(root, "enabled");
                        model.LoginEnabled = GetBoolean(root, "login_enabled");
                        model.Login = GetString(root, "login");
                    }
                }

                var (statusResult, statusDocument) = await _apiService.InvokeJsonAsync("get-vpn-status");
                if (statusResult.Success)
                {
                    model.StatusJson = FormatJson(statusResult.ResponseBody, statusDocument);

                    if (statusDocument != null)
                    {
                        var statusCode = GetNullableInt(statusDocument.RootElement, "status");
                        model.StatusCode = statusCode;
                        model.StatusDescription = DescribeVpnStatus(statusCode);
                    }
                }

                var (fileResult, fileDocument) = await _apiService.InvokeJsonAsync("has-vpn-file");
                if (fileResult.Success)
                {
                    model.FileStatusJson = FormatJson(fileResult.ResponseBody, fileDocument);

                    if (fileDocument != null)
                    {
                        model.HasFile = fileDocument.RootElement.TryGetProperty("has_file", out var hasFileElement)
                            ? hasFileElement.ValueKind switch
                            {
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                JsonValueKind.Number => hasFileElement.GetInt32() == 1,
                                _ => null
                            }
                            : null;
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar estado da OpenVPN", ex);
                _logger.LogWarning(ex, "Erro ao consultar dados da OpenVPN.");
            }
        }

        private static bool GetBoolean(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return false;

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.GetInt32() == 1,
                JsonValueKind.String => value.GetString() == "1" ||
                                        value.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true,
                _ => false
            };
        }

        private static int? GetNullableInt(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value))
                return null;

            return value.ValueKind switch
            {
                JsonValueKind.Number => value.GetInt32(),
                JsonValueKind.String when int.TryParse(value.GetString(), out var parsed) => parsed,
                _ => null
            };
        }

        private static string DescribeVpnStatus(int? statusCode)
        {
            return statusCode switch
            {
                0 => "Conectado",
                1 => "Falha de autenticação",
                2 => "CA ausente",
                3 => "Falha ao validar CA",
                4 => "Certificado ou chave ausente",
                5 => "Falha no certificado público",
                6 => "Falha na chave privada",
                7 => "Falha TLS",
                8 => "Desconectado",
                9 => "Tentando conectar",
                10 => "Sem conexão de rede",
                _ => "Status não identificado"
            };
        }

        private static string FormatJson(string rawJson, JsonDocument? document)
        {
            if (document == null)
                return rawJson;

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }

}




