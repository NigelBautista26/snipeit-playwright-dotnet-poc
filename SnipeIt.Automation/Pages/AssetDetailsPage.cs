using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace SnipeIt.Automation.Pages;

public static class AssetDetailsPage
{
    private static ILocator Summary(IPage page) => page.Locator(".box-profile");

    public static ILocator Serial(IPage page, string serial) =>
        Summary(page).GetByText(serial, new() { Exact = true });

    public static ILocator ModelLink(IPage page) =>
        Summary(page).Locator("a[href*='/models/']:not([data-type='image'])");

    public static ILocator DetailsPanel(IPage page) => page.Locator("#details");

    public static ILocator AssignedUser(IPage page, string userId) =>
        DetailsPanel(page).Locator($"a[href$='/users/{userId}']");

    private static ILocator HistoryTab(IPage page) => page.Locator("a[href='#history']");

    private static ILocator DeleteButton(IPage page) => page.Locator("button.delete-asset");

    private static ILocator DeleteConfirmation(IPage page) => page.Locator("#dataConfirmModal");

    private static ILocator ConfirmDeleteButton(IPage page) =>
        DeleteConfirmation(page).Locator("#dataConfirmOK");

    public static async Task OpenHistoryAsync(IPage page)
    {
        await HistoryTab(page).ClickAsync();
        await AssetHistoryPage.WaitUntilReadyAsync(page);
    }

    public static async Task DeleteAsync(IPage page, string expectedTag)
    {
        await DeleteButton(page).ClickAsync();
        await DeleteConfirmation(page).WaitForAsync();

        var confirmationText = await DeleteConfirmation(page).InnerTextAsync();
        if (!confirmationText.Contains(expectedTag, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Delete confirmation does not contain expected asset tag: {expectedTag}"
            );

        await ConfirmDeleteButton(page).ClickAsync();

        // Successful deletion returns to the asset list. The cleanup utility then
        // performs the stronger verification: searching the exact tag must return no rows.
        await page.WaitForURLAsync(new Regex(@"/hardware(?:[?#].*)?$"));
    }
}
