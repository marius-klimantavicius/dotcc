using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace RoslynObj.CSharp.Runtime;

/// <summary>
/// An observed borrowed data export. The version identifies the pinned ELF's
/// default-export metadata; resolution does not perform a versioned lookup.
/// Addresses identify provider-owned storage, not generated allocations.
/// </summary>
public sealed record BorrowedNativeRttiDataResolution(
    string CanonicalLibraryPath,
    string LibrarySha256,
    string ExportName,
    string PinnedProviderElfSymbolVersion,
    nint SymbolBaseAddress,
    nint ModuleLoadBaseAddress,
    ulong ExportRelativeVirtualAddress,
    int SymbolSizeBytes,
    nint MappingStartAddress,
    nint MappingEndAddress,
    string MappingPermissions,
    bool LoadedOwnerVerified,
    bool ReadOnlyMappingVerified);

/// <summary>
/// Resolves the two admitted ABI RTTI data addresses from the exact audited
/// provider. This does not read an RTTI slot, implement RTTI queries, apply a
/// captured address offset, or transfer ownership of native storage.
/// </summary>
public static class BorrowedNativeRttiData
{
    private const string PinnedSha256 =
        "1fd75fe70354a416d75aef22bcae68c47bd25d20e2d0568c30b1a9838cf62f11";
    private const string PinnedSymbolVersion = "CXXABI_1.3";
    private const int PinnedSymbolSize = 88;
    private const string ClassTypeInfoVtable = "_ZTVN10__cxxabiv117__class_type_infoE";
    private const string SingleInheritanceTypeInfoVtable = "_ZTVN10__cxxabiv120__si_class_type_infoE";
    private static readonly object Gate = new();
    private static readonly Dictionary<string, BorrowedNativeRttiDataResolution> Resolutions =
        new(StringComparer.Ordinal);
    private static ProviderLease? provider;

    // The library handle intentionally has no release path. Every published
    // address remains valid for the entire process lifetime.
    private sealed record ProviderLease(string Path, ulong Inode, nint Handle);
    private sealed record Mapping(
        ulong Start, ulong End, ulong Offset, string Permissions,
        ulong Inode, string OwnerPath);

    /// <summary>
    /// Returns the exported symbol BASE. The emitter applies its separately
    /// admitted captured ByteOffset exactly once. ELF size 88 is a provider
    /// fact; it is not a completed EDG array bound or a generated copy size.
    /// </summary>
    public static nint ResolveAddressOnlyVtable(
        string libraryPath,
        string librarySha256,
        string exportName,
        ulong exportRelativeVirtualAddress,
        int symbolSizeBytes,
        string symbolVersion)
    {
        RequirePlatform();
        ArgumentNullException.ThrowIfNull(libraryPath);
        ArgumentNullException.ThrowIfNull(librarySha256);
        ArgumentNullException.ThrowIfNull(exportName);
        ArgumentNullException.ThrowIfNull(symbolVersion);
        ValidateDescriptor(librarySha256, exportName, exportRelativeVirtualAddress, symbolSizeBytes, symbolVersion);
        if (!Path.IsPathFullyQualified(libraryPath))
            throw Unsupported("The provider path must be absolute.");

        lock (Gate)
        {
            var canonicalPath = CanonicalPath(libraryPath);
            if (!File.Exists(canonicalPath)) throw Unsupported("The provider must be an existing file.");
            using var file = new FileStream(canonicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            VerifyHash(file);
            var inode = ReadOpenFileInode(file);

            if (provider is null)
            {
                var handle = NativeLibrary.Load(canonicalPath);
                provider = new(canonicalPath, inode, handle);
            }
            else if (!StringComparer.Ordinal.Equals(provider.Path, canonicalPath) || provider.Inode != inode)
            {
                throw Unsupported("Only the same verified provider file may own these borrowed addresses in one process.");
            }

            var symbolBase = NativeLibrary.GetExport(provider.Handle, exportName);
            if (symbolBase == 0) throw Unsupported("The exact data export has no address.");
            if (Resolutions.TryGetValue(exportName, out var previous) && previous.SymbolBaseAddress != symbolBase)
                throw Unsupported("A previously verified borrowed data export changed its canonical address.");

            var mappings = ReadMappings();
            var owners = mappings.Where(mapping => mapping.Inode == inode &&
                StringComparer.Ordinal.Equals(mapping.OwnerPath, canonicalPath)).ToArray();
            var bases = owners.Where(mapping => mapping.Offset == 0).ToArray();
            if (bases.Length != 1) throw Unsupported("The exact provider must have one identifiable file-offset-zero mapping.");
            var moduleBase = bases[0].Start;
            var symbolAddress = Address(symbolBase);
            if (symbolAddress != checked(moduleBase + exportRelativeVirtualAddress))
                throw Unsupported("The resolved data export is not owned by the pinned provider at its audited RVA.");
            var symbolEnd = checked(symbolAddress + (ulong)symbolSizeBytes);
            var symbolMappings = owners.Where(mapping => mapping.Start <= symbolAddress && mapping.End >= symbolEnd).ToArray();
            if (symbolMappings.Length != 1 || symbolMappings[0].Permissions != "r--p")
                throw Unsupported("The complete provider data symbol must occupy one read-only, non-executable private mapping.");

            // Revalidate the same open file after loader resolution and map
            // inspection. No mutable file bytes stand in for relocated data.
            VerifyHash(file);
            var mapping = symbolMappings[0];
            Resolutions[exportName] = new(
                canonicalPath, PinnedSha256, exportName, PinnedSymbolVersion,
                symbolBase, Pointer(moduleBase), exportRelativeVirtualAddress,
                symbolSizeBytes, Pointer(mapping.Start), Pointer(mapping.End),
                mapping.Permissions, LoadedOwnerVerified: true, ReadOnlyMappingVerified: true);
            return symbolBase;
        }
    }

    /// <summary>Returns a stable snapshot of successful resolutions only.</summary>
    public static IReadOnlyList<BorrowedNativeRttiDataResolution> GetDiagnostics()
    {
        lock (Gate)
            return Array.AsReadOnly(Resolutions.Values.OrderBy(row => row.ExportName, StringComparer.Ordinal).ToArray());
    }

    private static void RequirePlatform()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64 ||
            !BitConverter.IsLittleEndian || IntPtr.Size != 8)
            throw Unsupported("Borrowed RTTI data is admitted only on little-endian Linux x64 with 8-byte pointers.");
    }

    private static void ValidateDescriptor(string hash, string name, ulong rva, int size, string version)
    {
        var expectedRva = name switch
        {
            ClassTypeInfoVtable => 0x26dd18UL,
            SingleInheritanceTypeInfoVtable => 0x26ebd8UL,
            _ => throw Unsupported("The data export is outside the exact two-symbol RTTI address contract.")
        };
        if (!StringComparer.OrdinalIgnoreCase.Equals(hash, PinnedSha256) || rva != expectedRva ||
            size != PinnedSymbolSize || !StringComparer.Ordinal.Equals(version, PinnedSymbolVersion))
            throw Unsupported("The provider hash, default ELF symbol version, size or RVA is not the audited descriptor.");
    }

    private static void VerifyHash(FileStream file)
    {
        file.Position = 0;
        var hash = Convert.ToHexStringLower(SHA256.HashData(file));
        if (!StringComparer.Ordinal.Equals(hash, PinnedSha256))
            throw Unsupported("The provider file does not match its exact pinned SHA-256.");
    }

    private static ulong ReadOpenFileInode(FileStream file)
    {
        var descriptor = file.SafeFileHandle.DangerousGetHandle().ToInt64();
        foreach (var line in File.ReadLines($"/proc/self/fdinfo/{descriptor.ToString(CultureInfo.InvariantCulture)}"))
        {
            if (line.StartsWith("ino:", StringComparison.Ordinal) &&
                ulong.TryParse(line.AsSpan(4).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var inode) && inode != 0)
                return inode;
        }
        throw Unsupported("The open provider file has no verifiable Linux inode identity.");
    }

    private static List<Mapping> ReadMappings()
    {
        var result = new List<Mapping>();
        foreach (var line in File.ReadLines("/proc/self/maps"))
        {
            var fields = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 6) continue;
            var ownerPath = fields[5].TrimStart(' ');
            if (!ownerPath.StartsWith('/')) continue;
            var bounds = fields[0].Split('-');
            if (bounds.Length != 2 ||
                !ulong.TryParse(bounds[0], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var start) ||
                !ulong.TryParse(bounds[1], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var end) ||
                !ulong.TryParse(fields[2], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var offset) ||
                !ulong.TryParse(fields[4], NumberStyles.None, CultureInfo.InvariantCulture, out var inode) ||
                end <= start || fields[1].Length != 4)
                throw Unsupported("A file-backed Linux mapping could not be parsed exactly.");
            result.Add(new(start, end, offset, fields[1], inode, ownerPath));
        }
        return result;
    }

    private static string CanonicalPath(string path, int depth = 0)
    {
        if (depth > 16) throw Unsupported("The provider path exceeds the bounded symlink resolution depth.");
        var fullPath = Path.GetFullPath(path);
        var current = Path.GetPathRoot(fullPath) ?? throw Unsupported("The provider path has no filesystem root.");
        foreach (var component in fullPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(current, component);
            FileSystemInfo info = Directory.Exists(candidate) ? new DirectoryInfo(candidate) : new FileInfo(candidate);
            var target = info.ResolveLinkTarget(returnFinalTarget: true);
            current = target is null ? candidate : CanonicalPath(target.FullName, depth + 1);
        }
        return current;
    }

    private static ulong Address(nint pointer)
    {
        if (pointer <= 0) throw Unsupported("A provider address is outside the admitted positive Linux userspace range.");
        return (ulong)pointer;
    }

    private static nint Pointer(ulong address)
    {
        if (address == 0 || address > long.MaxValue) throw Unsupported("A mapping address is outside the admitted Linux userspace range.");
        return (nint)(long)address;
    }

    private static PlatformNotSupportedException Unsupported(string message) => new(message);
}
