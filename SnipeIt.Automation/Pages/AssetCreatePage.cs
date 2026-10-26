using System.Text.RegularExpressions;
using Microsoft.Playwright;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Models;
using SnipeIt.Automation.Utils;

namespace SnipeIt.Automation.Pages;

public static class AssetCreatePage
{
    public static ILocator Form(IPage page) => page.Locator("#create-form");

    public static ILocator AssetTagField(IPage page) => page.Locator("#asset_tag");

    public static ILocator ModelDropdown(IPage page) =>
        page.Locator("#select2-model_select_id-container");

    public static ILocator UserDropdown(IPage page) =>
        page.Locator("#select2-assigned_user_select-container");

    public static ILocator CheckoutToUserRadio(IPage page) =>
        page.Locator("input[name='checkout_to_type'][value='user']");

    public static ILocator SelectedUserOption(IPage page) =>
        page.Locator("#assigned_user_select option:checked");

    private static ILocator SerialField(IPage page) => page.Locator("input[name='serials[1]']");

    private static ILocator StatusDropdown(IPage page) =>
        page.Locator("#select2-status_select_id-container");

    private static ILocator DropdownSearch(IPage page) =>
        page.Locator(".select2-container--open input.select2-search__field");

    private static ILocator OpenDropdown(IPage page) => page.Locator(".select2-container--open");

    private static ILocator ModelOption(IPage page, string model) =>
        OpenDropdown(page)
            .GetByRole(
                AriaRole.Option,
                new()
                {
                    NameRegex = new Regex(
                        $@"^(?:.+ - )?{Regex.Escape(model)}(?:\s+\([^)]*\))?$",
                        RegexOptions.IgnoreCase
                    ),
                }
            );

    private static ILocator StatusOption(IPage page, string status) =>
        page.GetByRole(AriaRole.Option, new() { Name = status, Exact = true });

    private static ILocator NotesField(IPage page) => page.Locator("textarea[name='notes']");

    private static ILocator CheckoutToUserLabel(IPage page) =>
        page.Locator("#assignto_selector label").Filter(new() { Has = CheckoutToUserRadio(page) });

    private static ILocator AvailableUserOptions(IPage page) =>
        page.Locator(
            ".select2-container--open "
                + ".select2-results__option[aria-selected]:not([aria-disabled='true'])"
        );

    private static ILocator AssignedUserSelect(IPage page) => page.Locator("#assigned_user_select");

    private static ILocator SaveButton(IPage page) => page.Locator("#submit_button");

    public static async Task OpenAsync(IPage page)
    {
        await page.GotoAsync(TestConfig.Url(AppPaths.CreateAsset));
        await AssetTagField(page).WaitForAsync();
    }

    public static async Task FillAssetAsync(IPage page, AssetData asset)
    {
        await AssetTagField(page).FillAsync(asset.Tag);
        await SerialField(page).FillAsync(asset.Serial);

        await ModelDropdown(page).ClickAsync();
        await DropdownSearch(page).FillAsync(asset.Model);
        await ModelOption(page, asset.Model).First.ClickAsync();

        await SelectStatusAsync(page, asset.Status);
        await NotesField(page).FillAsync(asset.Notes);
    }

    public static Task ClearAssetTagAsync(IPage page) => AssetTagField(page).FillAsync("");

    public static async Task SelectStatusAsync(IPage page, string status)
    {
        await StatusDropdown(page).ClickAsync();
        await StatusOption(page, status).ClickAsync();
    }

    public static async Task<SelectedUser> SelectRandomUserAsync(IPage page, int seed)
    {
        await CheckoutToUserLabel(page).ClickAsync();
        await UserDropdown(page).ClickAsync();

        var options = AvailableUserOptions(page);

        await options.First.WaitForAsync();

        var count = await options.CountAsync();
        var option = options.Nth(new Random(seed).Next(count));

        var label = (await option.InnerTextAsync()).Trim();

        await option.ClickAsync();

        var id = await AssignedUserSelect(page).InputValueAsync();

        if (!int.TryParse(id, out var parsedId) || parsedId <= 0)
        {
            throw new InvalidOperationException(
                $"The selected user '{label}' has no usable user ID."
            );
        }

        return new SelectedUser(id, label);
    }

    public static async Task<byte[]> CaptureStableFormAsync(IPage page)
    {
        await Form(page).WaitForAsync();
        await UserDropdown(page).WaitForAsync();

        await page.EvaluateAsync(
            """
            async () => {
                await document.fonts.ready;
                document.activeElement?.blur();
            }
            """
        );

        byte[]? previousScreenshot = null;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await page.EvaluateAsync(
                """
                () =>
                    new Promise(resolve =>
                        requestAnimationFrame(() =>
                            requestAnimationFrame(resolve)))
                """
            );

            await page.EvaluateAsync("() => document.activeElement?.blur()");

            var currentScreenshot = await CaptureFormOnceAsync(page);

            if (
                previousScreenshot is not null
                && previousScreenshot.SequenceEqual(currentScreenshot)
            )
            {
                return currentScreenshot;
            }

            previousScreenshot = currentScreenshot;
        }

        throw new InvalidOperationException(
            "The asset creation form did not reach a stable visual state."
        );
    }

    private static Task<byte[]> CaptureFormOnceAsync(IPage page) =>
        Form(page)
            .ScreenshotAsync(
                new()
                {
                    Animations = ScreenshotAnimations.Disabled,
                    Caret = ScreenshotCaret.Hide,

                    Mask = [ModelDropdown(page), UserDropdown(page)],

                    Style = """
                    *,
                    *::before,
                    *::after {
                        animation: none !important;
                        transition: none !important;
                        caret-color: transparent !important;
                    }

                    #custom_fields_content {
                        display: none !important;
                    }

                    :focus {
                        outline: none !important;
                        box-shadow: none !important;
                    }
                    """,
                }
            );

    public static Task SaveAsync(IPage page) => SaveButton(page).ClickAsync();
}
