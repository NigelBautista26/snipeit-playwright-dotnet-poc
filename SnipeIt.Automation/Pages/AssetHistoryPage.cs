using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace SnipeIt.Automation.Pages;

public static class AssetHistoryPage
{
    public static ILocator CreationRow(IPage page) =>
        HistoryRows(page).Filter(new() { Has = CreateActionCell(page) });

    public static ILocator CheckoutRow(IPage page) =>
        HistoryRows(page).Filter(new() { Has = CheckoutActionCell(page) });

    public static async Task<ILocator> CheckoutTargetUserAsync(IPage page, string userId) =>
        TargetUserLink(await CellAsync(page, CheckoutRow(page), "target"), userId);

    private static ILocator HistoryTable(IPage page) => page.Locator("#history table[id]");

    private static ILocator HistoryRows(IPage page) => HistoryTable(page).GetByRole(AriaRole.Row);

    private static ILocator CreateActionCell(IPage page) =>
        page.GetByRole(
            AriaRole.Cell,
            new() { NameRegex = new Regex("^create new$", RegexOptions.IgnoreCase) }
        );

    private static ILocator CheckoutActionCell(IPage page) =>
        page.GetByRole(
            AriaRole.Cell,
            new() { NameRegex = new Regex("^check ?out$", RegexOptions.IgnoreCase) }
        );

    private static ILocator HeaderCells(IPage page) => HistoryTable(page).Locator("thead th");

    private static ILocator RowCells(ILocator row) => row.Locator("td");

    private static ILocator TargetUserLink(ILocator cell, string userId) =>
        cell.Locator($"a[href$='/users/{userId}']");

    public static Task WaitUntilReadyAsync(IPage page) => HistoryTable(page).WaitForAsync();

    private static async Task<ILocator> CellAsync(IPage page, ILocator row, string field)
    {
        var columnFields = await HeaderCells(page)
            .EvaluateAllAsync<string[]>(
                "headers => headers.map(header => header.getAttribute('data-field') || '')"
            );

        var index = Array.IndexOf(columnFields, field);
        if (index < 0)
            throw new InvalidOperationException($"History is missing column: {field}");

        return RowCells(row).Nth(index);
    }
}
