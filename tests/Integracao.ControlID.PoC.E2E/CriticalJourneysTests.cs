using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace Integracao.ControlID.PoC.E2E;

public sealed class CriticalJourneysTests
{
    [Fact(Timeout = 180_000)]
    public async Task Stub_backed_journeys_are_accessible_responsive_and_visually_stable()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var environment = await E2EEnvironment.StartAsync(cancellationToken);
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = environment.AppUrl.ToString(),
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
            Locale = "pt-BR",
            ReducedMotion = ReducedMotion.Reduce
        });
        var page = await context.NewPageAsync();
        var consoleErrors = new List<string>();
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
                consoleErrors.Add(message.Text);
        };

        await BootstrapLocalAdministratorAsync(page);
        await ConnectAndAuthenticateStubAsync(page, environment.StubUrl);

        var pages = new[]
        {
            (Path: "/", Name: "home-desktop"),
            (Path: "/OfficialApi", Name: "catalog-desktop"),
            (Path: "/OfficialObjects?endpointId=objects-load&objectName=users&page=1", Name: "objects-desktop"),
            (Path: "/Media", Name: "media-desktop"),
            (Path: "/ProductSpecific", Name: "product-desktop"),
            (Path: "/DocumentedFeatures", Name: "features-desktop"),
            (Path: "/Config/Diagnostics", Name: "diagnostics-desktop"),
            (Path: "/Development/Simulator", Name: "simulator-desktop"),
            (Path: "/Auth/Status", Name: "status-desktop")
        };

        foreach (var target in pages)
        {
            await page.GotoAsync(target.Path, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await AssertPageQualityAsync(page, target.Path);
            await AssertAccessibilityAsync(page, target.Path);
            if (target.Name is "home-desktop" or "objects-desktop" or "media-desktop" or "simulator-desktop")
                await VisualRegression.AssertAsync(page, target.Name, environment.BaselineDirectory, environment.ScreenshotDirectory);
        }

        await page.SetViewportSizeAsync(390, 844);
        foreach (var target in new[] { (Path: "/", Name: "home-mobile"), (Path: "/Media", Name: "media-mobile") })
        {
            await page.GotoAsync(target.Path, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
            await AssertPageQualityAsync(page, target.Path);
            await AssertAccessibilityAsync(page, target.Path);
            await VisualRegression.AssertAsync(page, target.Name, environment.BaselineDirectory, environment.ScreenshotDirectory);
        }

        await page.Keyboard.PressAsync("Tab");
        var focusedTag = await page.EvaluateAsync<string>("document.activeElement?.tagName || ''");
        Assert.NotEqual("BODY", focusedTag);
        Assert.Empty(consoleErrors);
    }

    private static async Task BootstrapLocalAdministratorAsync(IPage page)
    {
        const string testCredential = "E2E-Only-Credential-42!";
        await page.GotoAsync("/Auth/Register", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("input[name='Name']").FillAsync("Administrador E2E");
        await page.Locator("input[name='Username']").FillAsync("e2e-admin");
        await page.Locator("input[name='Email']").FillAsync("e2e-admin@example.invalid");
        await page.Locator("input[name='Phone']").FillAsync("5511000000000");
        await page.Locator("input[name='Password']").FillAsync(testCredential);
        await page.Locator("input[name='ConfirmPassword']").FillAsync(testCredential);
        await page.Locator("form[action*='/Auth/Register'] button[type='submit']").ClickAsync();
        await page.WaitForURLAsync("**/Auth/LocalLogin");

        await page.Locator("input[name='Username']").FillAsync("e2e-admin");
        await page.Locator("input[name='Password']").FillAsync(testCredential);
        await page.Locator("form[action*='/Auth/LocalLogin'] button[type='submit']").ClickAsync();
        await page.WaitForURLAsync(url => !new Uri(url).AbsolutePath.Contains("LocalLogin", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task ConnectAndAuthenticateStubAsync(IPage page, Uri stubUrl)
    {
        await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("select[name='Scheme']").SelectOptionAsync(stubUrl.Scheme);
        await page.Locator("input[name='Host']").FillAsync(stubUrl.Host);
        await page.Locator("input[name='Port']").FillAsync(stubUrl.Port.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await page.Locator("form[action*='ConnectToDevice'] button[type='submit']").First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

        await page.GotoAsync("/Auth/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.Locator("input[name='Username']").FillAsync("stub-admin");
        await page.Locator("input[name='Password']").FillAsync("stub-password");
        await page.Locator("form[action*='/Auth/Login'] button[type='submit']").ClickAsync();
        await page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
        await Assertions.Expect(page.GetByText("Sessão ativa", new PageGetByTextOptions { Exact = true }).First).ToBeVisibleAsync();
    }

    private static async Task AssertPageQualityAsync(IPage page, string path)
    {
        await Assertions.Expect(page.Locator("main#mainContent")).ToBeVisibleAsync();
        var quality = await page.EvaluateAsync<PageQuality>(
            """
            () => ({
              overflow: document.documentElement.scrollWidth > document.documentElement.clientWidth + 1,
              visibleText: (document.body.innerText || '').trim().length,
              mojibake: /Ã[\u0080-\u00BF]|Â[\u0080-\u00BF]|â(?:€|€™|€œ|€)|\uFFFD/.test(document.body.innerText || ''),
              mojibakeSample: ((document.body.innerText || '').match(/.{0,40}(?:Ã[\u0080-\u00BF]|Â[\u0080-\u00BF]|â(?:€|€™|€œ|€)|\uFFFD).{0,40}/) || [''])[0],
              unlabeled: [...document.querySelectorAll('input:not([type=hidden]), select, textarea')]
                .filter(field => !field.labels?.length && !field.getAttribute('aria-label') && !field.getAttribute('aria-labelledby')).length,
              missingAlt: [...document.querySelectorAll('img')].filter(image => !image.hasAttribute('alt')).length
            })
            """);

        Assert.False(quality.Overflow, $"Overflow horizontal em {path}.");
        Assert.True(quality.VisibleText > 20, $"Pagina sem conteudo visivel em {path}.");
        Assert.False(quality.Mojibake, $"Mojibake visivel em {path}: {quality.MojibakeSample}");
        Assert.Equal(0, quality.Unlabeled);
        Assert.Equal(0, quality.MissingAlt);
    }

    private static async Task AssertAccessibilityAsync(IPage page, string path)
    {
        var result = await page.RunAxe();
        var violations = result.Violations
            .Where(static violation => violation.Impact is "critical" or "serious")
            .Select(violation =>
                $"{violation.Id} ({violation.Nodes.Count()} nos): " +
                string.Join(" | ", violation.Nodes.Take(12).Select(node =>
                    $"{node.Target} :: {node.Html}")))
            .ToArray();
        Assert.True(violations.Length == 0, $"Violacoes axe em {path}: {string.Join(", ", violations)}");
    }

    private sealed class PageQuality
    {
        public bool Overflow { get; set; }
        public int VisibleText { get; set; }
        public bool Mojibake { get; set; }
        public string MojibakeSample { get; set; } = string.Empty;
        public int Unlabeled { get; set; }
        public int MissingAlt { get; set; }
    }
}
