using Microsoft.Playwright;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Utils;

namespace SnipeIt.Automation.Pages;

public static class LoginPage
{
    private static ILocator UsernameField(IPage page) =>
        page.GetByRole(AriaRole.Textbox, new() { Name = "Username", Exact = true });

    private static ILocator PasswordField(IPage page) => page.Locator("input[name='password']");

    private static ILocator LoginButton(IPage page) =>
        page.GetByRole(AriaRole.Button, new() { Name = "Login", Exact = true });

    private static ILocator DatabaseResetMessage(IPage page) =>
        page.GetByText("RESETTING THE DATABASE", new() { Exact = false });

    public static async Task LoginAsync(IPage page)
    {
        await page.GotoAsync(TestConfig.Url(AppPaths.Login));

        if (await DatabaseResetMessage(page).IsVisibleAsync())
        {
            throw new InvalidOperationException(
                "The Snipe-IT public demo is currently resetting its database."
            );
        }

        await UsernameField(page).FillAsync(TestConfig.Username);
        await PasswordField(page).FillAsync(TestConfig.Password);
        await LoginButton(page).ClickAsync();

        await page.WaitForURLAsync(url => new Uri(url).AbsolutePath != AppPaths.Login);

        await AppShellPage.WaitUntilReadyAsync(page);
    }
}
