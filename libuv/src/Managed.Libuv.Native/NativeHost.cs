using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Managed.Libuv.Native;

/// <summary>
/// Frozen native process identity and lazy libraries. There is no libuv implementation here.
/// Loading this assembly neither discovers the host nor loads any native library.
/// </summary>
public sealed unsafe class NativeHost
{
    private static readonly Lazy<NativeHost> CurrentHost = new(Discover, LazyThreadSafetyMode.ExecutionAndPublication);
    private readonly ConcurrentDictionary<NativeLibraryKind, NativeModule> modules = new();
    private readonly nint libcBase;
    private readonly delegate* unmanaged[Cdecl]<nint, DlInfo*, int> dladdr;
    private int loadedLibraries;

    private NativeHost(NativeHostIdentity identity, nint libcBase = 0,
                       delegate* unmanaged[Cdecl]<nint, DlInfo*, int> dladdr = null)
    {
        Identity = identity;
        this.libcBase = libcBase;
        this.dladdr = dladdr;
    }

    public static NativeHost Current => CurrentHost.Value;
    public static bool IsDiscovered => CurrentHost.IsValueCreated;
    public NativeHostIdentity Identity { get; }
    /// <summary>Libraries loaded by this resolver, excluding the runtime's own libraries.</summary>
    public int LoadedLibraryCount => Volatile.Read(ref loadedLibraries);

    /// <summary>Creates a descriptor; the library is loaded only on its first export request.</summary>
    public NativeModule Library(NativeLibraryKind kind) => modules.GetOrAdd(kind, CreateModule);

    private NativeModule CreateModule(NativeLibraryKind kind)
    {
        string[] names = (Identity.OperatingSystem, Identity.CEnvironment, kind) switch
        {
            (NativeOperatingSystem.Linux, _, NativeLibraryKind.CRuntime) => [Identity.ProviderPath],
            (NativeOperatingSystem.Android, _, NativeLibraryKind.CRuntime) => [Identity.ProviderPath],
            (NativeOperatingSystem.Linux, NativeCEnvironment.Glibc, NativeLibraryKind.DynamicLoader) => ["libdl.so.2"],
            (NativeOperatingSystem.Linux, NativeCEnvironment.Glibc, NativeLibraryKind.Threads) => ["libpthread.so.0"],
            (NativeOperatingSystem.Linux, NativeCEnvironment.Glibc, NativeLibraryKind.Realtime) => ["librt.so.1"],
            (NativeOperatingSystem.Linux, NativeCEnvironment.Musl, NativeLibraryKind.DynamicLoader or NativeLibraryKind.Threads or NativeLibraryKind.Realtime) => [Identity.ProviderPath],
            (NativeOperatingSystem.Android, _, NativeLibraryKind.Threads or NativeLibraryKind.Realtime) => [Identity.ProviderPath],
            (NativeOperatingSystem.Android, _, NativeLibraryKind.DynamicLoader) => ["libdl.so"],
            (NativeOperatingSystem.Windows, _, NativeLibraryKind.CRuntime) => ["ucrtbase.dll"],
            (NativeOperatingSystem.Windows, _, NativeLibraryKind.WindowsKernel) => ["kernel32.dll"],
            (NativeOperatingSystem.Windows, _, NativeLibraryKind.WindowsSockets) => ["ws2_32.dll"],
            (NativeOperatingSystem.Windows, _, NativeLibraryKind.WindowsNt) => ["ntdll.dll"],
            (NativeOperatingSystem.MacOS, _, NativeLibraryKind.CRuntime or NativeLibraryKind.Threads or NativeLibraryKind.DynamicLoader or NativeLibraryKind.Realtime) => ["/usr/lib/libSystem.B.dylib"],
            (NativeOperatingSystem.MacOS, _, NativeLibraryKind.AppleCoreFoundation) => ["/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation"],
            (NativeOperatingSystem.MacOS, _, NativeLibraryKind.AppleCoreServices) => ["/System/Library/Frameworks/CoreServices.framework/CoreServices"],
            _ => throw new PlatformNotSupportedException($"Native library {kind} is not defined for {Identity.OperatingSystem}/{Identity.CEnvironment}; no foreign library was loaded.")
        };
        return new NativeModule(kind, names, ValidateLoadedModule);
    }

    private void ValidateLoadedModule(NativeLibraryKind kind, nint handle)
    {
        if (kind == NativeLibraryKind.CRuntime && libcBase != 0)
        {
            var info = Describe(dladdr, NativeLibrary.GetExport(handle, "malloc"));
            if (info.Base != libcBase)
                throw new PlatformNotSupportedException("Loaded C runtime differs from the observed process malloc provider.");
        }
        Interlocked.Increment(ref loadedLibraries);
    }

    private static NativeHost Discover()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        if (architecture is not (Architecture.X64 or Architecture.Arm64 or Architecture.X86 or Architecture.Arm))
            throw new PlatformNotSupportedException($"Native ABI for process architecture {architecture} has not been described.");
        var model = IntPtr.Size == 4 ? NativeDataModel.ILP32 : OperatingSystem.IsWindows() ? NativeDataModel.LLP64 : NativeDataModel.LP64;
        if (OperatingSystem.IsLinux() || OperatingSystem.IsAndroid())
            return DiscoverLinux(architecture, model);
        if (OperatingSystem.IsWindows())
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10))
                throw new PlatformNotSupportedException("The pinned libuv Windows backend requires Windows 10 or later.");
            return new(new(NativeOperatingSystem.Windows, architecture, NativeCEnvironment.UniversalCrt,
                           model, BitConverter.IsLittleEndian, "ucrtbase.dll",
                           "Windows runtime identity; explicit Windows 10+ UCRT binding policy. No translated backend registered."));
        }
        if (OperatingSystem.IsMacOS())
            return new(new(NativeOperatingSystem.MacOS, architecture, NativeCEnvironment.LibSystem,
                           model, BitConverter.IsLittleEndian, "/usr/lib/libSystem.B.dylib",
                           "macOS runtime identity; system libSystem binding policy. No translated backend registered."));
        throw new PlatformNotSupportedException($"No validated native ABI identity for {RuntimeInformation.OSDescription}; no translated backend selected.");
    }

    private static NativeHost DiscoverLinux(Architecture architecture, NativeDataModel model)
    {
        // Process scope inspects already loaded providers. It never guesses libc by loading one.
        nint process = NativeLibrary.GetMainProgramHandle();
        if (!NativeLibrary.TryGetExport(process, "dladdr", out var describePointer))
            throw new PlatformNotSupportedException("Cannot identify the loaded libc: process-scope dladdr is unavailable.");
        var describe = (delegate* unmanaged[Cdecl]<nint, DlInfo*, int>) describePointer;
        Provider? provider = null;
        foreach (var symbol in new[] { "malloc", "free", "read", OperatingSystem.IsAndroid() ? "__errno" : "__errno_location" })
        {
            if (!NativeLibrary.TryGetExport(process, symbol, out var address))
                throw new PlatformNotSupportedException($"Cannot identify the loaded libc: process-scope {symbol} is unavailable.");
            var candidate = Describe(describe, address);
            if (provider is not null && provider.Value.Base != candidate.Base)
                throw new PlatformNotSupportedException($"Ambiguous/interposed C runtime providers: {provider.Value.Path} and {candidate.Path}; explicit ABI validation is required.");
            provider = candidate;
        }
        var observed = provider!.Value;
        NativeCEnvironment environment;
        if (NativeLibrary.TryGetExport(process, "gnu_get_libc_version", out var version))
        {
            if (Describe(describe, version).Base != observed.Base)
                throw new PlatformNotSupportedException("GNU libc marker and process C functions resolve to different providers.");
            environment = NativeCEnvironment.Glibc;
        }
        else if (IsMuslProviderName(observed.Path))
            environment = NativeCEnvironment.Musl;
        else if (OperatingSystem.IsAndroid() && NativeLibrary.TryGetExport(process, "__system_property_get", out var properties)
                 && Describe(describe, properties).Base == observed.Base)
            environment = NativeCEnvironment.Bionic;
        else
            throw new PlatformNotSupportedException($"Unrecognized loaded libc provider {observed.Path}; ABI identity remains ambiguous.");
        if (!Path.IsPathFullyQualified(observed.Path))
            throw new PlatformNotSupportedException($"Libc provider path is not absolute: {observed.Path}; refusing ambiguous reload.");
        return new(new(environment == NativeCEnvironment.Bionic ? NativeOperatingSystem.Android : NativeOperatingSystem.Linux,
                       architecture, environment, model, BitConverter.IsLittleEndian, observed.Path,
                       "Process-scope malloc/free/read/errno resolve to one dladdr provider; libc marker checked without loading foreign libraries."),
                   observed.Base, describe);
    }

    private static bool IsMuslProviderName(string path)
    {
        var file = Path.GetFileName(path);
        return (file.StartsWith("ld-musl-", StringComparison.Ordinal) && file.EndsWith(".so.1", StringComparison.Ordinal))
            || (file.StartsWith("libc.musl-", StringComparison.Ordinal) && file.EndsWith(".so.1", StringComparison.Ordinal));
    }

    private static Provider Describe(delegate* unmanaged[Cdecl]<nint, DlInfo*, int> describe, nint address)
    {
        DlInfo info = default;
        if (describe(address, &info) == 0 || info.FileName == 0 || info.Base == 0)
            throw new PlatformNotSupportedException("Native symbol provider could not be identified by dladdr.");
        var path = Marshal.PtrToStringUTF8(info.FileName)
            ?? throw new PlatformNotSupportedException("Native symbol provider has no path.");
        return new(path, info.Base);
    }

    private readonly record struct Provider(string Path, nint Base);
    [StructLayout(LayoutKind.Sequential)]
    private struct DlInfo { public nint FileName, Base, SymbolName, SymbolAddress; }
}
