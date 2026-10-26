using System.Runtime.InteropServices;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Models;
using SnipeIt.Automation.Pages;
using VerifyNUnit;

namespace SnipeIt.Automation.Tests.Visual;

[NonParallelizable]
[Category("Visual")]
public sealed class AssetFormVisualTests : PageTest
{
    [SetUp]
    public async Task SetUp()
    {
        await Page.SetViewportSizeAsync(1440, 1000);

        await Page.EmulateMediaAsync(
            new() { ColorScheme = ColorScheme.Light, ReducedMotion = ReducedMotion.Reduce }
        );

        await LoginPage.LoginAsync(Page);
    }

    [Test]
    public async Task ReadyToDeployCheckoutFormMatchesApprovedSnapshot()
    {
        await AssetCreatePage.OpenAsync(Page);

        await AssetCreatePage.FillAssetAsync(
            Page,
            new AssetData(
                "visual-asset-tag",
                "visual-serial",
                AssetConstants.Model,
                AssetConstants.Status,
                "Visual checkout state"
            )
        );

        var screenshot = await AssetCreatePage.CaptureStableFormAsync(Page);

        var platform =
            $"{GetOperatingSystem()}-"
            + RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

        await Verifier
            .Verify(screenshot, "png")
            .UseDirectory($"Snapshots/{platform}/{BrowserName}")
            .UseFileName("asset-create-ready-to-deploy");
    }

    private static string GetOperatingSystem()
    {
        if (OperatingSystem.IsMacOS())
            return "macos";

        if (OperatingSystem.IsWindows())
            return "windows";

        return "linux";
    }
}
