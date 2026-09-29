using System.Runtime.InteropServices;
using Managed.Libuv.Native;

unsafe class Program
{
    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static int Main(string[] args)
    {
        Require(!NativeHost.IsDiscovered, "Assembly load must not discover host or load native providers.");
        if (args.Contains("--expect-ambiguous", StringComparer.Ordinal))
        {
            try { _ = NativeHost.Current; throw new Exception("Mixed/interposed libc providers accepted."); }
            catch (PlatformNotSupportedException error) when (error.Message.Contains("Ambiguous/interposed", StringComparison.Ordinal))
            {
                Console.WriteLine("ambiguous-runtime=rejected-before-library-load");
                return 0;
            }
        }
        var host = NativeHost.Current;
        Require(ReferenceEquals(host, NativeHost.Current), "Identity must be frozen.");
        Require(host.Identity.Architecture == RuntimeInformation.ProcessArchitecture, "Process architecture mismatch.");
        Require(host.LoadedLibraryCount == 0, "Discovery loaded a native library instead of inspecting existing providers.");
        var libc = host.Library(NativeLibraryKind.CRuntime);
        Require(!libc.IsLoaded && host.LoadedLibraryCount == 0, "Creating a descriptor eagerly loaded libc.");
        var foreign = OperatingSystem.IsWindows() ? NativeLibraryKind.AppleCoreFoundation : NativeLibraryKind.WindowsKernel;
        try { host.Library(foreign); throw new Exception("Foreign library descriptor accepted."); }
        catch (PlatformNotSupportedException) { }
        Require(host.LoadedLibraryCount == 0, "Foreign library rejection must not load libraries.");
        Parallel.For(0, 32, _ => Require(libc.GetRequiredExport("malloc") != 0, "malloc missing"));
        Require(libc.IsLoaded && host.LoadedLibraryCount == 1, "Concurrent binding loaded libc multiple times.");
        Require(!libc.TryGetExport("dotcc_libuv_required_missing_test_symbol", out var missing) && missing == 0,
                "Missing optional symbol must remain absent.");
        try { libc.GetRequiredExport("dotcc_libuv_required_missing_test_symbol"); throw new Exception("Missing required symbol accepted."); }
        catch (EntryPointNotFoundException) { }
        var absentLibrary = new NativeModule(NativeLibraryKind.CRuntime,
            ["dotcc-libuv-definitely-absent-native-library-21a5d4"], (_, _) => throw new Exception("Absent library validated."));
        Require(!absentLibrary.TryLoad() && !absentLibrary.TryGetExport("malloc", out _), "Absent optional library accepted.");
        Require(!absentLibrary.IsLoaded, "Absent library reported loaded.");
        try { absentLibrary.GetRequiredExport("malloc"); throw new Exception("Absent required library accepted."); }
        catch (DllNotFoundException) { }
        var malloc = (delegate* unmanaged[Cdecl]<nuint, void*>) libc.GetRequiredExport("malloc");
        var free = (delegate* unmanaged[Cdecl]<void*, void>) libc.GetRequiredExport("free");
        var bytes = (byte*) malloc(64);
        Require(bytes != null, "Native malloc failed.");
        try
        {
            for (var index = 0; index < 64; index++) bytes[index] = (byte) (index ^ 0x5a);
            GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
            for (var index = 0; index < 64; index++) Require(bytes[index] == (byte) (index ^ 0x5a), "Native buffer changed across GC.");
        }
        finally { free(bytes); }
        if (OperatingSystem.IsLinux())
        {
            var getpid = (delegate* unmanaged[Cdecl]<int>) libc.GetRequiredExport("getpid");
            Require(getpid() == Environment.ProcessId, "Native PID differs from process identity.");
            Require(host.Identity.CEnvironment is NativeCEnvironment.Glibc or NativeCEnvironment.Musl,
                    "Linux libc identity is unresolved.");
            var close = (delegate* unmanaged[Cdecl]<int, int>) libc.GetRequiredExport("close");
            var errno = (delegate* unmanaged[Cdecl]<int*>) libc.GetRequiredExport("__errno_location");
            Require(close(-1) == -1 && *errno() == 9, "Actual native errno not preserved at call boundary.");
        }
        Console.WriteLine($"dispatch-foundation=passed os={host.Identity.OperatingSystem} arch={host.Identity.Architecture} libc={host.Identity.CEnvironment} provider={host.Identity.ProviderPath} loads={host.LoadedLibraryCount} translated-backend=unimplemented");
        return 0;
    }
}
