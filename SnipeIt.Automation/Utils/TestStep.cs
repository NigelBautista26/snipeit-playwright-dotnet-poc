using NUnit.Framework;

namespace SnipeIt.Automation.Utils;

public static class TestStep
{
    public static async Task RunAsync(string name, Func<Task> action)
    {
        await TestContext.Progress.WriteLineAsync($"STEP: {name}");
        await action();
    }

    public static async Task<T> RunAsync<T>(string name, Func<Task<T>> action)
    {
        await TestContext.Progress.WriteLineAsync($"STEP: {name}");
        return await action();
    }
}
