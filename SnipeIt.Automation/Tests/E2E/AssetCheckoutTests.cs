using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using SnipeIt.Automation.Pages;
using SnipeIt.Automation.Utils;
using static Microsoft.Playwright.Assertions;

namespace SnipeIt.Automation.Tests.E2E;

[NonParallelizable]
[Category("E2E")]
public sealed class AssetCheckoutTests : PageTest
{
    [Test]
    [Description(
        "Create a Ready to Deploy MacBook Pro 13-inch, check it out to a random user, and verify list, details and history."
    )]
    public async Task CreatedMacBookIsListedWithCorrectDetailsAndCheckoutHistory()
    {
        var asset = AssetDataFactory.Create();
        var creationAttempted = false;

        try
        {
            await TestStep.RunAsync("Log in to Snipe-IT", () => LoginPage.LoginAsync(Page));

            var selectedUser = await TestStep.RunAsync(
                "Create a Ready to Deploy MacBook and check it out to a random user",
                async () =>
                {
                    await AssetCreatePage.OpenAsync(Page);
                    await AssetCreatePage.FillAssetAsync(Page, asset);
                    var user = await AssetCreatePage.SelectRandomUserAsync(
                        Page,
                        TestConfig.RandomSeed
                    );

                    creationAttempted = true;
                    await AssetCreatePage.SaveAsync(Page);

                    return user;
                }
            );

            await TestStep.RunAsync(
                "Find the created asset in the asset list",
                async () =>
                {
                    await AssetListPage.SearchAsync(Page, asset.Tag);
                    await Expect(AssetListPage.AssetRow(Page, asset.Tag)).ToHaveCountAsync(1);
                }
            );

            await TestStep.RunAsync(
                "Open the asset and verify its saved details",
                async () =>
                {
                    await AssetListPage.OpenAssetFromListAsync(Page, asset.Tag);

                    await Expect(AssetDetailsPage.DetailsPanel(Page)).ToContainTextAsync(asset.Tag);
                    await Expect(AssetDetailsPage.Serial(Page, asset.Serial)).ToBeVisibleAsync();
                    await Expect(AssetDetailsPage.ModelLink(Page)).ToHaveTextAsync(asset.Model);
                    await Expect(AssetDetailsPage.DetailsPanel(Page))
                        .ToContainTextAsync(asset.Status);
                    await Expect(AssetDetailsPage.AssignedUser(Page, selectedUser.Id))
                        .ToHaveCountAsync(1);
                }
            );

            await TestStep.RunAsync(
                "Verify creation and checkout history",
                async () =>
                {
                    await AssetDetailsPage.OpenHistoryAsync(Page);

                    await Expect(AssetHistoryPage.CreationRow(Page)).ToHaveCountAsync(1);
                    await Expect(AssetHistoryPage.CheckoutRow(Page)).ToHaveCountAsync(1);
                    await Expect(
                            await AssetHistoryPage.CheckoutTargetUserAsync(Page, selectedUser.Id)
                        )
                        .ToHaveCountAsync(1);
                }
            );
        }
        finally
        {
            if (creationAttempted && TestConfig.Cleanup)
            {
                await TestStep.RunAsync(
                    "Delete the created asset",
                    () => AssetCleanup.DeleteOwnedAssetAsync(Page, asset)
                );
            }
        }
    }
}
