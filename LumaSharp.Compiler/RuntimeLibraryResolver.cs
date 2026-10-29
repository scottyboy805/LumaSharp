namespace LumaSharp.Compiler;

internal static class RuntimeLibraryResolver
{
    public static string? Resolve(string? configuredPath, IEnumerable<string> sourcePaths)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            var configuredDirectory = Path.GetFullPath(configuredPath);
            if (!HasSources(configuredDirectory))
                throw new DirectoryNotFoundException($"Runtime library directory '{configuredDirectory}' does not exist or contains no .ls source files.");
            return configuredDirectory;
        }

        foreach (var sourcePath in sourcePaths)
        {
            var sourceDirectory = Path.GetDirectoryName(Path.GetFullPath(sourcePath));
            var match = FindInAncestors(sourceDirectory);
            if (match is not null)
                return match;
        }

        var assemblyLocation = typeof(LumaBuild).Assembly.Location;
        var compilerDirectory = string.IsNullOrWhiteSpace(assemblyLocation)
            ? AppContext.BaseDirectory
            : Path.GetDirectoryName(assemblyLocation);
        return FindInAncestors(compilerDirectory);
    }

    private static string? FindInAncestors(string? directory)
    {
        for (var current = directory is null ? null : new DirectoryInfo(directory); current is not null; current = current.Parent)
        {
            var candidate = Path.Combine(current.FullName, "LumaSharp.Library");
            if (HasSources(candidate))
                return candidate;
        }
        return null;
    }

    private static bool HasSources(string directory)
    {
        if (!Directory.Exists(directory))
            return false;

        return Directory.EnumerateFiles(directory, "*.ls", SearchOption.AllDirectories).Any();
    }
}
