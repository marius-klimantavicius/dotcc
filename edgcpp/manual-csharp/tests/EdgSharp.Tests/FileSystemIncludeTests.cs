// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Authored direct provider and session integration controls; supervisor executes them.
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using EdgSharp;

internal static partial class Tests
{
    // Every owned filesystem entry is created under this fresh test-only directory.
    private sealed class IncludeTestDirectory : IDisposable
    {
        internal string Root { get; } = Directory.CreateTempSubdirectory("edgsharp-includes-").FullName;
        internal string PathOf(string relative) => Path.Combine(Root, relative);
        internal string Bytes(string relative, byte[] bytes)
        {
            string path = PathOf(relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            return path;
        }
        internal string Text(string relative, string text) => Bytes(relative, new UTF8Encoding(false, true).GetBytes(text));
        internal FileSystemIncludeProvider Provider(int bytes = FileSystemIncludeProvider.DefaultMaxInputBytes,
            int characters = FileSystemIncludeProvider.DefaultMaxInputCharacters)
            => new(Root, [Root], bytes, characters);
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    private static string IncludeFound(IncludeReadResult result)
    {
        string detail = IncludeResultDetail(result.Status, result.CanonicalPath, result.Failure);
        Check(result.Status == IncludeInputStatus.Found, "Expected a successful read. " + detail);
        Check(result.CanonicalPath != null && result.Text != null && result.Failure == null, "Invalid successful read contract. " + detail);
        return result.Text!;
    }
    private static string IncludeResultDetail(IncludeInputStatus status, string? path, IncludeInputFailure? failure)
        => $"Path={path ?? "<null>"}; Status={status}; Failure.Kind={failure?.Kind.ToString() ?? "<none>"}; Failure.Message={failure?.Message ?? "<none>"}";
    private static void IncludeProbeFound(IncludeProbeResult result)
    {
        string detail = IncludeResultDetail(result.Status, result.CanonicalPath, result.Failure);
        Check(result.Status == IncludeInputStatus.Found, "Expected a successful presence probe. " + detail);
        Check(result.CanonicalPath != null && result.Failure == null, "Invalid successful probe contract. " + detail);
    }
    private static void IncludeFailed(IncludeReadResult result, IncludeFailureKind kind)
    {
        string detail = IncludeResultDetail(result.Status, result.CanonicalPath, result.Failure);
        Check(result.Status == IncludeInputStatus.Failure && result.Failure?.Kind == kind,
            "Expected include failure " + kind + ". " + detail);
        Check(result.Text == null && result.Failure!.Message.Length != 0, "Invalid failed read contract. " + detail);
    }

    private static void FileSystemPublicAndHostContracts()
    {
        // These finite value/API controls perform no I/O or native ABI experiments.
        const string path = "/contract/header.h", text = "immutable text";
        IncludeReadResult found = IncludeReadResult.Found(path, text);
        Equal(text, IncludeFound(found)); Same(text, found.Text);
        Equal("", IncludeFound(IncludeReadResult.Found(path, "")));
        IncludeProbeFound(IncludeProbeResult.Found(path));
        IncludeReadResult missing = IncludeReadResult.Missing(path);
        Equal(IncludeInputStatus.Missing, missing.Status); Equal(path, missing.CanonicalPath);
        Check(missing.Text == null && missing.Failure == null);
        IncludeProbeResult probeMissing = IncludeProbeResult.Missing(path);
        Equal(IncludeInputStatus.Missing, probeMissing.Status); Equal(path, probeMissing.CanonicalPath);
        Check(probeMissing.Failure == null);
        foreach (IncludeFailureKind kind in Enum.GetValues<IncludeFailureKind>())
        {
            var failure = new IncludeInputFailure(kind, "explicit failure");
            IncludeReadResult readFailure = IncludeReadResult.Failed(path, failure);
            IncludeFailed(readFailure, kind); Equal(path, readFailure.CanonicalPath); Same(failure, readFailure.Failure);
            IncludeProbeResult probeFailure = IncludeProbeResult.Failed(null, failure);
            Equal(IncludeInputStatus.Failure, probeFailure.Status); Check(probeFailure.CanonicalPath == null);
            Same(failure, probeFailure.Failure);
            var exception = new IncludeInputException(path, failure);
            IOException ioException = exception;
            Same(exception, ioException); Equal(path, exception.CanonicalPath);
            Same(failure, exception.Failure); Equal(failure.Message, exception.Message);
            Check(IncludeReadResult.Failed(null, failure).CanonicalPath == null);
        }
        Throws<ArgumentOutOfRangeException>(() => new IncludeInputFailure((IncludeFailureKind)(-1), "invalid kind"));
        Throws<ArgumentException>(() => new IncludeInputFailure(IncludeFailureKind.Io, ""));
        Throws<ArgumentNullException>(() => new IncludeInputFailure(IncludeFailureKind.Io, null!));
        Throws<ArgumentException>(() => IncludeReadResult.Found("", text));
        Throws<ArgumentException>(() => IncludeReadResult.Missing(""));
        Throws<ArgumentException>(() => IncludeProbeResult.Found(""));
        Throws<ArgumentException>(() => IncludeProbeResult.Missing(null!));
        Throws<ArgumentNullException>(() => IncludeReadResult.Found(path, null!));
        Throws<ArgumentNullException>(() => IncludeReadResult.Failed(path, null!));
        Throws<ArgumentNullException>(() => IncludeProbeResult.Failed(path, null!));
        Throws<ArgumentNullException>(() => new IncludeInputException(path, null!));

        static bool LinuxGlibc(string? version, bool identity = true)
            => FileSystemIncludeProvider.SupportsHostAbi(true, Architecture.X64, 8, true, identity, version);
        foreach (string version in new[] { "2.33", "2.39", "2.40", "2.40.9000" })
            Check(LinuxGlibc(version), "Expected inspected Linux x64 GNU libc ABI version " + version + " to qualify, independently of RID.");
        foreach (string? version in new string?[] { null, "", "2", "2.32", "1.2.5", "3.0", "2.39-musl", "unknown", " 2.39", "2.39 ", "2..39", "2.39\0", new string('2', 32) })
            Check(!LinuxGlibc(version), "Unknown/uninspected libc version unexpectedly qualified: " + version);
        Check(!LinuxGlibc("2.39", identity: false), "A GNU-style version string without actual GNU identity must not qualify musl/unknown libc.");
        Check(!FileSystemIncludeProvider.SupportsHostAbi(false, Architecture.X64, 8, true, true, "2.39"));
        Check(!FileSystemIncludeProvider.SupportsHostAbi(true, Architecture.Arm64, 8, true, true, "2.39"));
        Check(!FileSystemIncludeProvider.SupportsHostAbi(true, Architecture.X86, 4, true, true, "2.39"));
        Check(!FileSystemIncludeProvider.SupportsHostAbi(true, Architecture.X64, 4, true, true, "2.39"));
        Check(!FileSystemIncludeProvider.SupportsHostAbi(true, Architecture.X64, 8, false, true, "2.39"));
    }

    internal static void FileSystemConfiguration()
    {
        FileSystemPublicAndHostContracts();
        using var directory = new IncludeTestDirectory();
        string header = directory.Text("opt.h", "int opt = 7;\n");
        string[] roots = [directory.Root];
        var provider = new FileSystemIncludeProvider(directory.Root + Path.DirectorySeparatorChar + ".", roots, 123, 87);
        roots[0] = directory.PathOf("different");
        Equal(directory.Root, provider.BaseDirectory);
        Equal(directory.Root, provider.AllowedRoots.Single());
        Equal(123, provider.MaxInputBytes); Equal(87, provider.MaxInputCharacters);
        Throws<NotSupportedException>(() => ((IList)provider.AllowedRoots)[0] = "changed");
        Equal("int opt = 7;\n", IncludeFound(provider.Read("./opt.h")));
        Equal(header, provider.Read("sub/../opt.h").CanonicalPath);
        Throws<ArgumentException>(() => new FileSystemIncludeProvider(".", [directory.Root]));
        Throws<ArgumentException>(() => new FileSystemIncludeProvider(directory.Root, ["relative"]));
        Throws<ArgumentException>(() => new FileSystemIncludeProvider(directory.Root, []));
        Throws<ArgumentNullException>(() => new FileSystemIncludeProvider(directory.Root, null!));
        Throws<ArgumentOutOfRangeException>(() => directory.Provider(bytes: 0));
        Throws<ArgumentOutOfRangeException>(() => directory.Provider(characters: 0));
        IncludeFailed(provider.Read("bad\0path"), IncludeFailureKind.InvalidPath);
        IncludeFailed(provider.Read("bad\ud800path"), IncludeFailureKind.InvalidPath);
        IncludeFailed(provider.Read(""), IncludeFailureKind.InvalidPath);

        // Construction snapshots lexical configuration without touching nonexistent roots.
        string absent = directory.PathOf("not-created");
        var uncreated = new FileSystemIncludeProvider(absent, [absent]);
        Equal(absent, uncreated.BaseDirectory); Check(!Directory.Exists(absent));

        // Host I/O is selected only through the explicitly supplied provider.
        using var defaultSession = new CompilationSession();
        Check(!defaultSession.Preprocess("#include \"" + header + "\"\n", directory.PathOf("main.cpp")).IsSuccess);
        var memory = new InMemoryIncludeProvider(new Dictionary<string, string>());
        Check(!memory.TryRead(header, out string? memoryText) && memoryText == null);
    }

    internal static void FileSystemRequestedPaths()
    {
        using var directory = new IncludeTestDirectory();
        string first = directory.Text("first/value.h", "first");
        string second = directory.Text("second/value.h", "second");
        var provider = new FileSystemIncludeProvider(directory.Root,
            [directory.PathOf("first"), directory.PathOf("second")]);
        // The caller explicitly chooses each candidate and its order.
        Equal("second", IncludeFound(provider.Read(second)));
        Equal("first", IncludeFound(provider.Read(first)));
        Equal("first", IncludeFound(provider.Read("first/./value.h")));
        Equal("second", IncludeFound(provider.Read("second/value.h")));
        IncludeFailed(provider.Read("value.h"), IncludeFailureKind.OutsideAllowedRoots);
        IncludeFailed(provider.Read("first-sibling/value.h"), IncludeFailureKind.OutsideAllowedRoots);
        IncludeFailed(provider.Read("first/../../escaped.h"), IncludeFailureKind.OutsideAllowedRoots);
        Equal(IncludeInputStatus.Missing, provider.Read("first/missing.h").Status);

        var unrestrictedLexically = new FileSystemIncludeProvider(directory.Root, [directory.Root]);
        Equal(IncludeInputStatus.Missing, unrestrictedLexically.Read("value.h").Status);
        // A directory candidate is a miss so a preprocessor can try later roots.
        Directory.CreateDirectory(directory.PathOf("first/directory.h"));
        Equal(IncludeInputStatus.Missing, provider.Probe("first/directory.h").Status);
        Equal(IncludeInputStatus.Missing, provider.Read("first/directory.h").Status);

        string root = Path.GetPathRoot(directory.Root)!;
        var rooted = new FileSystemIncludeProvider(root, [root]);
        Equal(root, rooted.BaseDirectory); Equal(root, rooted.AllowedRoots.Single());
        Equal("first", IncludeFound(rooted.Read(first)));
        string fromRoot = Path.GetRelativePath(root, first);
        Equal(first, rooted.Read(fromRoot).CanonicalPath);
        Equal(IncludeInputStatus.Missing, rooted.Probe(root).Status);
    }

    internal static void FileSystemEncodings()
    {
        using var directory = new IncludeTestDirectory();
        var provider = directory.Provider();
        const string text = "// café \U0001f642\r\nint value = 7;\n";
        Encoding[] encodings = [new UTF8Encoding(false, true), new UTF8Encoding(true, true),
            new UnicodeEncoding(false, true, true), new UnicodeEncoding(true, true, true)];
        for (int index = 0; index < encodings.Length; ++index)
        {
            Encoding encoding = encodings[index];
            string path = directory.Bytes($"encoding-{index}.h", [.. encoding.GetPreamble(), .. encoding.GetBytes(text)]);
            IncludeReadResult read = provider.Read(path);
            Equal(text, IncludeFound(read)); Equal(path, read.CanonicalPath);
            Check(provider.TryRead(path, out string? compatibility)); Equal(text, compatibility);
        }
        foreach (byte[] empty in new byte[][] { [], [0xef, 0xbb, 0xbf], [0xff, 0xfe], [0xfe, 0xff] })
            Equal("", IncludeFound(provider.Read(directory.Bytes("empty.h", empty))));
        // Strip only the selection BOM, preserve embedded/repeated U+FEFF and newlines.
        Equal("\ufeffx\n", IncludeFound(provider.Read(directory.Bytes("twice.h", [0xef, 0xbb, 0xbf, 0xef, 0xbb, 0xbf, 0x78, 0x0a]))));
        // No heuristic: BOM-less UTF-16 bytes that are legal UTF-8 decode as UTF-8.
        Equal("A\0", IncludeFound(provider.Read(directory.Bytes("no-bom.h", [0x41, 0x00]))));
    }

    internal static void FileSystemFailures()
    {
        using var directory = new IncludeTestDirectory();
        var provider = directory.Provider();
        IncludeReadResult missing = provider.Read("missing.h");
        Equal(IncludeInputStatus.Missing, missing.Status);
        Equal(directory.PathOf("missing.h"), missing.CanonicalPath);
        Check(missing.Failure == null && missing.Text == null);
        Check(!provider.TryRead("missing.h", out string? absent) && absent == null);
        byte[][] malformed =
        [
            [0xc0, 0xaf], [0xed, 0xa0, 0x80], [0xf4, 0x90, 0x80, 0x80],
            [0xef, 0xbb], [0xe2, 0x82], [0xff, 0xfe, 0x41],
            [0xff, 0xfe, 0x00, 0xd8], [0xfe, 0xff, 0xdc, 0x00]
        ];
        for (int index = 0; index < malformed.Length; ++index)
        {
            string path = directory.Bytes($"malformed-{index}.h", malformed[index]);
            IncludeProbeFound(provider.Probe(path));
            IncludeFailed(provider.Read(path), IncludeFailureKind.InvalidEncoding);
            try { provider.TryRead(path, out _); throw new Exception("Malformed input must throw a typed compatibility failure."); }
            catch (IncludeInputException error)
            {
                Equal(IncludeFailureKind.InvalidEncoding, error.Failure.Kind); Equal(path, error.CanonicalPath);
            }
        }
        foreach (byte[] unsupported in new byte[][] { [0xff, 0xfe, 0x00, 0x00, 0x41, 0x00, 0x00, 0x00],
            [0x00, 0x00, 0xfe, 0xff], [0x2b, 0x2f, 0x76, 0x38, 0x2d] })
            IncludeFailed(provider.Read(directory.Bytes("unsupported.h", unsupported)), IncludeFailureKind.UnsupportedEncoding);
    }

    internal static void FileSystemAccessFailures()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException("This permission regression requires Linux; other hosts are unverified.");
        using var directory = new IncludeTestDirectory();
        string denied = directory.Text("denied.h", "int hidden;\n");
        string deniedRoot = directory.PathOf("denied-directory");
        Directory.CreateDirectory(deniedRoot);
        string inner = directory.Text("denied-directory/inner.h", "int inner;\n");
        var provider = directory.Provider();
        try
        {
            File.SetUnixFileMode(denied, UnixFileMode.UserRead);
            Equal("int hidden;\n", IncludeFound(provider.Read(denied))); // No write permission is required.
            File.SetUnixFileMode(denied, UnixFileMode.None);
            // Presence does not require opening an existing, unreadable file.
            IncludeProbeFound(provider.Probe(denied));
            IncludeFailed(provider.Read(denied), IncludeFailureKind.Access);
            File.SetUnixFileMode(deniedRoot, UnixFileMode.None);
            IncludeProbeResult probe = provider.Probe(inner);
            Equal(IncludeInputStatus.Failure, probe.Status); Equal(IncludeFailureKind.Access, probe.Failure!.Kind);
            IncludeFailed(provider.Read(inner), IncludeFailureKind.Access);
        }
        finally
        {
            File.SetUnixFileMode(denied, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            File.SetUnixFileMode(deniedRoot, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
        // Running as a permission-bypassing account must fail these assertions, never skip/pass.
    }

    internal static void FileSystemLimits()
    {
        using var directory = new IncludeTestDirectory();
        string path = directory.Text("bounded.h", "1234");
        var exact = directory.Provider(bytes: 4, characters: 4);
        Equal("1234", IncludeFound(exact.Read(path)));
        var bytes = directory.Provider(bytes: 3);
        IncludeProbeFound(bytes.Probe(path));
        IncludeFailed(bytes.Read(path), IncludeFailureKind.LimitExceeded);
        IncludeFailed(directory.Provider(characters: 3).Read(path), IncludeFailureKind.LimitExceeded);
        string scalar = directory.Text("scalar.h", "\U0001f642");
        Equal("\U0001f642", IncludeFound(directory.Provider(bytes: 4, characters: 2).Read(scalar)));
        IncludeFailed(directory.Provider(characters: 1).Read(scalar), IncludeFailureKind.LimitExceeded);
        string bom = directory.Bytes("bom.h", [0xef, 0xbb, 0xbf, 0x78]);
        IncludeFailed(directory.Provider(bytes: 3).Read(bom), IncludeFailureKind.LimitExceeded);
        Equal("x", IncludeFound(directory.Provider(bytes: 4, characters: 1).Read(bom)));
        // A sparse oversized input must be rejected from length before allocating its contents.
        string sparse = directory.PathOf("oversized.h");
        using (var stream = new FileStream(sparse, FileMode.CreateNew)) stream.SetLength(1024 * 1024);
        IncludeFailed(directory.Provider(bytes: 32).Read(sparse), IncludeFailureKind.LimitExceeded);
    }

    internal static void FileSystemConcurrency()
    {
        using var directory = new IncludeTestDirectory();
        directory.Text("first/value.h", "first"); directory.Text("second/value.h", "second");
        var first = new FileSystemIncludeProvider(directory.PathOf("first"), [directory.Root]);
        var second = new FileSystemIncludeProvider(directory.PathOf("second"), [directory.Root]);
        Task[] reads = Enumerable.Range(0, 12).Select(index => Task.Run(() =>
        {
            FileSystemIncludeProvider provider = index % 2 == 0 ? first : second;
            string expected = index % 2 == 0 ? "first" : "second";
            for (int iteration = 0; iteration < 16; ++iteration)
            {
                IncludeProbeFound(provider.Probe("value.h"));
                Equal(expected, IncludeFound(provider.Read("value.h")));
            }
        })).ToArray();
        Task.WaitAll(reads);
        IncludeReadResult retained = first.Read("value.h");
        directory.Text("first/value.h", "changed");
        Equal("first", IncludeFound(retained)); Equal("changed", IncludeFound(first.Read("value.h")));
        Equal(IncludeInputStatus.Missing, first.Probe("later.h").Status);
        directory.Text("first/later.h", "later");
        Equal("later", IncludeFound(first.Read("later.h")));
    }

    private sealed class ObservedFileSystemIncludes(FileSystemIncludeProvider provider) : IStructuredIncludeProvider
    {
        internal List<string> Reads { get; } = [];
        internal List<string> Probes { get; } = [];
        internal Action<string, IncludeReadResult>? AfterRead { get; set; }
        internal bool ForbidIo { get; set; }
        public IncludeReadResult Read(string path)
        {
            Check(!ForbidIo, "A retained query must not invoke the provider.");
            Reads.Add(path);
            IncludeReadResult result = provider.Read(path);
            AfterRead?.Invoke(path, result);
            return result;
        }
        public IncludeProbeResult Probe(string path)
        {
            Check(!ForbidIo, "A retained query must not invoke the provider.");
            Probes.Add(path); return provider.Probe(path);
        }
        public bool TryRead(string path, out string? text)
        {
            text = null;
            throw new Exception("The preprocessor must select structured Probe/Read for this provider.");
        }
    }

    internal static void FileSystemSessionIncludes()
    {
        using var directory = new IncludeTestDirectory();
        string physical = directory.Text("physical/value.h", "physical\n");
        string quote = directory.Text("quote/quote.h", "quote\n");
        directory.Text("quote/value.h", "wrong_quote\n");
        string system = directory.Text("system/value.h", "system\n");
        var observed = new ObservedFileSystemIncludes(directory.Provider());
        var options = new PreprocessingOptions(observed, [directory.PathOf("quote")], [directory.PathOf("system")]);
        PreprocessingResult result = Preprocessed("#line 90 \"virtual/place.cpp\"\n#include \"value.h\"\n#include \"quote.h\"\n#include <value.h>\n",
            options, directory.PathOf("physical/main.cpp"));
        Equal("physical quote system", PpText(result));
        Check(result.PhysicalSources.Select(source => source.Name).SequenceEqual(new[]
            { directory.PathOf("physical/main.cpp"), physical, quote, system }));
        Check(observed.Reads.SequenceEqual(new[] { physical, directory.PathOf("physical/quote.h"), quote, system }));
        Equal(3, result.Includes.Count); Equal(0, observed.Probes.Count);

        Directory.CreateDirectory(directory.PathOf("first/h.h"));
        string later = directory.Text("second/h.h", "constexpr int fallback = 2;\n");
        var fallbackProvider = new ObservedFileSystemIncludes(directory.Provider());
        var fallbackOptions = new PreprocessingOptions(fallbackProvider,
            systemDirectories: [directory.PathOf("first"), directory.PathOf("second")]);
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = fallbackOptions });
        AnalysisResult fallback = session.Analyze("#include <h.h>\nstatic_assert(fallback == 2);\n", directory.PathOf("main.cpp"));
        Check(fallback.IsSuccess, string.Join("; ", fallback.Diagnostics.Select(d => d.Message)));
        Equal(new BigInteger(2), Variable(fallback, "fallback").Initializer!.ConstantValue!.Value);
        Equal(later, fallback.PhysicalSources[1].Name);
        Check(fallbackProvider.Reads.SequenceEqual(new[] { directory.PathOf("first/h.h"), later }));
        PreprocessingResult presence = session.Preprocess("#if __has_include(<h.h>)\nyes\n#endif\n", directory.PathOf("main.cpp"));
        Equal("yes", PpText(presence)); Equal(1, presence.PhysicalSources.Count); Equal(0, presence.Includes.Count);
    }

    internal static void FileSystemSessionProbes()
    {
        using var directory = new IncludeTestDirectory();
        directory.Bytes("malformed.h", [0xc0, 0xaf]);
        string active = directory.Text("active.h", "#pragma once\n#define ACTIVE 7\n#line 80 \"virtual-header.h\"\nint hidden = ACTIVE;\n");
        var observed = new ObservedFileSystemIncludes(directory.Provider());
        var options = new PreprocessingOptions(observed);
        PreprocessingResult probe = Preprocessed("#if __has_include(\"malformed.h\") && __has_include(\"active.h\")\nprobe\n#endif\n#ifdef ACTIVE\nwrong\n#endif\n__LINE__",
            options, directory.PathOf("main.cpp"));
        Equal("probe 7", PpText(probe)); Equal(1, probe.PhysicalSources.Count); Equal(0, probe.Includes.Count);
        Check(!probe.Macros.ContainsKey("ACTIVE")); Equal(0, observed.Reads.Count); Equal(2, observed.Probes.Count);
        PreprocessingResult included = Preprocessed("#if __has_include(\"active.h\") && __has_include(\"active.h\")\n#include \"active.h\"\n#include \"active.h\"\n#endif\nACTIVE __LINE__",
            options, directory.PathOf("main.cpp"));
        Equal("int hidden = 7 ; 7 5", PpText(included)); Equal(2, included.PhysicalSources.Count); Equal(1, included.Includes.Count);
        Equal(1, observed.Reads.Count); Equal(active, observed.Reads.Single());
        Equal(2, observed.Probes.Count(path => path == active)); // One in each separate analysis.
        Token declaration = included.Tokens.Tokens.First(token => token.Spelling == "hidden");
        Equal("virtual-header.h", declaration.Origin.Presumed.FileName);
        Equal(80, declaration.Origin.Presumed.Line);
        Same(included.Includes.Single(), declaration.Origin.Include);
        PreprocessingResult missing = Preprocessed("#if __has_include(\"missing.h\") || __has_include(\"missing.h\")\nwrong\n#endif\nright",
            options, directory.PathOf("main.cpp"));
        Equal("right", PpText(missing));
        Equal(1, observed.Probes.Count(path => path == directory.PathOf("missing.h")));
    }

    internal static void FileSystemSessionDiagnostics()
    {
        using var directory = new IncludeTestDirectory();
        string malformed = directory.Bytes("first/h.h", [0xc0, 0xaf]);
        directory.Text("second/h.h", "int shouldNotBeRead;\n");
        var observed = new ObservedFileSystemIncludes(directory.Provider());
        var options = new PreprocessingOptions(observed,
            systemDirectories: [directory.PathOf("first"), directory.PathOf("second")]);
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        AnalysisResult invalid = session.Analyze("#include <h.h>\n", directory.PathOf("main.cpp"));
        Check(!invalid.IsSuccess);
        Check(invalid.Diagnostics.Any(d => d.Code == "PP063" && d.Class == DiagnosticClass.InvalidSource));
        Check(!invalid.Diagnostics.Any(d => d.Code == "PP028"));
        Check(observed.Reads.SequenceEqual(new[] { malformed }));
        Check(!invalid.Declarations.Any(d => d.Name == "shouldNotBeRead"));
        Check(session.Preprocess("#include <missing.h>\n", directory.PathOf("main.cpp")).Diagnostics.Any(d => d.Code == "PP028"));

        string unsupported = directory.Bytes("first/unsupported.h", [0x00, 0x00, 0xfe, 0xff]);
        PreprocessingResult unsupportedResult = session.Preprocess("#include <unsupported.h>\n", directory.PathOf("main.cpp"));
        Check(!unsupportedResult.IsSuccess && unsupportedResult.Diagnostics.Any(d => d.Class == DiagnosticClass.UnsupportedFeature));
        Equal(IncludeFailureKind.UnsupportedEncoding, directory.Provider().Read(unsupported).Failure!.Kind);
        directory.Text("large.h", "12345");
        using var bounded = new CompilationSession(new AnalysisOptions { Preprocessing = new(directory.Provider(bytes: 4)) });
        PreprocessingResult limit = bounded.Preprocess("#include \"large.h\"\n", directory.PathOf("main.cpp"));
        Check(!limit.IsSuccess && limit.Diagnostics.Any(d => d.Code == "PP063" && d.Class == DiagnosticClass.ResourceLimit));
        // Presence of an oversized header does not read it or consume the read budget.
        Equal("yes", PpText(bounded.Preprocess("#if __has_include(\"large.h\")\nyes\n#endif\n", directory.PathOf("main.cpp"))));
    }

    internal static void FileSystemSessionCachingQueries()
    {
        using var directory = new IncludeTestDirectory();
        string changing = directory.Text("changing.h", "1\n");
        var observed = new ObservedFileSystemIncludes(directory.Provider());
        bool changed = false;
        observed.AfterRead = (path, result) =>
        {
            if (path == changing && result.Status == IncludeInputStatus.Found && !changed)
            { changed = true; directory.Text("changing.h", "2\n"); }
        };
        var options = new PreprocessingOptions(observed);
        using var session = new CompilationSession(new AnalysisOptions { Preprocessing = options });
        Equal("1 1", PpText(session.Preprocess("#include \"changing.h\"\n#include \"changing.h\"\n", directory.PathOf("main.cpp"))));
        Equal(1, observed.Reads.Count(path => path == changing));
        Equal("2", PpText(session.Preprocess("#include \"changing.h\"\n", directory.PathOf("main.cpp"))));
        Equal(2, observed.Reads.Count(path => path == changing));

        string retainedHeader = directory.Text("retained.h", "#define VALUE 7\ntemplate<class T> constexpr T id(T x){return x;}\n");
        AnalysisResult result = session.Analyze("#include \"retained.h\"\nint answer=VALUE; static_assert(id(VALUE)==7);\n", directory.PathOf("main.cpp"));
        Check(result.IsSuccess, string.Join("; ", result.Diagnostics.Select(d => d.Message)));
        int reads = observed.Reads.Count, probes = observed.Probes.Count;
        session.Dispose();
        observed.ForbidIo = true;
        directory.Text("retained.h", "#error changed after completed analysis\n");
        Equal(new BigInteger(7), Variable(result, "answer").Initializer!.ConstantValue!.Value);
        Equal(1, Template(result, "id").Instances.Count);
        Equal(ConstantEvaluationStatus.Success, result.StaticAssertions.Single().Evaluation.Status);
        Equal("#define VALUE 7\ntemplate<class T> constexpr T id(T x){return x;}\n", result.PhysicalSources.Single(file => file.Name == retainedHeader).Text);
        Check(result.Tokens.Tokens.Any(token => token.Origin.ExpansionNotes.Count != 0));
        Equal(reads, observed.Reads.Count); Equal(probes, observed.Probes.Count);
    }

    // Primary must wire this method to --filesystem-special-child <owned-root>
    // BEFORE the standard runner's usage/argument rejection. The parent below supplies
    // a hard subprocess deadline; an in-process Task timeout cannot stop a blocking open.
    internal static void FileSystemSpecialFileChild(string root)
    {
        Check(OperatingSystem.IsLinux() && RuntimeInformation.ProcessArchitecture == Architecture.X64,
            "The filesystem special-file child requires Linux x64; other hosts are unverified.");
        var provider = new FileSystemIncludeProvider(root, [root]);
        string fifo = Path.Combine(root, "fifo.h");
        Check(CreateIncludeFifo(fifo, 0x180) == 0, "Test FIFO creation failed with errno " + Marshal.GetLastPInvokeError());
        for (int index = 0; index < 32; ++index)
        {
            IncludeProbeResult probe = provider.Probe(fifo);
            Equal(IncludeInputStatus.Failure, probe.Status); Equal(IncludeFailureKind.InvalidPath, probe.Failure!.Kind);
            IncludeFailed(provider.Read(fifo), IncludeFailureKind.InvalidPath);
        }
        string linked = Path.Combine(root, "linked-fifo.h");
        File.CreateSymbolicLink(linked, fifo);
        IncludeFailed(provider.Read(linked), IncludeFailureKind.InvalidPath);
        string socketPath = Path.Combine(root, "socket.h");
        using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Bind(new UnixDomainSocketEndPoint(socketPath));
        Equal(IncludeFailureKind.InvalidPath, provider.Probe(socketPath).Failure!.Kind);
        IncludeReadResult socketRead = provider.Read(socketPath);
        Equal(IncludeInputStatus.Failure, socketRead.Status);
        Check(socketRead.Failure!.Kind is IncludeFailureKind.Io or IncludeFailureKind.InvalidPath);
        string regular = Path.Combine(root, "regular.h");
        File.WriteAllBytes(regular, [0x6f, 0x6b]);
        string alias = Path.Combine(root, "alias.h");
        File.CreateSymbolicLink(alias, regular);
        Equal("ok", IncludeFound(provider.Read(alias))); Equal(alias, provider.Read(alias).CanonicalPath);
        string loop = Path.Combine(root, "loop.h");
        File.CreateSymbolicLink(loop, loop);
        IncludeFailed(provider.Read(loop), IncludeFailureKind.InvalidPath);
    }

    [DllImport("libc", EntryPoint = "mkfifo", SetLastError = true)]
    private static extern int CreateIncludeFifo([MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint mode);

    internal static void FileSystemSpecialFilesBounded()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This subprocess regression requires Linux x64; other hosts are unverified.");
        using var directory = new IncludeTestDirectory();
        string executable = Environment.ProcessPath ?? throw new Exception("The bounded regression needs the current executable path.");
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        if (Path.GetFileNameWithoutExtension(executable) == "dotnet") start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "EdgSharp.Tests.dll"));
        start.ArgumentList.Add("--filesystem-special-child"); start.ArgumentList.Add(directory.Root);
        using Process child = Process.Start(start) ?? throw new Exception("Could not start filesystem regression child.");
        Task<string> stdout = child.StandardOutput.ReadToEndAsync(), stderr = child.StandardError.ReadToEndAsync();
        if (!child.WaitForExit(10000))
        {
            child.Kill(entireProcessTree: true);
            Check(child.WaitForExit(5000), "Timed-out filesystem child did not terminate after kill.");
            throw new Exception("Filesystem special-file child exceeded 10 seconds; blocking open/resource regression.");
        }
        Task.WaitAll(stdout, stderr);
        Check(child.ExitCode == 0, "Filesystem special-file child failed: " + stdout.Result + stderr.Result);
    }
}
