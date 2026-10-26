using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Utils;

namespace SnipeIt.Automation.Pages;

public static class AssetListPage
{
    public static ILocator AssetRow(IPage page, string tag) =>
        AssetTable(page).Locator("tbody tr").Filter(new() { Has = AssetTagLink(page, tag) });

    private static ILocator AssetTable(IPage page) => page.Locator("#assetsListingTable");

    private static ILocator SearchField(IPage page) =>
        page.Locator(".fixed-table-toolbar .search input");

    private static ILocator AssetTagLink(IPage page, string tag) =>
        page.GetByRole(AriaRole.Link, new() { Name = tag, Exact = true });

    private static ILocator AssetLink(IPage page, string tag) =>
        AssetRow(page, tag).GetByRole(AriaRole.Link, new() { Name = tag, Exact = true });

    private static ILocator LoadingIndicator(IPage page) =>
        page.Locator(".fixed-table-loading:visible");

    public static async Task SearchAsync(IPage page, string tag)
    {
        bool IsAssetResponse(IResponse response) =>
            response.Request.Method == "GET"
            && new Uri(response.Url).AbsolutePath.EndsWith(
                "/api/v1/hardware",
                StringComparison.Ordinal
            )
            && response.Ok;

        await page.RunAndWaitForResponseAsync(
            () => page.GotoAsync(TestConfig.Url(AppPaths.Assets)),
            IsAssetResponse
        );

        await AssetTable(page).WaitForAsync();

        if (await SearchField(page).InputValueAsync() != tag)
        {
            await page.RunAndWaitForResponseAsync(
                async () =>
                {
                    await SearchField(page).FillAsync(tag);
                    await SearchField(page).PressAsync("Enter");
                },
                response =>
                    IsAssetResponse(response)
                    && Uri.UnescapeDataString(new Uri(response.Url).Query)
                        .Contains($"search={tag}", StringComparison.Ordinal)
            );
        }

        await LoadingIndicator(page).WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public static async Task OpenAssetFromListAsync(IPage page, string tag)
    {
        await AssetLink(page, tag).ClickAsync();
        await page.WaitForURLAsync(new Regex(@"/hardware/\d+(?:[?#].*)?$"));
    }
}
