using System.Security.Cryptography;
using System.Text.Json;

namespace DotCC.PostProcess;

internal static class Program
{
    private const string Usage = "Usage: dotnet dotcc-postprocess.dll input.csproj --output directory [--configuration Release]";

    private static async Task<int> Main(string[] arguments)
    {
        if (arguments.Length == 1 && arguments[0] is "--help" or "-h")
        {
            Console.WriteLine(Usage);
            return 0;
        }
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler cancel = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += cancel;
        try
        {
            var options = Parse(arguments);
            var paths = SnapshotPaths.Validate(options.Project, options.Output);
            var input = await ProjectInput.ReadAsync(paths.Project, options.Configuration, cancellation.Token);
            var original = input.CreateCompilation(cancellation.Token);
            var result = SourcePostProcessor.Rewrite(original, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();

            // Publish only a complete snapshot. Failed evaluation/rewrites never
            // create output, and a concurrent writer cannot be overwritten.
            Directory.CreateDirectory(Path.GetDirectoryName(paths.Output)!);
            var staging = Path.Combine(Path.GetDirectoryName(paths.Output)!, ".dotcc-postprocess-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(staging);
            try
            {
                var projects = SnapshotProjectWriter.Write(input, original, result.Compilation, staging);
                var files = Directory.EnumerateFiles(staging, "*", SearchOption.AllDirectories)
                    .Select(file => new { Path = Relative(staging, file), Sha256 = Hash(file) })
                    .OrderBy(file => file.Path, StringComparer.Ordinal).ToArray();
                var manifest = new
                {
                    Format = "dotcc-cond-snapshot-v1",
                    InputProject = paths.Project,
                    input.AssemblyName,
                    input.TargetFramework,
                    input.Configuration,
                    OriginalProject = Relative(staging, projects.OriginalProjectPath),
                    OptimizedProject = Relative(staging, projects.OptimizedProjectPath),
                    result.Rewritten,
                    result.Skipped,
                    result.RemovedEmptyBlocks,
                    Diagnostics = result.Diagnostics.Order(StringComparer.Ordinal).ToArray(),
                    Notes = new[]
                    {
                        "Standalone opt-in snapshot; dotcc and SQLite build hooks are unchanged.",
                        "Known SDK analyzers are omitted; source-generator trigger attributes and custom analyzers are rejected.",
                        "Input compiler arguments were evaluated with SkipCompilerExecution=true; project dependencies must already be restored/built."
                    },
                    Sources = input.Arguments.SourceFiles.Select(source => new { Path = source.Path, Sha256 = input.SourceHashes[source.Path] })
                        .OrderBy(source => source.Path, StringComparer.Ordinal).ToArray(),
                    CompilerArguments = input.RawArguments,
                    Files = files
                };
                File.WriteAllText(Path.Combine(staging, "manifest.json"), JsonSerializer.Serialize(manifest,
                    new JsonSerializerOptions { WriteIndented = true }) + "\n");
                cancellation.Token.ThrowIfCancellationRequested();
                foreach (var source in input.SourceHashes)
                    if (Hash(source.Key) != source.Value)
                        throw new InvalidOperationException("Input source changed during processing: " + source.Key);
                if (Directory.Exists(paths.Output) || File.Exists(paths.Output))
                    throw new InvalidOperationException("Output appeared during processing; refusing to overwrite it.");
                Directory.Move(staging, paths.Output);
                Console.WriteLine($"Rewrote {result.Rewritten} Cond.B calls; skipped {result.Skipped}.");
                Console.WriteLine($"Removed {result.RemovedEmptyBlocks} standalone empty blocks.");
                Console.WriteLine(Path.Combine(paths.Output, Relative(staging, projects.OptimizedProjectPath)));
                return 0;
            }
            finally
            {
                if (Directory.Exists(staging)) Directory.Delete(staging, recursive: true);
            }
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Postprocessing cancelled.");
            return 130;
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArgumentException or IOException
            or UnauthorizedAccessException or JsonException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
        finally { Console.CancelKeyPress -= cancel; }
    }

    private static (string Project, string Output, string Configuration) Parse(string[] arguments)
    {
        if (arguments.Length < 3 || arguments[0].StartsWith('-')) throw new ArgumentException(Usage);
        string? output = null;
        string configuration = "Release";
        bool configurationSeen = false;
        for (int index = 1; index < arguments.Length; index += 2)
        {
            if (index + 1 >= arguments.Length) throw new ArgumentException(Usage);
            switch (arguments[index])
            {
                case "--output" when output == null: output = arguments[index + 1]; break;
                case "--configuration" when !configurationSeen:
                    configuration = arguments[index + 1]; configurationSeen = true; break;
                default: throw new ArgumentException(Usage);
            }
        }
        return (arguments[0], output ?? throw new ArgumentException(Usage), configuration);
    }

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
    private static string Hash(string file) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(file)));
}
