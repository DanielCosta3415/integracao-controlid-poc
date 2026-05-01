using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Integracao.ControlID.PoC.Tests.TestSupport;

public sealed class StaticUrlHelper : IUrlHelper
{
    public StaticUrlHelper(ActionContext actionContext)
    {
        ActionContext = actionContext;
    }

    public ActionContext ActionContext { get; }

    public string? Action(UrlActionContext actionContext)
    {
        var controller = actionContext.Controller?.ToString();
        var action = actionContext.Action?.ToString();

        if (string.IsNullOrWhiteSpace(controller) && string.IsNullOrWhiteSpace(action))
            return "/";

        return $"/{controller ?? "Home"}/{action ?? "Index"}";
    }

    public string? Content(string? contentPath)
    {
        return contentPath;
    }

    public bool IsLocalUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) && url.StartsWith('/');
    }

    public string? Link(string? routeName, object? values)
    {
        return RouteUrl(new UrlRouteContext { RouteName = routeName, Values = values });
    }

    public string? RouteUrl(UrlRouteContext routeContext)
    {
        return routeContext.RouteName ?? "/";
    }
}
