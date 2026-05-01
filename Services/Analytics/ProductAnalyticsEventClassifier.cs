namespace Integracao.ControlID.PoC.Services.Analytics;

public static class ProductAnalyticsEventClassifier
{
    public static bool TryClassify(string method, string? path, out ProductAnalyticsEvent productEvent)
    {
        productEvent = default;

        var action = ResolveAction(method);
        var segments = NormalizeSegments(path);
        if (segments.Length == 0)
        {
            productEvent = new ProductAnalyticsEvent("dashboard_viewed", "activation", "view");
            return true;
        }

        var controller = segments[0];
        var operation = segments.Length > 1 ? segments[1] : "index";

        productEvent = controller switch
        {
            "home" => new ProductAnalyticsEvent("dashboard_viewed", "activation", action),
            "workspace" => new ProductAnalyticsEvent("workspace_explored", "activation", action),
            "auth" => ClassifyAuth(operation, action),
            "session" => new ProductAnalyticsEvent("device_session_managed", "device_session", action),
            "devices" => new ProductAnalyticsEvent("device_registry_managed", "device_setup", action),
            "officialapi" => operation == "invoke"
                ? new ProductAnalyticsEvent("official_endpoint_invoked", "official_api", action)
                : new ProductAnalyticsEvent("official_catalog_explored", "official_api", action),
            "officialobjects" => new ProductAnalyticsEvent("official_objects_managed", "official_objects", action),
            "operationmodes" => new ProductAnalyticsEvent("operation_modes_managed", "operation_modes", action),
            "productspecific" => new ProductAnalyticsEvent("product_specific_flow_used", "product_specific", action),
            "advancedofficial" => new ProductAnalyticsEvent("advanced_official_flow_used", "advanced_official", action),
            "hardware" => new ProductAnalyticsEvent("hardware_flow_used", "hardware", action),
            "system" => new ProductAnalyticsEvent("system_flow_used", "system", action),
            "users" or "groups" or "accessrules" or "cards" or "qrcodes" or "biometrictemplates" or "media" or "logo"
                => new ProductAnalyticsEvent("identity_credential_flow_used", "identity_credentials", action),
            "officialevents" or "monitor" or "monitorwebhook" or "api"
                => new ProductAnalyticsEvent("event_monitoring_used", "callbacks_monitoring", action),
            "pushcenter" or "push" or "result"
                => new ProductAnalyticsEvent("push_flow_used", "push", action),
            "privacy" => new ProductAnalyticsEvent("privacy_report_used", "privacy_governance", action),
            "accesslogs" or "changelogs" or "errors"
                => new ProductAnalyticsEvent("audit_history_used", "audit_history", action),
            "documentedfeatures" => new ProductAnalyticsEvent("documentation_explored", "documentation", action),
            _ => default
        };

        return !string.IsNullOrWhiteSpace(productEvent.Name);
    }

    private static ProductAnalyticsEvent ClassifyAuth(string operation, string action)
    {
        return operation switch
        {
            "locallogin" => new ProductAnalyticsEvent(action == "submit" ? "local_login_submitted" : "local_login_viewed", "activation", action),
            "register" => new ProductAnalyticsEvent(action == "submit" ? "local_registration_submitted" : "local_registration_viewed", "activation", action),
            "login" => new ProductAnalyticsEvent(action == "submit" ? "device_login_submitted" : "device_login_viewed", "device_session", action),
            "logout" or "locallogout" => new ProductAnalyticsEvent("logout_requested", "device_session", action),
            "status" => new ProductAnalyticsEvent("auth_status_viewed", "device_session", action),
            "changepassword" => new ProductAnalyticsEvent("credential_change_requested", "security", action),
            _ => new ProductAnalyticsEvent("auth_flow_used", "activation", action)
        };
    }

    private static string ResolveAction(string method)
    {
        return string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase)
            ? "view"
            : "submit";
    }

    private static string[] NormalizeSegments(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "/")
            return [];

        var pathOnly = path.Trim();
        var queryStart = pathOnly.IndexOfAny(['?', '#']);
        if (queryStart >= 0)
            pathOnly = pathOnly[..queryStart];

        return pathOnly
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static segment => segment.ToLowerInvariant())
            .ToArray();
    }
}

public readonly record struct ProductAnalyticsEvent(string Name, string Flow, string Action);
