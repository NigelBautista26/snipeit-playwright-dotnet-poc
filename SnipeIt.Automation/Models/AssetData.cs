namespace SnipeIt.Automation.Models;

public sealed record AssetData(
    string Tag,
    string Serial,
    string Model,
    string Status,
    string Notes
);

public sealed record SelectedUser(string Id, string DisplayLabel);
