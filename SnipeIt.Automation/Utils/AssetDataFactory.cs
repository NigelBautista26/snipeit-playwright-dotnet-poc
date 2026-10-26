using System.Text.RegularExpressions;
using SnipeIt.Automation.Constants;
using SnipeIt.Automation.Models;

namespace SnipeIt.Automation.Utils;

public static class AssetDataFactory
{
    public static AssetData Create()
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var tag = $"{AssetConstants.TagPrefix}{DateTime.UtcNow:yyyyMMddHHmmss}-{suffix}";

        return new(
            tag,
            $"G360-{suffix}",
            AssetConstants.Model,
            AssetConstants.Status,
            $"Global 360 QA assessment: {tag}"
        );
    }

    public static void RequireOwnedTag(string tag)
    {
        if (!Regex.IsMatch(tag, @"^g360-poc-\d{14}-[a-f0-9]{12}$"))
        {
            throw new InvalidOperationException(
                $"Refusing to clean up an asset without a generated assessment tag: {tag}"
            );
        }
    }
}
