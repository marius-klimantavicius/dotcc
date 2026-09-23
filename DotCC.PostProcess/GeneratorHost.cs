using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DotCC.PostProcess;

/// <summary>Runs the evaluated Csc generators without writing generated files into the input project.</summary>
internal sealed class GeneratorHost : IDisposable
{
    internal sealed record GeneratedSource(string Phase, string Generator, string HintName, string Path, string Sha256);
    private readonly GeneratorLoader loader;
    private readonly GeneratorDriver driver;
    private readonly Configuration configuration;
    private int runs;
    internal Dictionary<string, string> InputHashes { get; } = new(StringComparer.Ordinal);
    internal SortedSet<string> Diagnostics { get; } = new(StringComparer.Ordinal);
    internal List<GeneratedSource> GeneratedSources { get; } = [];

    internal GeneratorHost(ProjectInput input, CancellationToken token)
    {
        loader = new GeneratorLoader(ReadInput);
        try
        {
            var configs = input.Arguments.AnalyzerConfigPaths.Select(path =>
            {
                var fullPath = Path.GetFullPath(path, input.BaseDirectory);
                return AnalyzerConfig.Parse(Text(fullPath), fullPath);
            }).ToArray();
            var set = AnalyzerConfigSet.Create(configs, out var diagnostics);
            Report(diagnostics, "Analyzer configuration");
            Report(set.GlobalConfigOptions.Diagnostics, "Global analyzer configuration");
            configuration = new Configuration(set);
            var additional = input.Arguments.AdditionalFiles.Select(file =>
            {
                var path = Path.GetFullPath(file.Path, input.BaseDirectory);
                return (AdditionalText)new AdditionalInput(path, Text(path));
            }).ToImmutableArray();
            var paths = input.Arguments.AnalyzerReferences.Select(reference =>
                Path.GetFullPath(reference.FilePath, input.BaseDirectory)).Distinct(StringComparer.Ordinal).ToArray();
            foreach (var path in paths) loader.AddDependencyLocation(path);
            var generators = ImmutableArray.CreateBuilder<ISourceGenerator>();
            foreach (var path in paths)
            {
                token.ThrowIfCancellationRequested();
                ReadInput(path);
                var reference = new AnalyzerFileReference(path, loader);
                var failures = new List<string>();
                reference.AnalyzerLoadFailed += (_, error) =>
                {
                    if (error.ErrorCode != AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers)
                        failures.Add(error.Message);
                };
                generators.AddRange(reference.GetGenerators(LanguageNames.CSharp));
                if (failures.Count != 0)
                    throw new InvalidOperationException("Cannot load source generators from " + path + ":\n" + string.Join("\n", failures));
            }
            driver = CSharpGeneratorDriver.Create(generators.ToImmutable(), additional,
                input.Arguments.ParseOptions, configuration);
        }
        catch { loader.Dispose(); throw; }

        SourceText Text(string path)
        {
            using var bytes = new MemoryStream(ReadInput(path), writable: false);
            return SourceText.From(bytes, input.Arguments.Encoding, input.Arguments.ChecksumAlgorithm);
        }
    }

    internal CSharpCompilation Run(CSharpCompilation input, CancellationToken token)
    {
        input = input.WithOptions(input.Options.WithSyntaxTreeOptionsProvider(configuration.TreeOptions));
        foreach (var tree in input.SyntaxTrees)
            Report(configuration.Set.GetOptionsForSourcePath(tree.FilePath).Diagnostics, "Analyzer configuration");
        // Always start from the unevaluated driver. The second run verifies the
        // rewritten input, even for generators observing source text or trivia.
        var completed = driver.RunGeneratorsAndUpdateCompilation(input, out var output, out var diagnostics, token);
        var result = completed.GetRunResult();
        foreach (var run in result.Results)
        {
            if (run.Exception != null)
                throw new InvalidOperationException("Source generator failed: " + run.Generator.GetType().FullName + "\n" + run.Exception);
            foreach (var source in run.GeneratedSources)
                GeneratedSources.Add(new(runs == 0 ? "Original" : "Optimized",
                    source.SyntaxTree.FilePath[..^(source.HintName.Length + 1)], source.HintName,
                    source.SyntaxTree.FilePath, Convert.ToHexStringLower(SHA256.HashData(
                        System.Text.Encoding.UTF8.GetBytes(source.SourceText.ToString())))));
        }
        Report(CompilationWithAnalyzers.GetEffectiveDiagnostics(diagnostics, output), "Source generation");
        runs++;
        return (CSharpCompilation)output;
    }

    private void Report(IEnumerable<Diagnostic> diagnostics, string phase)
    {
        var visible = diagnostics.Where(d => !d.IsSuppressed && d.Severity != DiagnosticSeverity.Hidden).ToArray();
        foreach (var diagnostic in visible) Diagnostics.Add(diagnostic.ToString());
        if (visible.Any(d => d.Severity == DiagnosticSeverity.Error))
            throw new InvalidOperationException(phase + " failed:\n" + string.Join("\n", visible.Select(d => d.ToString())));
    }

    private byte[] ReadInput(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
        lock (InputHashes)
        {
            if (InputHashes.TryGetValue(path, out var previous) && previous != hash)
                throw new IOException("Generator input changed during processing: " + path);
            InputHashes[path] = hash;
        }
        return bytes;
    }

    internal void VerifyInputs()
    {
        foreach (var path in InputHashes.Keys.ToArray()) ReadInput(path);
    }

    public void Dispose() => loader.Dispose();

    private sealed class AdditionalInput(string path, SourceText text) : AdditionalText
    {
        public override string Path => path;
        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return text;
        }
    }

    private sealed class Options(ImmutableDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override IEnumerable<string> Keys => values.Keys;
        public override bool TryGetValue(string key, out string value) => values.TryGetValue(key, out value!);
    }

    private sealed class Configuration(AnalyzerConfigSet set) : AnalyzerConfigOptionsProvider
    {
        internal AnalyzerConfigSet Set => set;
        internal SyntaxTreeOptionsProvider TreeOptions { get; } = new TreeConfiguration(set);
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(set.GlobalConfigOptions.AnalyzerOptions);
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => new Options(set.GetOptionsForSourcePath(tree.FilePath).AnalyzerOptions);
        public override AnalyzerConfigOptions GetOptions(AdditionalText text) => new Options(set.GetOptionsForSourcePath(text.Path).AnalyzerOptions);
    }

    private sealed class TreeConfiguration(AnalyzerConfigSet set) : SyntaxTreeOptionsProvider
    {
        public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken)
            => set.GetOptionsForSourcePath(tree.FilePath).AnalyzerOptions.TryGetValue("generated_code", out var value)
                && bool.TryParse(value, out var generated) ? generated ? GeneratedKind.MarkedGenerated : GeneratedKind.NotGenerated : GeneratedKind.Unknown;
        public override bool TryGetDiagnosticValue(SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
            => set.GetOptionsForSourcePath(tree.FilePath).TreeOptions.TryGetValue(diagnosticId, out severity);
        public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
            => set.GlobalConfigOptions.TreeOptions.TryGetValue(diagnosticId, out severity);
    }

    /// <summary>Separate analyzer directories can carry different dependency versions.
    /// Roslyn itself is shared with the host, preserving generator interface identity.</summary>
    private sealed class GeneratorLoader(Func<string, byte[]> read) : IAnalyzerAssemblyLoader, IDisposable
    {
        private readonly Dictionary<string, Context> contexts = new(StringComparer.Ordinal);
        private readonly List<string> locations = [];
        public void AddDependencyLocation(string fullPath) => locations.Add(fullPath);
        public Assembly LoadFromPath(string fullPath)
        {
            var directory = Path.GetDirectoryName(fullPath)!;
            if (!contexts.TryGetValue(directory, out var context))
                contexts.Add(directory, context = new Context(directory, locations, read));
            return context.LoadFromAssemblyPath(fullPath);
        }
        public void Dispose() { foreach (var context in contexts.Values) context.Unload(); }

        private sealed class Context(string directory, List<string> locations, Func<string, byte[]> read)
            : AssemblyLoadContext(isCollectible: true)
        {
            protected override Assembly? Load(AssemblyName name)
            {
                if (name.Name == typeof(Compilation).Assembly.GetName().Name) return typeof(Compilation).Assembly;
                if (name.Name == typeof(CSharpCompilation).Assembly.GetName().Name) return typeof(CSharpCompilation).Assembly;
                var local = Path.Combine(directory, name.Name + ".dll");
                var path = File.Exists(local) ? local : locations.FirstOrDefault(candidate =>
                    string.Equals(Path.GetFileNameWithoutExtension(candidate), name.Name, StringComparison.OrdinalIgnoreCase)
                    && AssemblyName.ReferenceMatchesDefinition(AssemblyName.GetAssemblyName(candidate), name));
                if (path == null) return null;
                read(path);
                return LoadFromAssemblyPath(path);
            }
        }
    }
}
