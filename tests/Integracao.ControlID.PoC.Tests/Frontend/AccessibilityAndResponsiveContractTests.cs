using System.Text.RegularExpressions;

namespace Integracao.ControlID.PoC.Tests.Frontend;

public class AccessibilityAndResponsiveContractTests
{
    [Fact]
    public void Layout_ProvidesSkipLinkMainLandmarkAndAccessibleSearchContract()
    {
        var layout = ReadRepoFile("Views", "Shared", "_Layout.cshtml");

        Assert.Contains("class=\"skip-link\"", layout);
        Assert.Contains("href=\"#mainContent\"", layout);
        Assert.Contains("<section class=\"app-page-context\" aria-label=\"Contexto da pagina\">", layout);
        Assert.Contains("<main id=\"mainContent\" class=\"app-content\" tabindex=\"-1\">", layout);
        Assert.Contains("<div class=\"visually-hidden\" hidden>", layout);
        Assert.Contains("role=\"combobox\"", layout);
        Assert.Contains("autocomplete=\"off\"", layout);
        Assert.Contains("aria-autocomplete=\"list\"", layout);
        Assert.Contains("aria-controls=\"moduleSearchResults\"", layout);
        Assert.Contains("aria-describedby=\"moduleSearchHint moduleSearchStatus\"", layout);
        Assert.Contains("id=\"moduleSearchStatus\"", layout);
        Assert.Contains("role=\"status\"", layout);
        Assert.Contains("aria-live=\"polite\"", layout);
        Assert.Contains("role=\"listbox\"", layout);
    }

    [Fact]
    public void ShellScript_ImplementsKeyboardSearchAndAnnouncesResultState()
    {
        var script = ReadRepoFile("wwwroot", "js", "site.js");

        Assert.Contains("const searchStatus = document.getElementById(\"moduleSearchStatus\");", script);
        Assert.Contains("announceSearchStatus", script);
        Assert.Contains("aria-activedescendant", script);
        Assert.Contains("aria-selected", script);
        Assert.Contains("ArrowDown", script);
        Assert.Contains("ArrowUp", script);
        Assert.Contains("event.key === \"Home\" || event.key === \"End\"", script);
        Assert.Contains("scrollIntoView({ block: \"nearest\" })", script);
        Assert.Contains("renderSearchResults(searchInput.value, true);", script);
        Assert.Contains("Busca fechada.", script);
    }

    [Fact]
    public void ShellScript_DoesNotGenerateRedundantAriaLabelsForNativeLabelsOrVisibleText()
    {
        var script = ReadRepoFile("wwwroot", "js", "site.js");

        Assert.Contains("if (\"labels\" in element && element.labels?.length)", script);
        Assert.Contains("return;", script);
        Assert.Contains("if (normalizeUiText(element.textContent || \"\"))", script);
        Assert.Contains("let label = normalizeUiText(element.getAttribute(\"title\") || element.getAttribute(\"placeholder\") || \"\");", script);
    }

    [Fact]
    public void Alerts_UseAppropriateLiveRegionsForInfoAndFallbackStates()
    {
        var statusMessage = ReadRepoFile("Views", "Shared", "_StatusMessage.cshtml");
        var script = ReadRepoFile("wwwroot", "js", "site.js");

        Assert.Contains("role=\"status\"", statusMessage);
        Assert.Contains("aria-live=\"polite\"", statusMessage);
        Assert.Contains("applyAlertAccessibilityFallbacks", script);
        Assert.Contains("alertElement.classList.contains(\"alert-danger\") || alertElement.classList.contains(\"alert-warning\")", script);
        Assert.Contains("isUrgent ? \"alert\" : \"status\"", script);
        Assert.Contains("isUrgent ? \"assertive\" : \"polite\"", script);
    }

    [Fact]
    public void ValidationScripts_LoadJQueryBeforeUnobtrusiveValidation()
    {
        var partial = ReadRepoFile("Views", "Shared", "_ValidationScriptsPartial.cshtml");

        var jqueryIndex = partial.IndexOf("~/lib/jquery/dist/jquery.min.js", StringComparison.Ordinal);
        var validationIndex = partial.IndexOf("~/lib/jquery-validation/dist/jquery.validate.min.js", StringComparison.Ordinal);
        var unobtrusiveIndex = partial.IndexOf("~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js", StringComparison.Ordinal);

        Assert.True(jqueryIndex >= 0, "jQuery must be loaded explicitly for client-side validation.");
        Assert.True(validationIndex > jqueryIndex, "jquery.validate must load after jQuery.");
        Assert.True(unobtrusiveIndex > validationIndex, "jquery.validate.unobtrusive must load after jquery.validate.");
    }

    [Fact]
    public void Css_ProvidesResponsiveBreakpointsAndTouchSafeNavigationContracts()
    {
        var css = ReadRepoFile("wwwroot", "css", "site.css");

        Assert.Contains("@media (max-width: 1199px)", css);
        Assert.Contains("@media (max-width: 991px)", css);
        Assert.Contains("@media (max-width: 767px)", css);
        Assert.Contains(".skip-link", css);
        Assert.Contains(".skip-link:focus", css);
        Assert.Contains("min-height: 2.75rem;", css);
        Assert.Contains("overflow-x: auto;", css);
        Assert.Contains(".app-nav-home,", css);
        Assert.Contains(".app-nav-domain__summary", css);
    }

    [Fact]
    public void Css_KeepsLiteralColorsCentralizedInRootTokens()
    {
        var css = ReadRepoFile("wwwroot", "css", "site.css");
        var cssOutsideRoot = RemoveRootBlock(css);
        var rawColorMatches = Regex.Matches(cssOutsideRoot, @"#[0-9a-fA-F]{3,8}|rgba\(", RegexOptions.CultureInvariant);

        Assert.Empty(rawColorMatches);
    }

    [Fact]
    public void DecorativeVisuals_AreHiddenFromAssistiveTechnology()
    {
        var topNavigation = ReadRepoFile("Views", "Shared", "_TopNavigation.cshtml");
        var home = ReadRepoFile("Views", "Home", "Index.cshtml");

        Assert.Contains("class=\"app-nav-module__icon\" aria-hidden=\"true\"", topNavigation);
        Assert.Contains("class=\"domain-card__icon\" aria-hidden=\"true\"", home);
        Assert.Contains("class=\"activity-item__pill activity-item__pill--@activity.Tone\" aria-hidden=\"true\"", home);
    }

    private static string RemoveRootBlock(string css)
    {
        var rootStart = css.IndexOf(":root", StringComparison.Ordinal);
        Assert.True(rootStart >= 0, "CSS must define a :root token block.");

        var braceStart = css.IndexOf('{', rootStart);
        Assert.True(braceStart >= 0, "CSS :root block must open with a brace.");

        var depth = 0;
        for (var index = braceStart; index < css.Length; index++)
        {
            if (css[index] == '{')
            {
                depth++;
            }
            else if (css[index] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return css.Remove(rootStart, index - rootStart + 1);
                }
            }
        }

        throw new InvalidOperationException("CSS :root block was not closed.");
    }

    private static string ReadRepoFile(params string[] segments)
    {
        return File.ReadAllText(Path.Combine(FindRepositoryRoot(), Path.Combine(segments)));
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Integracao.ControlID.PoC.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found from the test output directory.");
    }
}
