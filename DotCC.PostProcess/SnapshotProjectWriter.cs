using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotCC.PostProcess;

internal sealed record SnapshotProjects(string OriginalProjectPath, string OptimizedProjectPath);

/// <summary>
/// Writes a flattened compilation with its exact metadata inputs. SDK framework
/// references remain available to restore/publish, but cannot replace the inputs
/// seen by Csc. Runtime implementation assemblies are deliberately kept separate
/// from reference assemblies so consumers and NativeAOT receive executable code.
/// </summary>
internal static class SnapshotProjectWriter
{
    private static readonly UTF8Encoding Utf8 = new(false);

    internal static SnapshotProjects Write(ProjectInput input, CSharpCompilation original,
        CSharpCompilation optimized, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        // Prevent the input repository (or the destination's parents) from
        // injecting build targets, source files, packages, or compiler options.
        foreach (var name in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props" })
            File.WriteAllText(Path.Combine(outputDirectory, name), "<Project />\n", Utf8);

        var references = new List<(string Path, MetadataReferenceProperties Properties)>();
        foreach (var reference in original.References)
        {
            if (reference is not PortableExecutableReference { FilePath: { } path }
                || reference.Properties.Kind != MetadataImageKind.Assembly)
                throw new InvalidOperationException("Snapshot references must be file-backed assemblies.");
            references.Add((CopyAsset(path, "refs", references.Count, outputDirectory), reference.Properties));
        }

        var runtimeReferences = new List<string>();
        var runtimeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in input.RuntimeReferencePaths.Order(StringComparer.Ordinal))
        {
            if (!string.Equals(Path.GetExtension(path), ".dll", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Unsupported runtime dependency (expected an implementation DLL): " + path);
            var name = Path.GetFileName(path);
            if (runtimeNames.TryGetValue(name, out var previous))
            {
                if (!SameBytes(previous, path))
                    throw new InvalidOperationException("Runtime dependencies have conflicting filenames: " + previous + " and " + path);
                continue;
            }
            runtimeNames.Add(name, path);
            var relative = "runtime/" + name;
            Directory.CreateDirectory(Path.Combine(outputDirectory, "runtime"));
            File.Copy(path, Path.Combine(outputDirectory, relative));
            runtimeReferences.Add(relative);
        }

        string? sourceLink = null;
        if (input.Arguments.SourceLink is { Length: > 0 } sourceLinkPath)
            sourceLink = CopyAsset(Resolve(input, sourceLinkPath), "assets", 0, outputDirectory);
        var analyzerConfigs = input.Arguments.AnalyzerConfigPaths.Select((path, index) =>
            CopyAsset(Resolve(input, path), "analyzerconfigs", index, outputDirectory)).ToArray();

        // Preserve resource bytes, visibility and logical names without passing
        // through SDK .resx processing or namespace-based name inference.
        var resourceArguments = new List<string>();
        foreach (var argument in input.RawArguments)
        {
            var (key, value) = Option(argument);
            if (key is not ("resource" or "res")) continue;
            var parts = SplitResource(value);
            var path = Resolve(input, parts[0]);
            var copied = CopyAsset(path, "resources", resourceArguments.Count, outputDirectory);
            var logicalName = parts.Count > 1 && parts[1].Length != 0 ? parts[1] : Path.GetFileName(path);
            var access = parts.Count > 2 ? parts[2] : "public";
            if (parts.Count > 3 || access is not ("public" or "private"))
                throw new InvalidOperationException("Unsupported resource argument: " + argument);
            resourceArguments.Add("/resource:" + Quote("../" + copied) + "," + Quote(logicalName) + "," + access);
        }

        var originals = original.SyntaxTrees.ToArray();
        var rewrites = optimized.SyntaxTrees.ToArray();
        // Regeneration may change the number or names of generated documents.
        // Each variant captures its own complete compilation.
        var originalProject = WriteVariant("Original", "Original.csproj", originals);
        var optimizedProject = WriteVariant("Optimized", input.AssemblyName + ".csproj", rewrites);
        return new(originalProject, optimizedProject);

        string WriteVariant(string directory, string projectName, SyntaxTree[] trees)
        {
            var variant = Path.Combine(outputDirectory, directory);
            Directory.CreateDirectory(Path.Combine(variant, "src"));
            var sourcePaths = new Dictionary<string, string>(StringComparer.Ordinal);
            var sourceItems = new XElement("ItemGroup");
            var mappings = new List<string>();
            for (var index = 0; index < trees.Length; index++)
            {
                var tree = trees[index];
                // Csc maps directory prefixes, not individual source filenames.
                // Keep the basename unchanged and isolate each source in its
                // own directory, including files with identical basenames.
                var sourceDirectory = "src/" + index.ToString("D5") + "/";
                var source = sourceDirectory + Path.GetFileName(tree.FilePath);
                Directory.CreateDirectory(Path.Combine(variant, sourceDirectory));
                File.WriteAllText(Path.Combine(variant, source), tree.GetText().ToString(), tree.GetText().Encoding ?? Utf8);
                var fullOriginalPath = Path.GetFullPath(tree.FilePath, input.BaseDirectory);
                if (!sourcePaths.TryAdd(fullOriginalPath, source))
                    throw new InvalidOperationException("Duplicate source path in compilation: " + fullOriginalPath);
                sourceItems.Add(Item("Compile", source));
                // MSBuild expands the destination at build time, after the
                // staging directory has been atomically renamed by the caller.
                var originalDirectory = Path.GetDirectoryName(fullOriginalPath)! + Path.DirectorySeparatorChar;
                mappings.Add("$(MSBuildProjectDirectory)/" + sourceDirectory + "=" + PathMapPart(MapOriginal(originalDirectory, input)));
            }
            foreach (var map in input.Arguments.PathMap)
                mappings.Add(PathMapPart(map.Key) + "=" + PathMapPart(map.Value));

            foreach (var embedded in input.Arguments.EmbeddedFiles)
            {
                var path = Resolve(input, embedded.Path);
                if (!sourcePaths.TryGetValue(path, out var copy))
                    throw new InvalidOperationException("Embedded source is outside the evaluated source files: " + path);
                sourceItems.Add(Item("EmbeddedFiles", copy));
            }

            var options = original.Options;
            var properties = new XElement("PropertyGroup",
                Property("TargetFramework", input.TargetFramework),
                Property("AssemblyName", input.AssemblyName),
                Property("OutputType", options.OutputKind switch
                {
                    OutputKind.DynamicallyLinkedLibrary => "Library",
                    OutputKind.ConsoleApplication => "Exe",
                    OutputKind.WindowsApplication => "WinExe",
                    _ => throw new InvalidOperationException("Unsupported snapshot output kind: " + options.OutputKind)
                }),
                Property("EnableDefaultItems", "false"),
                Property("ImportDirectoryBuildTargets", "false"),
                Property("GenerateAssemblyInfo", "false"),
                Property("GenerateTargetFrameworkAttribute", "false"),
                Property("ImplicitUsings", "disable"),
                Property("DisableImplicitFrameworkDefines", "true"),
                Property("EnableNETAnalyzers", "false"),
                Property("RunAnalyzersDuringBuild", "false"),
                Property("RunAnalyzersDuringLiveAnalysis", "false"),
                Property("GenerateDocumentationFile", Bool(input.Arguments.DocumentationPath != null)),
                Property("ProduceReferenceAssembly", "true"),
                Property("GenerateDependencyFile", "true"),
                Property("CopyLocalLockFileAssemblies", "true"),
                Property("EnableSourceControlManagerQueries", "false"),
                Property("EnableSourceLink", "false"));

            // Apply these both at evaluation and immediately before Csc. SDK
            // configuration/platform defaults must not change source semantics.
            var semantics = new XElement("PropertyGroup",
                Property("LangVersion", input.Arguments.ParseOptions.LanguageVersion.ToDisplayString()),
                Property("DefineConstants", string.Join(";", input.Arguments.ParseOptions.PreprocessorSymbolNames)),
                Property("AllowUnsafeBlocks", Bool(options.AllowUnsafe)),
                Property("CheckForOverflowUnderflow", Bool(options.CheckOverflow)),
                Property("Optimize", Bool(options.OptimizationLevel == OptimizationLevel.Release)),
                Property("Nullable", options.NullableContextOptions.ToString().ToLowerInvariant()),
                Property("Deterministic", Bool(options.Deterministic)),
                Property("WarningLevel", options.WarningLevel.ToString()),
                Property("TreatWarningsAsErrors", Bool(options.GeneralDiagnosticOption == ReportDiagnostic.Error)),
                Property("NoWarn", string.Join(";", options.SpecificDiagnosticOptions.Where(pair => pair.Value == ReportDiagnostic.Suppress).Select(pair => pair.Key).Order(StringComparer.Ordinal))),
                Property("WarningsAsErrors", string.Join(";", options.SpecificDiagnosticOptions.Where(pair => pair.Value == ReportDiagnostic.Error).Select(pair => pair.Key).Order(StringComparer.Ordinal))),
                Property("WarningsNotAsErrors", string.Join(";", options.SpecificDiagnosticOptions.Where(pair => pair.Value == ReportDiagnostic.Warn).Select(pair => pair.Key).Order(StringComparer.Ordinal))),
                Property("PlatformTarget", options.Platform.ToString()),
                Property("Prefer32Bit", Bool(options.Platform == Platform.AnyCpu32BitPreferred)),
                Property("StartupObject", options.MainTypeName ?? ""),
                Property("NoCompilerStandardLib", "true"),
                Property("DebugSymbols", Bool(input.Arguments.EmitPdb)),
                Property("DebugType", !input.Arguments.EmitPdb ? "none" : input.Arguments.EmitOptions.DebugInformationFormat switch
                {
                    Microsoft.CodeAnalysis.Emit.DebugInformationFormat.PortablePdb => "portable",
                    Microsoft.CodeAnalysis.Emit.DebugInformationFormat.Embedded => "embedded",
                    _ => "full"
                }),
                Property("PathMap", string.Join(",", mappings)),
                Property("SourceLink", sourceLink == null ? "" : "../" + sourceLink),
                Property("CompilerResponseFile", "$(MSBuildProjectDirectory)/compiler.rsp"));
            properties.Add(semantics.Elements().Select(element => new XElement(element)));

            var runtimeItems = new XElement("ItemGroup");
            foreach (var runtime in runtimeReferences)
                runtimeItems.Add(new XElement("Reference", new XAttribute("Include", Path.GetFileNameWithoutExtension(runtime)),
                    Property("HintPath", "../" + runtime), Property("Private", "true")));

            var exactInputs = new XElement("ItemGroup",
                Remove("ReferencePathWithRefAssemblies"), Remove("Analyzer"), Remove("EditorConfigFiles"),
                Remove("_CoreCompileResourceInputs"));
            foreach (var (path, metadata) in references)
            {
                var item = Item("ReferencePathWithRefAssemblies", "../" + path);
                if (!metadata.Aliases.IsDefaultOrEmpty) item.Add(Property("Aliases", string.Join(",", metadata.Aliases)));
                if (metadata.EmbedInteropTypes) item.Add(Property("EmbedInteropTypes", "true"));
                exactInputs.Add(item);
            }
            foreach (var config in analyzerConfigs)
                exactInputs.Add(Item("EditorConfigFiles", "../" + config));
            var target = new XElement("Target", new XAttribute("Name", "RestoreSnapshotCompilerInputs"),
                new XAttribute("BeforeTargets", "CoreCompile"), semantics, exactInputs);
            var project = new XElement("Project", new XAttribute("Sdk", "Microsoft.NET.Sdk"),
                properties, sourceItems, runtimeItems, target);
            var projectPath = Path.Combine(variant, projectName);
            File.WriteAllText(projectPath, project + "\n", Utf8);

            var response = input.RawArguments.Where(argument => KeepOption(argument)).Concat(resourceArguments);
            File.WriteAllText(Path.Combine(variant, "compiler.rsp"), string.Join("\n", response) + "\n", Utf8);
            return projectPath;
        }
    }

    private static bool KeepOption(string argument)
    {
        var (key, _) = Option(argument);
        // Source paths and every option referring to an input/output path are
        // represented by copied files and SDK items. Other switches preserve the
        // evaluated compiler settings, including /features and warning ordering.
        return key is "unsafe" or "checked" or "nowarn" or "fullpaths" or "nostdlib" or "errorreport"
            or "warn" or "define" or "d" or "highentropyva" or "nullable" or "features" or "debug"
            or "filealign" or "optimize" or "warnaserror" or "utf8output" or "deterministic"
            or "langversion" or "platform" or "main" or "codepage" or "checksumalgorithm"
            or "preferreduilang" or "errorendlocation" or "nowin32manifest" or "subsystemversion" or "baseaddress";
    }

    private static (string Key, string Value) Option(string argument)
    {
        if (argument.Length == 0 || argument[0] is not ('/' or '-')) return ("", "");
        var colon = argument.IndexOf(':');
        return ((colon < 0 ? argument[1..] : argument[1..colon]).TrimEnd('+', '-').ToLowerInvariant(),
            colon < 0 ? "" : argument[(colon + 1)..]);
    }

    private static List<string> SplitResource(string value)
    {
        var result = new List<string>();
        var part = new StringBuilder();
        var quoted = false;
        foreach (var c in value)
        {
            if (c == '"') quoted = !quoted;
            else if (c == ',' && !quoted) { result.Add(part.ToString()); part.Clear(); }
            else part.Append(c);
        }
        if (quoted) throw new InvalidOperationException("Unterminated resource argument: " + value);
        result.Add(part.ToString());
        return result;
    }

    private static string MapOriginal(string path, ProjectInput input)
    {
        foreach (var pair in input.Arguments.PathMap)
            if (path.StartsWith(pair.Key, StringComparison.Ordinal))
                return pair.Value + path[pair.Key.Length..];
        return path;
    }

    private static string PathMapPart(string path)
    {
        // Csc's path map syntax uses doubled separators for literal ','/'='.
        return path.Replace(",", ",,", StringComparison.Ordinal).Replace("=", "==", StringComparison.Ordinal);
    }

    private static string CopyAsset(string path, string category, int index, string output)
    {
        var relative = category + "/" + index.ToString("D5") + "/" + SafeName(Path.GetFileName(path));
        var destination = Path.Combine(output, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        File.Copy(path, destination);
        return relative;
    }

    private static string Resolve(ProjectInput input, string path) => Path.GetFullPath(path, input.BaseDirectory);
    private static string SafeName(string name) => string.Concat(name.Select(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-' ? c : '_'));
    private static string Bool(bool value) => value ? "true" : "false";
    private static string Quote(string text)
    {
        if (text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
            throw new InvalidOperationException("Unsupported quote/newline in compiler asset name.");
        return "\"" + text + "\"";
    }
    private static XElement Property(string name, string value) => new(name, value);
    private static XElement Item(string name, string include) => new(name, new XAttribute("Include", EscapeMsBuild(include)));
    private static XElement Remove(string name) => new(name, new XAttribute("Remove", "@(" + name + ")"));
    private static string EscapeMsBuild(string value) => value.Replace("%", "%25", StringComparison.Ordinal)
        .Replace(";", "%3B", StringComparison.Ordinal).Replace("$", "%24", StringComparison.Ordinal)
        .Replace("@", "%40", StringComparison.Ordinal).Replace("'", "%27", StringComparison.Ordinal)
        .Replace("(", "%28", StringComparison.Ordinal).Replace(")", "%29", StringComparison.Ordinal)
        .Replace("?", "%3F", StringComparison.Ordinal).Replace("*", "%2A", StringComparison.Ordinal);
    private static bool SameBytes(string left, string right)
    {
        using var a = File.OpenRead(left);
        using var b = File.OpenRead(right);
        return a.Length == b.Length && SHA256.HashData(a).AsSpan().SequenceEqual(SHA256.HashData(b));
    }
}
