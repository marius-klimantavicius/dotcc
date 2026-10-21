// SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
// Handwritten managed input adaptation of EDG lexical.c's search/open/resolve
// contracts and host_envir.c's BOM boundary. Narrow Linux filesystem interop
// validates descriptors; no native frontend, compiler process or generated port.
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace EdgSharp;

/// <summary>Optional input contract separating presence from reading/decoding.</summary>
public interface IStructuredIncludeProvider : IIncludeProvider
{
    /// <summary>Checks one explicitly searched path without reading or decoding its contents.</summary>
    IncludeProbeResult Probe(string path);
    /// <summary>Reads one explicitly searched path; failure is distinct from absence.</summary>
    IncludeReadResult Read(string path);
}

public enum IncludeInputStatus { Found, Missing, Failure }

public enum IncludeFailureKind
{
    InvalidEncoding, UnsupportedEncoding, Access, Io, InvalidPath, OutsideAllowedRoots, LimitExceeded, UnsupportedHost
}

/// <summary>A retained value, without platform exception or source/session ownership.</summary>
public sealed class IncludeInputFailure
{
    public IncludeFailureKind Kind { get; }
    public string Message { get; }
    public IncludeInputFailure(IncludeFailureKind kind, string message)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        ArgumentException.ThrowIfNullOrEmpty(message);
        (Kind, Message) = (kind, message);
    }
}

public sealed class IncludeProbeResult
{
    public IncludeInputStatus Status { get; }
    /// <summary>Normalized absolute host path for this provider; null if normalization failed.</summary>
    public string? CanonicalPath { get; }
    public IncludeInputFailure? Failure { get; }
    private IncludeProbeResult(IncludeInputStatus status, string? path, IncludeInputFailure? failure)
        => (Status, CanonicalPath, Failure) = (status, path, failure);
    public static IncludeProbeResult Found(string path) => new(IncludeInputStatus.Found, RequiredPath(path), null);
    public static IncludeProbeResult Missing(string path) => new(IncludeInputStatus.Missing, RequiredPath(path), null);
    public static IncludeProbeResult Failed(string? path, IncludeInputFailure failure)
        => new(IncludeInputStatus.Failure, path, failure ?? throw new ArgumentNullException(nameof(failure)));
    private static string RequiredPath(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return path;
    }
}

public sealed class IncludeReadResult
{
    public IncludeInputStatus Status { get; }
    public string? CanonicalPath { get; }
    /// <summary>Immutable decoded contents, non-null only on Found, including an empty file.</summary>
    public string? Text { get; }
    public IncludeInputFailure? Failure { get; }
    private IncludeReadResult(IncludeInputStatus status, string? path, string? text, IncludeInputFailure? failure)
        => (Status, CanonicalPath, Text, Failure) = (status, path, text, failure);
    public static IncludeReadResult Found(string path, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(text);
        return new(IncludeInputStatus.Found, path, text, null);
    }
    public static IncludeReadResult Missing(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        return new(IncludeInputStatus.Missing, path, null, null);
    }
    public static IncludeReadResult Failed(string? path, IncludeInputFailure failure)
        => new(IncludeInputStatus.Failure, path, null, failure ?? throw new ArgumentNullException(nameof(failure)));
}

/// <summary>Compatibility TryRead throws this on failure, and returns false only on Missing.</summary>
public sealed class IncludeInputException : IOException
{
    public string? CanonicalPath { get; }
    public IncludeInputFailure Failure { get; }
    public IncludeInputException(string? canonicalPath, IncludeInputFailure failure)
        : base((failure ?? throw new ArgumentNullException(nameof(failure))).Message)
        => (CanonicalPath, Failure) = (canonicalPath, failure);
}

/// <summary>
/// Opt-in Linux x64 host filesystem input. Relative requests resolve against the required absolute
/// base; roots are explicit absolute lexical path prefixes, not include-search directories.
/// Configuration is immutable and methods can run concurrently. There is no content,
/// probe, source, or TU cache: the caller owns stable results within each analysis.
/// </summary>
/// <remarks>
/// Paths use host BCL syntax, normalize dot segments and trailing separators, and preserve
/// the Unix root '/'. Symlinks/hardlinks are followed but aliases are not unified. Allowed
/// roots constrain lexical names, not symlink targets; this is not a security sandbox.
/// Base/roots are never obtained from CWD or environment variables. Linux x64 is the
/// implemented host is Linux x86-64 LP64, little endian, with GNU libc 2.33+ in
/// major version 2. Portable and distribution RIDs do not determine ABI support.
/// Musl, unknown libc ABIs, Windows/macOS and other architectures return UnsupportedHost.
/// Windows rooted but nonabsolute paths (including drive-relative paths) are rejected.
/// Reads accept UTF-8 without BOM or with UTF-8 BOM, and BOM-selected UTF-16 LE/BE.
/// One leading BOM is removed. Malformed sequences throw no replacement characters into
/// the returned text. UTF-32/UTF-7 BOMs are explicitly unsupported; BOM-less input is
/// always UTF-8, never a locale code page or heuristic UTF-16. Decoding does not establish
/// C++ Unicode identifier, literal, UCN, or execution-character-set support.
/// Byte/character budgets bound buffers, not wall-clock time or network filesystem behavior.
/// </remarks>
public sealed class FileSystemIncludeProvider : IStructuredIncludeProvider
{
    public const int DefaultMaxInputBytes = 16 * 1024 * 1024;
    public const int DefaultMaxInputCharacters = 16 * 1024 * 1024;
    public string BaseDirectory { get; }
    public ReadOnlyCollection<string> AllowedRoots { get; }
    public int MaxInputBytes { get; }
    /// <summary>Maximum decoded UTF-16 code units, not Unicode scalar values.</summary>
    public int MaxInputCharacters { get; }

    public FileSystemIncludeProvider(string baseDirectory, IEnumerable<string> allowedRoots,
        int maxInputBytes = DefaultMaxInputBytes, int maxInputCharacters = DefaultMaxInputCharacters)
    {
        if (maxInputBytes <= 0 || maxInputBytes > Array.MaxLength)
            throw new ArgumentOutOfRangeException(nameof(maxInputBytes));
        if (maxInputCharacters <= 0 || maxInputCharacters > Array.MaxLength)
            throw new ArgumentOutOfRangeException(nameof(maxInputCharacters));
        BaseDirectory = AbsoluteConfigurationPath(baseDirectory, nameof(baseDirectory));
        ArgumentNullException.ThrowIfNull(allowedRoots);
        string[] roots = allowedRoots.Select(root => AbsoluteConfigurationPath(root, nameof(allowedRoots))).ToArray();
        if (roots.Length == 0) throw new ArgumentException("At least one explicit allowed root is required.", nameof(allowedRoots));
        AllowedRoots = Array.AsReadOnly(roots);
        (MaxInputBytes, MaxInputCharacters) = (maxInputBytes, maxInputCharacters);
    }

    private static string AbsoluteConfigurationPath(string path, string parameter)
    {
        ArgumentException.ThrowIfNullOrEmpty(path, parameter);
        if (path.IndexOf('\0') >= 0 || !Path.IsPathFullyQualified(path))
            throw new ArgumentException("An explicit fully qualified host path is required.", parameter);
        _ = new UTF8Encoding(false, true).GetByteCount(path); // Never marshal a replaced surrogate into a different filename.
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    private IncludeProbeResult Normalize(string path)
    {
        string? canonical = null;
        try
        {
            if (string.IsNullOrEmpty(path) || path.IndexOf('\0') >= 0 ||
                (Path.IsPathRooted(path) && !Path.IsPathFullyQualified(path)))
                return ProbeFailure(null, IncludeFailureKind.InvalidPath, "An empty, NUL-containing, or drive-relative input path is invalid.");
            _ = new UTF8Encoding(false, true).GetByteCount(path);
            canonical = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path, BaseDirectory));
            StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (string root in AllowedRoots)
            {
                if (canonical.Equals(root, comparison)) return IncludeProbeResult.Found(canonical);
                // A root '/' already ends in its separator; appending another breaks prefix matching.
                string prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
                if (canonical.StartsWith(prefix, comparison)) return IncludeProbeResult.Found(canonical);
            }
            return ProbeFailure(canonical, IncludeFailureKind.OutsideAllowedRoots, "Input path is outside the explicitly allowed lexical roots.");
        }
        catch (Exception error) when (IsInputException(error)) { return FromException(canonical, error); }
    }

    // Pure ABI predicate, also exercised with unsupported facts by the contract tests.
    // A distro-specific RID is neither evidence for nor against the libc ABI.
    internal static bool SupportsHostAbi(bool linux, Architecture architecture, int pointerSize,
        bool littleEndian, bool hasGnuLibcIdentity, string? glibcVersion)
    {
        if (!linux || architecture != Architecture.X64 || pointerSize != 8 || !littleEndian || !hasGnuLibcIdentity ||
            glibcVersion is null || glibcVersion.Length == 0 || glibcVersion.Length >= 32) return false;
        foreach (char value in glibcVersion)
            if ((value < '0' || value > '9') && value != '.') return false;
        // This binding uses stat/fstat exports introduced in GLIBC_2.33. A future
        // libc major ABI must be inspected rather than assumed layout-compatible.
        return Version.TryParse(glibcVersion, out Version? version) && version.Major == 2 && version.Minor >= 33;
    }

    private static IncludeInputFailure? HostFailure()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64 ||
            IntPtr.Size != 8 || !BitConverter.IsLittleEndian)
            return UnsupportedHostFailure("The process is not Linux x86-64 LP64 little endian.");
        try
        {
            // Inspect the running process's symbol namespace before loading a named
            // libc. An installed glibc compatibility library alone is not evidence
            // that a musl/unknown process has the supported ABI. This pseudo-handle
            // is borrowed and must not be passed to NativeLibrary.Free.
            IntPtr process = NativeLibrary.GetMainProgramHandle();
            if (!NativeLibrary.TryGetExport(process, "gnu_get_libc_version", out _))
                return UnsupportedHostFailure("The running process has no GNU libc identity export.");
            IntPtr nativeVersion = LinuxFiles.GlibcVersion();
            if (nativeVersion == IntPtr.Zero) return UnsupportedHostFailure("GNU libc returned no ABI version.");
            // GNU libc owns a static ASCII version string. Bound inspection rather
            // than using an unbounded null-terminated string marshaling operation.
            Span<char> characters = stackalloc char[32];
            for (int index = 0; index < characters.Length; ++index)
            {
                byte value = Marshal.ReadByte(nativeVersion, index);
                if (value == 0)
                {
                    string version = new(characters[..index]);
                    return SupportsHostAbi(true, Architecture.X64, 8, true, true, version) ? null
                        : UnsupportedHostFailure("Unrecognized or unsupported GNU libc version '" + version + "'.");
                }
                if ((value < '0' || value > '9') && value != '.')
                    return UnsupportedHostFailure("GNU libc returned an unrecognized ABI version format.");
                characters[index] = (char)value;
            }
            return UnsupportedHostFailure("GNU libc ABI version exceeds the 31-character supported bound.");
        }
        catch (Exception error) when (error is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException
            or NotSupportedException or System.ComponentModel.Win32Exception)
        {
            return UnsupportedHostFailure("GNU libc ABI identification is unavailable: " + error.Message);
        }
    }

    private static IncludeInputFailure UnsupportedHostFailure(string detail)
        => new(IncludeFailureKind.UnsupportedHost,
            "Filesystem includes require Linux x86-64 LP64 little endian with GNU libc major 2, version 2.33 or later. " + detail);
    private static IncludeProbeResult UnsupportedHost(string? path)
        => IncludeProbeResult.Failed(path, UnsupportedHostFailure("The required filesystem library or entry point is unavailable."));

    /// <summary>
    /// Checks metadata only. Found says a regular file exists; it does not
    /// promise read access, supported encoding, size, or unchanged later host contents.
    /// </summary>
    public IncludeProbeResult Probe(string path)
    {
        IncludeProbeResult normalized = Normalize(path);
        if (normalized.Status != IncludeInputStatus.Found) return normalized;
        string canonical = normalized.CanonicalPath!;
        IncludeInputFailure? hostFailure = HostFailure();
        if (hostFailure != null) return IncludeProbeResult.Failed(canonical, hostFailure);
        try
        {
            if (LinuxFiles.StatPath(canonical, out LinuxFiles.FileStatus status) != 0)
                return FromErrno(canonical, Marshal.GetLastPInvokeError(), "stat");
            // EDG treats a directory candidate as a search miss, permitting later roots.
            if (status.Kind == LinuxFiles.Directory) return IncludeProbeResult.Missing(canonical);
            if (status.Kind != LinuxFiles.Regular)
                return ProbeFailure(canonical, IncludeFailureKind.InvalidPath, "Include input must be a regular file; FIFO, socket and device inputs are unsupported.");
            return normalized;
        }
        catch (Exception error) when (IsInputException(error)) { return FromException(canonical, error); }
    }

    public IncludeReadResult Read(string path)
    {
        IncludeProbeResult normalized = Normalize(path);
        if (normalized.Status == IncludeInputStatus.Failure) return IncludeReadResult.Failed(normalized.CanonicalPath, normalized.Failure!);
        string canonical = normalized.CanonicalPath!;
        IncludeInputFailure? hostFailure = HostFailure();
        if (hostFailure != null) return IncludeReadResult.Failed(canonical, hostFailure);
        try
        {
            // The open itself is nonblocking. A FIFO substituted during path resolution
            // cannot block before descriptor validation. No pathname is reopened later.
            int descriptor = LinuxFiles.Open(canonical, LinuxFiles.NonBlocking | LinuxFiles.CloseOnExec);
            if (descriptor < 0) return ReadResult(FromErrno(canonical, Marshal.GetLastPInvokeError(), "open"));
            using var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
            if (LinuxFiles.StatHandle(handle, out LinuxFiles.FileStatus status) != 0)
                return ReadResult(FromErrno(canonical, Marshal.GetLastPInvokeError(), "fstat"));
            if (status.Kind == LinuxFiles.Directory) return IncludeReadResult.Missing(canonical);
            if (status.Kind != LinuxFiles.Regular)
                return ReadFailure(canonical, IncludeFailureKind.InvalidPath, "Include input must be a regular file; FIFO, socket and device inputs are unsupported.");
            long length = status.Length;
            if (length < 0) return ReadFailure(canonical, IncludeFailureKind.Io, "Include input reported a negative length.");
            if (length > MaxInputBytes)
                return ReadFailure(canonical, IncludeFailureKind.LimitExceeded, "Include input exceeds the configured byte limit.");
            // Both using scopes can Dispose the same SafeHandle; it closes the descriptor
            // exactly once, including stream-construction failure and all early returns.
            using var stream = new FileStream(handle, FileAccess.Read, bufferSize: 1);
            // Validate the reported length before allocation. No ReadAllBytes/ReadToEnd,
            // unbounded growth, or allocation based on unchecked host metadata.
            byte[] bytes = new byte[(int)length];
            int count = 0;
            while (count < bytes.Length)
            {
                int read = stream.Read(bytes, count, bytes.Length - count);
                if (read == 0) break;
                count += read;
            }
            if (count == bytes.Length && stream.ReadByte() != -1)
                return ReadFailure(canonical, count == MaxInputBytes ? IncludeFailureKind.LimitExceeded : IncludeFailureKind.Io,
                    "Include input grew while being read; no partial text was returned.");
            return Decode(canonical, bytes.AsSpan(0, count));
        }
        catch (Exception error) when (IsInputException(error))
        {
            IncludeProbeResult failure = FromException(canonical, error);
            return failure.Status == IncludeInputStatus.Missing ? IncludeReadResult.Missing(canonical)
                : IncludeReadResult.Failed(canonical, failure.Failure!);
        }
    }

    private IncludeReadResult Decode(string canonical, ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith((ReadOnlySpan<byte>)[0x00, 0x00, 0xfe, 0xff]) ||
            bytes.StartsWith((ReadOnlySpan<byte>)[0xff, 0xfe, 0x00, 0x00]) ||
            (bytes.Length >= 4 && bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76 &&
             bytes[3] is 0x38 or 0x39 or 0x2b or 0x2f))
            return ReadFailure(canonical, IncludeFailureKind.UnsupportedEncoding, "UTF-32 and UTF-7 input encodings are not supported.");
        Encoding encoding;
        int bom;
        if (bytes.StartsWith((ReadOnlySpan<byte>)[0xef, 0xbb, 0xbf]))
            (encoding, bom) = (new UTF8Encoding(false, true), 3);
        else if (bytes.StartsWith((ReadOnlySpan<byte>)[0xff, 0xfe]))
            (encoding, bom) = (new UnicodeEncoding(false, false, true), 2);
        else if (bytes.StartsWith((ReadOnlySpan<byte>)[0xfe, 0xff]))
            (encoding, bom) = (new UnicodeEncoding(true, false, true), 2);
        else (encoding, bom) = (new UTF8Encoding(false, true), 0);
        try
        {
            ReadOnlySpan<byte> content = bytes[bom..];
            // Strict count validates all bytes and bounds the result before the string allocation.
            if (encoding.GetCharCount(content) > MaxInputCharacters)
                return ReadFailure(canonical, IncludeFailureKind.LimitExceeded, "Include input exceeds the configured UTF-16 character limit.");
            return IncludeReadResult.Found(canonical, encoding.GetString(content));
        }
        catch (DecoderFallbackException)
        {
            return ReadFailure(canonical, IncludeFailureKind.InvalidEncoding, "Include input has malformed bytes for its selected UTF encoding.");
        }
    }

    public bool TryRead(string path, out string? text)
    {
        IncludeReadResult result = Read(path);
        text = result.Text;
        if (result.Status == IncludeInputStatus.Failure) throw new IncludeInputException(result.CanonicalPath, result.Failure!);
        return result.Status == IncludeInputStatus.Found;
    }

    private static IncludeProbeResult ProbeFailure(string? path, IncludeFailureKind kind, string message)
        => IncludeProbeResult.Failed(path, new IncludeInputFailure(kind, message));
    private static IncludeReadResult ReadFailure(string path, IncludeFailureKind kind, string message)
        => IncludeReadResult.Failed(path, new IncludeInputFailure(kind, message));
    private static bool IsInputException(Exception error)
        => error is IOException or UnauthorizedAccessException or SecurityException or ArgumentException or NotSupportedException
            or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException;
    private static IncludeProbeResult FromException(string? path, Exception error)
        => error switch
        {
            FileNotFoundException or DirectoryNotFoundException => IncludeProbeResult.Missing(path!),
            DllNotFoundException or EntryPointNotFoundException or BadImageFormatException => UnsupportedHost(path),
            UnauthorizedAccessException or SecurityException => ProbeFailure(path, IncludeFailureKind.Access, error.Message),
            PathTooLongException or ArgumentException or NotSupportedException => ProbeFailure(path, IncludeFailureKind.InvalidPath, error.Message),
            _ => ProbeFailure(path, IncludeFailureKind.Io, error.Message)
        };

    private static IncludeReadResult ReadResult(IncludeProbeResult result)
        => result.Status == IncludeInputStatus.Missing ? IncludeReadResult.Missing(result.CanonicalPath!)
            : IncludeReadResult.Failed(result.CanonicalPath, result.Failure!);
    private static IncludeProbeResult FromErrno(string path, int error, string operation)
    {
        // Linux x86-64 errno values. Interrupted/would-block operations are explicit I/O
        // failures; there is no unbounded retry loop or misclassification as Missing.
        if (error is 2 or 20 or 21) return IncludeProbeResult.Missing(path); // ENOENT, ENOTDIR, EISDIR
        IncludeFailureKind kind = error switch
        {
            1 or 13 => IncludeFailureKind.Access,                 // EPERM, EACCES
            22 or 36 or 40 => IncludeFailureKind.InvalidPath,      // EINVAL, ENAMETOOLONG, ELOOP
            38 => IncludeFailureKind.UnsupportedHost,             // ENOSYS
            _ => IncludeFailureKind.Io
        };
        return ProbeFailure(path, kind, $"Linux filesystem {operation} failed with errno {error}.");
    }

    // This is a host ABI binding, not an EDG FILE* model. See the handoff for
    // inspected libc headers/runtime sources. Other hosts never call these APIs.
    private static class LinuxFiles
    {
        internal const int NonBlocking = 0x800;  // O_RDONLY is zero; O_NONBLOCK
        internal const int CloseOnExec = 0x80000;
        internal const uint Regular = 0x8000, Directory = 0x4000;
        [StructLayout(LayoutKind.Explicit, Size = 144)]
        internal struct FileStatus
        {
            // Linux x86-64 libc struct stat is 144 bytes; only these ABI fields
            // are interpreted. Explicit size retains space for every native write.
            [FieldOffset(24)] internal uint Mode;
            [FieldOffset(48)] internal long Length;
            internal readonly uint Kind => Mode & 0xf000;
        }
        // Bind identity and filesystem operations to the same GNU libc SONAME.
        [DllImport("libc.so.6", EntryPoint = "gnu_get_libc_version", ExactSpelling = true)]
        internal static extern IntPtr GlibcVersion();
        [DllImport("libc.so.6", EntryPoint = "open", ExactSpelling = true, SetLastError = true)]
        internal static extern int Open([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int flags);
        [DllImport("libc.so.6", EntryPoint = "stat", ExactSpelling = true, SetLastError = true)]
        internal static extern int StatPath([MarshalAs(UnmanagedType.LPUTF8Str)] string path, out FileStatus status);
        [DllImport("libc.so.6", EntryPoint = "fstat", ExactSpelling = true, SetLastError = true)]
        internal static extern int StatHandle(SafeFileHandle handle, out FileStatus status);
    }
}
