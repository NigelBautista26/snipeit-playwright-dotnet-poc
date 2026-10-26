using System.Text.Json;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Pages;
using SnipeIt.Automation.Utils;

namespace SnipeIt.Automation.Tests.Api;

[NonParallelizable]
[Category("Api")]
public sealed class AssetApiTests : PageTest
{
    private Dictionary<string, string> _headers = [];

    [SetUp]
    public async Task Authenticate()
    {
        await LoginPage.LoginAsync(Page);

        var csrf = await AppShellPage.GetCsrfTokenAsync(Page);

        _headers = new()
        {
            ["Accept"] = "application/json",
            ["Content-Type"] = "application/json",
            ["User-Agent"] = "SnipeIt-Playwright-POC",
            ["X-Requested-With"] = "XMLHttpRequest",
            ["X-CSRF-TOKEN"] = csrf,
        };
    }

    [Test]
    public async Task ReadyToDeployStatusSupportsCheckout()
    {
        await using var response = await Context.APIRequest.GetAsync(
            TestConfig.Url(
                $"/api/v1/statuslabels?name={Uri.EscapeDataString(AssetConstants.Status)}&limit=100"
            ),
            new() { Headers = _headers }
        );

        Assert.That(response.Status, Is.EqualTo(200));

        using var body = JsonDocument.Parse(await response.TextAsync());

        var rows = body
            .RootElement.GetProperty("rows")
            .EnumerateArray()
            .Where(row => row.GetProperty("name").GetString() == AssetConstants.Status)
            .ToArray();

        Assert.That(rows, Has.Length.EqualTo(1));

        Assert.That(rows[0].GetProperty("type").GetString(), Is.EqualTo("deployable"));

        var statusId = rows[0].GetProperty("id").GetInt32();

        await using var eligibility = await Context.APIRequest.GetAsync(
            TestConfig.Url($"/api/v1/statuslabels/{statusId}/deployable"),
            new() { Headers = _headers }
        );

        Assert.That(eligibility.Status, Is.EqualTo(200));

        Assert.That((await eligibility.TextAsync()).Trim(), Is.EqualTo("1"));
    }

    [Test]
    public async Task NewGeneratedAssetTagDoesNotAlreadyExist()
    {
        var tag = AssetDataFactory.Create().Tag;

        await using var response = await Context.APIRequest.GetAsync(
            TestConfig.Url($"/api/v1/hardware?search={Uri.EscapeDataString(tag)}"),
            new() { Headers = _headers }
        );

        Assert.That(response.Status, Is.EqualTo(200));

        using var body = JsonDocument.Parse(await response.TextAsync());

        Assert.That(body.RootElement.GetProperty("total").GetInt32(), Is.Zero);

        Assert.That(body.RootElement.GetProperty("rows").GetArrayLength(), Is.Zero);
    }
}
