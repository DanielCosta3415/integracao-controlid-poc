using System;
using System.Linq;
using Integracao.ControlID.PoC.ViewModels.Shared;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Navigation
{
    public class PageShellService
    {
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionDeviceNameKey = "ControlID_DeviceName";
        private const string SessionDeviceSerialKey = "ControlID_DeviceSerial";
        private const string SessionDeviceFirmwareKey = "ControlID_DeviceFirmware";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        private readonly NavigationCatalogService _navigationCatalogService;

        public PageShellService(NavigationCatalogService navigationCatalogService)
        {
            _navigationCatalogService = navigationCatalogService;
        }

        public PageShellContextViewModel BuildShellContext(
            HttpContext? httpContext,
            string controller,
            string action,
            string title,
            string subtitle)
        {
            var currentModule = _navigationCatalogService.GetModule(controller, action);
            var currentDomain = currentModule == null
                ? _navigationCatalogService.GetDomainByController(controller)
                : _navigationCatalogService.GetDomain(currentModule.DomainId);

            return new PageShellContextViewModel
            {
                Controller = controller,
                Action = action,
                Title = title,
                Subtitle = subtitle,
                SectionLabel = currentDomain?.ShortTitle ?? Humanize(controller),
                CurrentDomain = currentDomain,
                CurrentModule = currentModule,
                ConnectionPanel = BuildConnectionPanel(httpContext, BuildReturnUrl(httpContext))
            };
        }

        public ConnectionPanelViewModel BuildConnectionPanel(HttpContext? httpContext, string? returnUrl = null)
        {
            var session = httpContext?.Session;
            var baseAddress = session?.GetString(SessionDeviceAddressKey) ?? string.Empty;
            var model = new ConnectionPanelViewModel
            {
                BaseAddress = baseAddress,
                ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl!,
                IsDeviceConnected = !string.IsNullOrWhiteSpace(baseAddress),
                IsSessionActive = !string.IsNullOrWhiteSpace(session?.GetString(SessionSessionStringKey)),
                DeviceName = session?.GetString(SessionDeviceNameKey) ?? string.Empty,
                DeviceSerial = session?.GetString(SessionDeviceSerialKey) ?? string.Empty,
                DeviceFirmware = session?.GetString(SessionDeviceFirmwareKey) ?? string.Empty
            };

            if (Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri))
            {
                model.Scheme = uri.Scheme;
                model.Host = uri.Host;
                model.Port = uri.IsDefaultPort ? null : uri.Port;
            }

            return model;
        }

        public bool TryNormalizeConnection(ConnectionPanelViewModel model, out string baseAddress, out string errorMessage)
        {
            baseAddress = string.Empty;
            errorMessage = string.Empty;

            if (!string.IsNullOrWhiteSpace(model.Host) &&
                (model.Host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                 model.Host.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                if (!Uri.TryCreate(model.Host.Trim(), UriKind.Absolute, out var fullUri))
                {
                    errorMessage = "Informe um IP, domínio ou URL válida para o equipamento.";
                    return false;
                }

                baseAddress = $"{fullUri.Scheme}://{fullUri.Host}{(fullUri.IsDefaultPort ? string.Empty : $":{fullUri.Port}")}";
                return true;
            }

            if (string.IsNullOrWhiteSpace(model.Host))
            {
                errorMessage = "Informe o IP ou domínio do equipamento Control iD.";
                return false;
            }

            var scheme = string.Equals(model.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";

            try
            {
                var builder = new UriBuilder(scheme, model.Host.Trim());
                if (model.Port.HasValue && model.Port.Value > 0)
                {
                    builder.Port = model.Port.Value;
                }

                baseAddress = builder.Uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
                return true;
            }
            catch (UriFormatException)
            {
                errorMessage = "Não foi possível montar a URL do equipamento com os dados informados.";
                return false;
            }
        }

        private static string BuildReturnUrl(HttpContext? httpContext)
        {
            if (httpContext == null)
            {
                return "/";
            }

            var path = httpContext.Request.Path.HasValue ? httpContext.Request.Path.Value : "/";
            var query = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value : string.Empty;
            return string.IsNullOrWhiteSpace(path) ? "/" : $"{path}{query}";
        }

        private static string Humanize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Concat(value.Select((character, index) =>
                index > 0 && char.IsUpper(character)
                    ? $" {character}"
                    : character.ToString()));
        }
    }
}
