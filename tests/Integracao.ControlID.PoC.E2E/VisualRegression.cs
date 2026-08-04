using Microsoft.Playwright;

namespace Integracao.ControlID.PoC.E2E;

internal static class VisualRegression
{
    public static async Task AssertAsync(
        IPage page,
        string name,
        string baselineDirectory,
        string screenshotDirectory)
    {
        Directory.CreateDirectory(baselineDirectory);
        Directory.CreateDirectory(screenshotDirectory);
        var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = true,
            Animations = ScreenshotAnimations.Disabled
        });
        var artifactPath = Path.Combine(screenshotDirectory, name + ".png");
        await File.WriteAllBytesAsync(artifactPath, screenshot, TestContext.Current.CancellationToken);

        var baselinePath = Path.Combine(baselineDirectory, name + ".png");
        var update = string.Equals(Environment.GetEnvironmentVariable("UPDATE_VISUAL_BASELINES"), "1", StringComparison.Ordinal);
        if (update || !File.Exists(baselinePath))
        {
            await File.WriteAllBytesAsync(baselinePath, screenshot, TestContext.Current.CancellationToken);
            return;
        }

        var baseline = await File.ReadAllBytesAsync(baselinePath, TestContext.Current.CancellationToken);
        var difference = await page.EvaluateAsync<double>(
            """
            async ({ actual, expected }) => {
              const load = source => new Promise((resolve, reject) => {
                const image = new Image();
                image.onload = () => resolve(image);
                image.onerror = reject;
                image.src = `data:image/png;base64,${source}`;
              });
              const [left, right] = await Promise.all([load(actual), load(expected)]);
              if (left.width !== right.width || left.height !== right.height) return 1;
              const canvas = document.createElement('canvas');
              canvas.width = left.width;
              canvas.height = left.height;
              const context = canvas.getContext('2d', { willReadFrequently: true });
              context.drawImage(left, 0, 0);
              const a = context.getImageData(0, 0, canvas.width, canvas.height).data;
              context.clearRect(0, 0, canvas.width, canvas.height);
              context.drawImage(right, 0, 0);
              const b = context.getImageData(0, 0, canvas.width, canvas.height).data;
              let changed = 0;
              for (let index = 0; index < a.length; index += 4) {
                const delta = Math.abs(a[index] - b[index]) + Math.abs(a[index + 1] - b[index + 1]) + Math.abs(a[index + 2] - b[index + 2]);
                if (delta > 72) changed++;
              }
              return changed / (a.length / 4);
            }
            """,
            new { actual = Convert.ToBase64String(screenshot), expected = Convert.ToBase64String(baseline) });

        Assert.True(difference <= 0.03, $"Regressao visual em {name}: {difference:P2} dos pixels excederam a tolerancia de 3%. Artefato: {artifactPath}");
    }
}
