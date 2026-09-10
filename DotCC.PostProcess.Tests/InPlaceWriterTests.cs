using System.Security.Cryptography;
using System.Text;
using DotCC.PostProcess;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using Xunit;

namespace DotCC.PostProcess.Tests;

public sealed class InPlaceWriterTests
{
    private static readonly MetadataReference[] References = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p)).ToArray();

    private sealed class Fixture : IDisposable
    {
        internal string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "dotcc-in-place-" + Guid.NewGuid().ToString("N"));
        internal string[] Paths { get; }
        internal byte[][] Before { get; }
        internal CSharpCompilation Original { get; }
        internal CSharpCompilation Rewritten { get; }
        internal Dictionary<string, string> Hashes { get; }

        internal Fixture(Encoding? encoding = null)
        {
            encoding ??= new UTF8Encoding(false);
            Directory.CreateDirectory(DirectoryPath);
            Paths = new[] { "First.cs", "Second.cs", "Helper.cs" }.Select(n => Path.Combine(DirectoryPath, n)).ToArray();
            var contents = new[] {
                "static partial class Case { public static bool First() { {} return Cond.B(1); } } // café\r\n",
                "static partial class Case { public static bool Second() => Cond.B(0); }\r\n",
                "static class Cond { public static bool B(int x) => x != 0; }\r\n"
            };
            Before = contents.Select(s => encoding.GetPreamble().Concat(encoding.GetBytes(s)).ToArray()).ToArray();
            var trees = new List<SyntaxTree>();
            for (int i = 0; i < Paths.Length; i++)
            {
                File.WriteAllBytes(Paths[i], Before[i]);
                using var stream = new MemoryStream(Before[i]);
                trees.Add(CSharpSyntaxTree.ParseText(SourceText.From(stream, encoding), path: Paths[i]));
            }
            Hashes = Paths.Select((path, i) => (path, hash: Convert.ToHexStringLower(SHA256.HashData(Before[i]))))
                .ToDictionary(p => p.path, p => p.hash);
            Original = CSharpCompilation.Create("Case", trees, References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            Rewritten = SourcePostProcessor.Rewrite(Original).Compilation;
        }
        internal void Unchanged()
        {
            for (int i = 0; i < Paths.Length; i++) File.ReadAllBytes(Paths[i]).ShouldBe(Before[i]);
        }
        internal void NoTemporaryFiles() => Directory.GetFiles(DirectoryPath, ".dotcc-postprocess-*").ShouldBeEmpty();
        public void Dispose()
        {
            foreach (var path in Directory.GetFiles(DirectoryPath)) File.SetAttributes(path, FileAttributes.Normal);
            Directory.Delete(DirectoryPath, recursive: true);
        }
    }

    [Theory]
    [InlineData("utf8", false)]
    [InlineData("utf8", true)]
    [InlineData("utf16le", false)]
    [InlineData("utf16le", true)]
    [InlineData("utf16be", true)]
    public void Preserves_encoding_BOM_line_endings_and_unchanged_file_timestamps(string kind, bool bom)
    {
        Encoding encoding = kind == "utf8" ? new UTF8Encoding(bom) : new UnicodeEncoding(kind == "utf16be", bom);
        using var fixture = new Fixture(encoding);
        var helperTime = File.GetLastWriteTimeUtc(fixture.Paths[2]);
        InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, TestContext.Current.CancellationToken).ShouldBe(2);
        for (int i = 0; i < fixture.Paths.Length; i++)
        {
            var text = fixture.Rewritten.SyntaxTrees.ElementAt(i).GetRoot(TestContext.Current.CancellationToken).ToFullString();
            File.ReadAllBytes(fixture.Paths[i]).ShouldBe(encoding.GetPreamble().Concat(encoding.GetBytes(text)).ToArray());
        }
        File.GetLastWriteTimeUtc(fixture.Paths[2]).ShouldBe(helperTime);
        fixture.NoTemporaryFiles();
    }

    [Fact]
    public void No_op_does_not_touch_files()
    {
        using var fixture = new Fixture();
        var times = fixture.Paths.Select(File.GetLastWriteTimeUtc).ToArray();
        InPlaceWriter.Write(fixture.Original, fixture.Original, fixture.Hashes, TestContext.Current.CancellationToken).ShouldBe(0);
        fixture.Unchanged();
        fixture.Paths.Select(File.GetLastWriteTimeUtc).ShouldBe(times);
        fixture.NoTemporaryFiles();
    }

    [Fact]
    public void Rejects_changed_input_even_when_that_file_needs_no_rewrite()
    {
        using var fixture = new Fixture();
        File.AppendAllText(fixture.Paths[2], "// user's edit");
        Should.Throw<IOException>(() => InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, default))
            .Message.ShouldContain("Input source changed");
        File.ReadAllBytes(fixture.Paths[0]).ShouldBe(fixture.Before[0]);
        File.ReadAllText(fixture.Paths[2]).ShouldContain("user's edit");
        fixture.NoTemporaryFiles();
    }

    [Fact]
    public void Revalidates_serialized_source_before_replacing_any_file()
    {
        using var fixture = new Fixture();
        var tree = fixture.Rewritten.SyntaxTrees.Last();
        var invalid = fixture.Rewritten.ReplaceSyntaxTree(tree, CSharpSyntaxTree.ParseText("invalid source", path: tree.FilePath,
            cancellationToken: TestContext.Current.CancellationToken));
        Should.Throw<InvalidOperationException>(() => InPlaceWriter.Write(fixture.Original, invalid, fixture.Hashes, default))
            .Message.ShouldContain("Serialized compilation failed");
        fixture.Unchanged(); fixture.NoTemporaryFiles();
    }

    [Fact]
    public void Read_only_changed_file_prevents_all_replacements()
    {
        using var fixture = new Fixture();
        File.SetAttributes(fixture.Paths[1], File.GetAttributes(fixture.Paths[1]) | FileAttributes.ReadOnly);
        Should.Throw<UnauthorizedAccessException>(() => InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, default));
        fixture.Unchanged(); fixture.NoTemporaryFiles();
    }

    [Fact]
    public void Rolls_back_prior_replacements_on_IO_failure()
    {
        using var fixture = new Fixture();
        int replacements = 0;
        Should.Throw<IOException>(() => InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, default, (source, target) =>
        {
            if (++replacements == 2) throw new IOException("injected replacement failure");
            File.Move(source, target, overwrite: true);
        })).Message.ShouldContain("injected replacement failure");
        replacements.ShouldBe(2);
        fixture.Unchanged(); fixture.NoTemporaryFiles();
    }

    [Fact]
    public void Cancellation_during_commit_restores_originals()
    {
        using var fixture = new Fixture();
        using var cancellation = new CancellationTokenSource();
        Should.Throw<OperationCanceledException>(() => InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, cancellation.Token, (source, target) =>
        {
            File.Move(source, target, overwrite: true);
            cancellation.Cancel();
        }));
        fixture.Unchanged(); fixture.NoTemporaryFiles();
    }

    [Fact]
    public void Rollback_preserves_concurrent_edits_and_retains_original_backup()
    {
        using var fixture = new Fixture();
        Should.Throw<IOException>(() => InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, default, (source, target) =>
        {
            File.Move(source, target, overwrite: true);
            if (target == fixture.Paths[0]) File.WriteAllText(target, "// user's concurrent edit");
        })).Message.ShouldContain("rollback was incomplete");
        File.ReadAllText(fixture.Paths[0]).ShouldBe("// user's concurrent edit");
        File.ReadAllBytes(fixture.Paths[1]).ShouldBe(fixture.Before[1]);
        var backup = Directory.GetFiles(fixture.DirectoryPath, ".dotcc-postprocess-*.bak").Single();
        File.ReadAllBytes(backup).ShouldBe(fixture.Before[0]);
        Directory.GetFiles(fixture.DirectoryPath, ".dotcc-postprocess-*.tmp").ShouldBeEmpty();
    }

    [Fact]
    public void Updates_symlink_target_without_replacing_the_link_and_preserves_Unix_mode()
    {
        using var fixture = new Fixture();
        var target = Path.Combine(fixture.DirectoryPath, "Target.cs");
        File.Move(fixture.Paths[0], target);
        try { File.CreateSymbolicLink(fixture.Paths[0], target); }
        catch (UnauthorizedAccessException) { Assert.Skip("Creating symbolic links is not permitted on this host."); }
        UnixFileMode mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead;
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(target, mode);
        InPlaceWriter.Write(fixture.Original, fixture.Rewritten, fixture.Hashes, TestContext.Current.CancellationToken).ShouldBe(2);
        new FileInfo(fixture.Paths[0]).LinkTarget.ShouldNotBeNull();
        File.ReadAllText(target).ShouldNotContain("Cond.B(");
        if (!OperatingSystem.IsWindows()) File.GetUnixFileMode(target).ShouldBe(mode);
        fixture.NoTemporaryFiles();
    }
}
