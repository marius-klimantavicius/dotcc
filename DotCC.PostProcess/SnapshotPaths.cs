namespace DotCC.PostProcess;

internal static class SnapshotPaths
{
    internal static (string Project, string Output) Validate(string project, string output)
    {
        project = ValidateProject(project);
        output = Path.GetFullPath(output);
        if (Directory.Exists(output) || File.Exists(output))
            throw new InvalidOperationException("Output already exists; refusing to overwrite it.");
        var inputDirectory = Canonical(Path.GetDirectoryName(project)!);
        var actualProject = Canonical(project);
        var actualInputDirectory = Path.GetDirectoryName(actualProject)!;
        var outputDirectory = Canonical(output);
        if (Contains(outputDirectory, actualProject) || Contains(inputDirectory, outputDirectory)
            || Contains(actualInputDirectory, outputDirectory))
            throw new InvalidOperationException("Input and output directories must not overlap, including through symbolic links.");
        return (project, output);
    }

    internal static string ValidateProject(string project)
    {
        project = Path.GetFullPath(project);
        if (!File.Exists(project) || !string.Equals(Path.GetExtension(project), ".csproj", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Input must be an existing C# project (.csproj).");
        return project;
    }

    private static bool Contains(string parent, string child)
    {
        var comparison = (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(parent, child, comparison)
            || child.StartsWith(Path.TrimEndingDirectorySeparator(parent) + Path.DirectorySeparatorChar, comparison);
    }

    internal static string Canonical(string path)
    {
        path = Path.GetFullPath(path);
        var current = Path.GetPathRoot(path)!;
        foreach (var component in path[current.Length..].Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, component);
            FileSystemInfo info = Directory.Exists(current) ? new DirectoryInfo(current) : new FileInfo(current);
            if (info.LinkTarget != null) current = info.ResolveLinkTarget(returnFinalTarget: true)?.FullName
                ?? throw new InvalidOperationException("Cannot resolve symbolic link: " + current);
        }
        return Path.TrimEndingDirectorySeparator(current);
    }
}
