using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DotCC.PostProcess;

internal sealed record ProjectInput(string ProjectPath, string AssemblyName, string TargetFramework,
    string Configuration, CSharpCommandLineArguments Arguments, IReadOnlyList<string> RawArguments,
    IReadOnlyList<string> RuntimeReferencePaths) : IDisposable
{
    internal Dictionary<string, string> SourceHashes { get; } = new(StringComparer.Ordinal);

    internal GeneratorHost? Generators { get; private set; }
    internal bool IsProjectSource(SyntaxTree tree) => SourceHashes.ContainsKey(tree.FilePath);
    public void Dispose() => Generators?.Dispose();

    internal string BaseDirectory => Arguments.BaseDirectory ?? Path.GetDirectoryName(ProjectPath)!;

    internal static async Task<ProjectInput> ReadAsync(string project, string configuration, CancellationToken cancellationToken)
    {
        if (!Regex.IsMatch(configuration, "^[A-Za-z0-9_.-]+$"))
            throw new InvalidOperationException("Configuration must be a simple MSBuild configuration name.");
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(project)!,
            UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var argument in new[] { "msbuild", project, "-t:ResolveReferences;PrepareResources;Compile", "-nologo",
            "-p:Configuration=" + configuration, "-p:SkipCompilerExecution=true", "-p:ProvideCommandLineArgs=true",
            "-p:NonExistentFile=" + Path.Combine(Path.GetTempPath(), "dotcc-postprocess-evaluate-" + Guid.NewGuid().ToString("N")),
            "-p:BuildProjectReferences=false", "-getItem:CscCommandLineArgs,ReferenceCopyLocalPaths",
            "-getProperty:TargetPath,AssemblyName,OutputType,TargetFramework" })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet msbuild.");
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        try { await process.WaitForExitAsync(cancellationToken); }
        catch
        {
            try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            throw;
        }
        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
            throw new InvalidOperationException("MSBuild could not evaluate compiler inputs. Restore/build dependencies first.\n" + output + error);
        using var document = JsonDocument.Parse(output);
        var root = document.RootElement;
        var properties = root.GetProperty("Properties");
        var arguments = root.GetProperty("Items").GetProperty("CscCommandLineArgs").EnumerateArray()
            .Select(item => item.GetProperty("Identity").GetString()!).ToArray();
        if (arguments.Length == 0) throw new InvalidOperationException("MSBuild returned no Csc compiler arguments.");
        var parsed = CSharpCommandLineParser.Default.Parse(arguments, start.WorkingDirectory, sdkDirectory: null);
        var errors = parsed.Errors.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0) throw new InvalidOperationException("Unsupported compiler arguments:\n" + string.Join("\n", errors.Select(e => e.ToString())));
        var assemblyName = properties.GetProperty("AssemblyName").GetString()!;
        if (string.IsNullOrWhiteSpace(assemblyName) || Path.GetFileName(assemblyName) != assemblyName
            || assemblyName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidOperationException("AssemblyName must be a valid simple filename.");
        var framework = properties.GetProperty("TargetFramework").GetString()!;
        if (framework != "net10.0")
            throw new InvalidOperationException("The standalone snapshot currently supports the single target framework net10.0.");
        var runtime = root.GetProperty("Items").GetProperty("ReferenceCopyLocalPaths").EnumerateArray()
            .Select(item => item.TryGetProperty("FullPath", out var full) ? full.GetString()! : item.GetProperty("Identity").GetString()!)
            .Select(path => Path.GetFullPath(path, start.WorkingDirectory)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        return new(project, assemblyName, framework, configuration, parsed, arguments, runtime);
    }

    internal CSharpCompilation CreateCompilation(CancellationToken cancellationToken)
    {
        ValidateOptions();
        var trees = Arguments.SourceFiles.Select(source =>
        {
            var bytes = File.ReadAllBytes(source.Path);
            SourceHashes[source.Path] = Convert.ToHexStringLower(SHA256.HashData(bytes));
            using var stream = new MemoryStream(bytes, writable: false);
            var text = SourceText.From(stream, Arguments.Encoding, Arguments.ChecksumAlgorithm, canBeEmbedded: true);
            return CSharpSyntaxTree.ParseText(text, Arguments.ParseOptions, source.Path, cancellationToken);
        }).ToArray();
        var references = Arguments.MetadataReferences.Select(reference =>
        {
            if (reference.Properties.Kind != MetadataImageKind.Assembly)
                throw new InvalidOperationException("Module metadata references are not supported.");
            var path = ResolveReference(reference.Reference);
            return MetadataReference.CreateFromFile(path, reference.Properties);
        }).ToArray();
        var compilation = CSharpCompilation.Create(AssemblyName, trees, references, Arguments.CompilationOptions);
        Generators = new GeneratorHost(this, cancellationToken);
        return Generators.Run(compilation, cancellationToken);
    }

    internal CSharpCompilation Regenerate(CSharpCompilation compilation, CancellationToken cancellationToken)
    {
        var sources = compilation.RemoveSyntaxTrees(compilation.SyntaxTrees.Where(tree => !IsProjectSource(tree)));
        return Generators!.Run(sources, cancellationToken);
    }

    internal string ResolveReference(string path)
    {
        if (Path.IsPathRooted(path) && File.Exists(path)) return Path.GetFullPath(path);
        foreach (var directory in new[] { BaseDirectory }.Concat(Arguments.ReferencePaths))
        {
            var candidate = Path.GetFullPath(path, directory);
            if (File.Exists(candidate)) return candidate;
        }
        throw new InvalidOperationException("Cannot resolve evaluated metadata reference: " + path);
    }

    private void ValidateOptions()
    {
        // These require build-time behavior beyond a flattened C# compilation.
        // Never produce a snapshot which silently drops their semantics.
        var rejected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "keyfile", "keycontainer", "delaysign", "publicsign", "addmodule", "moduleassemblyname",
            "win32res", "win32icon", "win32manifest", "linkresource", "linkres", "appconfig",
            "ruleset", "instrument", "refonly", "recurse"
        };
        var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "noconfig", "unsafe", "checked", "nowarn", "fullpaths", "nostdlib", "errorreport", "warn",
            "define", "d", "highentropyva", "nullable", "features", "debug", "filealign", "optimize",
            "out", "refout", "target", "t", "warnaserror", "utf8output", "deterministic", "sourcelink",
            "langversion", "embed", "analyzerconfig", "analyzer", "reference", "r", "resource", "res",
            "pathmap", "platform", "main", "pdb", "doc", "codepage", "checksumalgorithm", "nologo",
            "lib", "preferreduilang", "errorendlocation", "reportanalyzer", "skipanalyzers", "nowin32manifest",
            "subsystemversion", "baseaddress", "additionalfile", "additionalfiles", "generatedfilesout"
        };
        foreach (var argument in RawArguments)
        {
            var unquoted = argument.Length >= 2 && argument[0] == '\"' && argument[^1] == '\"' ? argument[1..^1] : argument;
            if ((!unquoted.StartsWith('/') && !unquoted.StartsWith('-')) || File.Exists(unquoted))
            {
                var candidate = Path.GetFullPath(unquoted, BaseDirectory);
                // Roslyn can retain parent segments in linked source paths.
                // Normalize for comparison without changing the observable source path.
                if (Arguments.SourceFiles.Any(source => Path.GetFullPath(source.Path, BaseDirectory) == candidate)) continue;
            }
            if (!argument.StartsWith('/') && !argument.StartsWith('-'))
                throw new InvalidOperationException("Unsupported compiler input: " + argument);
            var key = argument[1..].Split(':', 2)[0].TrimEnd('+', '-');
            if (rejected.Contains(key) || !supported.Contains(key))
                throw new InvalidOperationException("The standalone snapshot does not support compiler option " + argument);
        }
        foreach (var config in Arguments.AnalyzerConfigPaths)
        {
            var path = Path.GetFullPath(config, BaseDirectory).Replace('\\', '/');
            bool sdk = path.Contains("/sdk/", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".globalconfig", StringComparison.Ordinal);
            bool generated = path.StartsWith(BaseDirectory.Replace('\\', '/') + "/obj/", StringComparison.Ordinal)
                && path.EndsWith(".GeneratedMSBuildEditorConfig.editorconfig", StringComparison.Ordinal);
            if (!sdk && !generated)
                throw new InvalidOperationException("Custom analyzer configuration is unsupported because source paths are relocated: " + config);
        }
    }
}
