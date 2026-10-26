using Microsoft.Playwright;

namespace SnipeIt.Automation.Pages;

public static class AppShellPage
{
    private static ILocator AccountMenu(IPage page) =>
        page.Locator("li.user.user-menu > a.dropdown-toggle");

    private static ILocator CsrfTokenMeta(IPage page) => page.Locator("meta[name='csrf-token']");

    public static Task WaitUntilReadyAsync(IPage page) => AccountMenu(page).WaitForAsync();

    public static async Task<string> GetCsrfTokenAsync(IPage page)
    {
        var token = await CsrfTokenMeta(page).GetAttributeAsync("content");

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("The authenticated page has no CSRF token.");

        return token;
    }
}
