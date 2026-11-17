using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;

#nullable enable

namespace ZeroLog.Benchmarks.Tools;

public static class NuGetTools
{
    private static string? _nuGetVersion;

    public static string GetNuGetVersion()
        => _nuGetVersion ??= ResolveNuGetVersion();

    private static string ResolveNuGetVersion()
    {
        var commandLineArgs = Environment.GetCommandLineArgs();

        if (commandLineArgs.IndexOf("--nuget") is var index and >= 0 && index + 1 < commandLineArgs.Length)
            return commandLineArgs[index + 1];

        using var httpClient = new HttpClient();

        return httpClient.GetFromJsonAsync<NuGetVersionIndex>("https://api.nuget.org/v3-flatcontainer/zerolog/index.json")
                         .GetAwaiter()
                         .GetResult()!
                         .Versions
                         .Last(i => !i.Contains('-'));
    }

    private sealed record NuGetVersionIndex(string[] Versions);
}
