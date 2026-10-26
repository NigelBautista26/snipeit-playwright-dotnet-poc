using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Pages;
using SnipeIt.Automation.Utils;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.Automation.Tests.Functional;

[NonParallelizable]
[Category("Functional")]
public sealed class AssetFormTests : PageTest
{
    [SetUp]
    public async Task Login()
    {
        await LoginPage.LoginAsync(Page);
    }

    [Test]
    public async Task AssetTagIsRequiredBeforeCreationCanBeSubmitted()
    {
        await AssetCreatePage.OpenAsync(Page);

        await AssetCreatePage.ClearAssetTagAsync(Page);
        await AssetCreatePage.SaveAsync(Page);

        await Expect(AssetCreatePage.AssetTagField(Page)).ToHaveAttributeAsync("required", "");

        Assert.That(new Uri(Page.Url).AbsolutePath, Is.EqualTo(AppPaths.CreateAsset));
    }

    [Test]
    public async Task ReadyToDeployAllowsUserCheckoutAndArchivedDoesNot()
    {
        await AssetCreatePage.OpenAsync(Page);

        await AssetCreatePage.SelectStatusAsync(Page, AssetConstants.Status);

        await Expect(AssetCreatePage.UserDropdown(Page)).ToBeVisibleAsync();

        await AssetCreatePage.SelectStatusAsync(Page, "Archived");

        await Expect(AssetCreatePage.UserDropdown(Page)).ToBeHiddenAsync();

        await AssetCreatePage.SelectStatusAsync(Page, AssetConstants.Status);

        await Expect(AssetCreatePage.UserDropdown(Page)).ToBeVisibleAsync();

        var selectedUser = await AssetCreatePage.SelectRandomUserAsync(Page, TestConfig.RandomSeed);

        await Expect(AssetCreatePage.CheckoutToUserRadio(Page)).ToBeCheckedAsync();

        await Expect(AssetCreatePage.SelectedUserOption(Page))
            .ToHaveAttributeAsync("value", selectedUser.Id);
    }
}
