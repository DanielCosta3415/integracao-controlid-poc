using System;
using System.Linq;
using Integracao.ControlID.PoC.Services.Security;
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
        private readonly ControlIdInputSanitizer _inputSanitizer;

        public PageShellService(NavigationCatalogService navigationCatalogService, ControlIdInputSanitizer inputSanitizer)
        {
            _navigationCatalogService = navigationCatalogService;
            _inputSanitizer = inputSanitizer;
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
            return _inputSanitizer.TryNormalizeBaseAddress(model.Host, model.Scheme, model.Port, out baseAddress, out errorMessage);
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
