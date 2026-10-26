using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SnipeIt.Automation.Models;
using SnipeIt.Automation.Pages;

namespace SnipeIt.Automation.Utils;

public static class AssetCleanup
{
    public static async Task DeleteOwnedAssetAsync(IPage page, AssetData asset)
    {
        AssetDataFactory.RequireOwnedTag(asset.Tag);

        await AssetListPage.SearchAsync(page, asset.Tag);
        var row = AssetListPage.AssetRow(page, asset.Tag);
        var rowCount = await row.CountAsync();

        if (rowCount == 0)
            return;

        if (rowCount != 1)
            throw new InvalidOperationException(
                $"Cleanup found multiple rows for {asset.Tag}; nothing was deleted."
            );

        await AssetListPage.OpenAssetFromListAsync(page, asset.Tag);

        if (!await AssetDetailsPage.Serial(page, asset.Serial).IsVisibleAsync())
            throw new InvalidOperationException(
                $"Cleanup asset serial did not match {asset.Serial}; nothing was deleted."
            );

        var assetPath = new Uri(page.Url).AbsolutePath;
        if (!Regex.IsMatch(assetPath, @"^/hardware/\d+$"))
            throw new InvalidOperationException(
                "Cleanup did not reach a single asset detail page."
            );

        await AssetDetailsPage.DeleteAsync(page, asset.Tag);

        await AssetListPage.SearchAsync(page, asset.Tag);
        if (await AssetListPage.AssetRow(page, asset.Tag).CountAsync() != 0)
            throw new InvalidOperationException(
                $"Cleanup could not confirm deletion of {asset.Tag}."
            );
    }
}
