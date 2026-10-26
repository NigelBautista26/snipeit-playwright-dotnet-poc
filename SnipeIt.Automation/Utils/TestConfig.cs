namespace SnipeIt.Automation.Utils;

public static class TestConfig
{
    private static readonly Uri BaseOrigin = ReadBaseUrl();

    public static string Username { get; } = ReadUsername(BaseOrigin);
    public static string Password { get; } = ReadPassword(BaseOrigin);
    public static bool Cleanup { get; } = ReadBool("CLEANUP", true);
    public static int RandomSeed { get; } =
        ReadPositiveInt("RANDOM_SEED", Random.Shared.Next(1, int.MaxValue));

    public static string Url(string path) => new Uri(BaseOrigin, path).ToString();

    private static Uri ReadBaseUrl()
    {
        var url = new Uri(
            Environment.GetEnvironmentVariable("SNIPEIT_BASE_URL") ?? "https://demo.snipeitapp.com"
        );

        if (
            url.Scheme is not ("http" or "https")
            || url.AbsolutePath != "/"
            || !string.IsNullOrEmpty(url.Query)
            || !string.IsNullOrEmpty(url.Fragment)
            || !string.IsNullOrEmpty(url.UserInfo)
        )
        {
            throw new ArgumentException(
                "SNIPEIT_BASE_URL must be an HTTP(S) origin without a path, credentials or query."
            );
        }

        return url;
    }

    private static string ReadUsername(Uri baseUrl) =>
        Environment.GetEnvironmentVariable("SNIPEIT_USERNAME")
        ?? (
            IsPublicDemo(baseUrl)
                ? "admin"
                : throw new ArgumentException("Set SNIPEIT_USERNAME for a non-demo instance.")
        );

    private static string ReadPassword(Uri baseUrl) =>
        Environment.GetEnvironmentVariable("SNIPEIT_PASSWORD")
        ?? (
            IsPublicDemo(baseUrl)
                ? "password"
                : throw new ArgumentException("Set SNIPEIT_PASSWORD for a non-demo instance.")
        );

    private static bool IsPublicDemo(Uri url) =>
        url.Scheme == "https" && url.Host == "demo.snipeitapp.com" && url.IsDefaultPort;

    private static bool ReadBool(string name, bool fallback) =>
        Environment.GetEnvironmentVariable(name) is { } value
            ? bool.TryParse(value, out var parsed)
                ? parsed
                : throw new ArgumentException($"{name} must be true or false.")
            : fallback;

    private static int ReadPositiveInt(string name, int fallback) =>
        Environment.GetEnvironmentVariable(name) is { } value
            ? int.TryParse(value, out var parsed) && parsed > 0
                ? parsed
                : throw new ArgumentException($"{name} must be a positive integer.")
            : fallback;
}
