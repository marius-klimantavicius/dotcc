using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

internal static unsafe class HostContractProbe
{
    private const int ProtectedKind = 0, ReentryKind = 1, ThreadKind = 2, MutexKind = 3;
    private const int OuterResult = 0x4a17, NestedResult = 0x2b13, CallbackFailed = -7001;
    private static nint helperHandle, libcHandle;
    private static Dictionary<string, long> abi = [];

    public static int Main(string[] args)
    {
        if (args.Length != 2)
            throw new ArgumentException("Usage: HostContracts ABSOLUTE_NATIVE_HELPER_PATH REPORT_JSON");
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
            throw new PlatformNotSupportedException("This finite probe is specifically Linux x64.");
        var helperPath = Path.GetFullPath(args[0]);
        var reportPath = Path.GetFullPath(args[1]);
        if (!Path.IsPathFullyQualified(args[0])) throw new ArgumentException("Use an absolute helper path.");
        helperHandle = NativeLibrary.Load(helperPath);
        NativeLibrary.SetDllImportResolver(typeof(Native).Assembly, ResolveLibrary);
        abi = Enumerable.Range(0, Native.AbiCount()).ToDictionary(
            index => Utf8(Native.AbiName(index)), index => Native.AbiValue(index), StringComparer.Ordinal);
        if (abi["pointer_size"] != 8 || abi["int_size"] != 4 || abi["int64_size"] != 8 || abi["size_t_size"] != 8)
            throw new PlatformNotSupportedException("The actual native ABI does not match this Linux x64 probe.");
        var providers = Enumerable.Range(0, Native.ProviderCount()).Select(index => new Provider(
            Utf8(Native.ProviderSymbol(index)), Native.ProviderAddress(index),
            Path.GetFullPath(Utf8(Native.ProviderPath(index))), Native.ProviderBase(index))).ToArray();
        var errnoProvider = providers.Single(provider => provider.Symbol == "__errno_location");
        libcHandle = NativeLibrary.Load(errnoProvider.Path);
        var checks = new List<CheckResult>();
        void Check(string name, bool passed) => checks.Add(new(name, passed));
        Check("loaded_errno_symbol_matches_native_provider", (nuint)NativeLibrary.GetExport(libcHandle, "__errno_location") == errnoProvider.Address);
        Check("loaded_open_symbol_matches_native_provider", (nuint)NativeLibrary.GetExport(libcHandle, "open") == providers.Single(provider => provider.Symbol == "open").Address);
        Check("loaded_helper_symbol_matches_native_provider", (nuint)NativeLibrary.GetExport(helperHandle, "hc_abi_count") == providers.Single(provider => provider.Symbol == "hc_abi_count").Address);
        Check("provider_paths_exist", providers.All(provider => File.Exists(provider.Path)));
        Native.TlsWrite(abi["main_marker"]);
        var mainPoint = ReadPoint();

        // This is a raw managed PInvoke followed by a later managed TLS read.
        // There is deliberately no claim to have captured that call's errno in C.
        var pathBytes = Encoding.UTF8.GetBytes("/dev/null/roslynobj-errno-probe\0");
        int rawOpenReturn;
        fixed (byte* path = pathBytes) rawOpenReturn = Native.RawOpen(path, checked((int)abi["O_RDONLY"]));
        var rawManagedLater = ReadPoint();
        Churn();
        var rawAfterAllocationAndGc = ReadPoint();
        var separateNativeReturn = Native.FailCapture(out var separateImmediateError, out var separateNativeTls);
        var rawAfterSeparateNativeReentry = ReadPoint();
        Check("raw_open_failed", rawOpenReturn == -1);
        Check("separate_native_open_failed", separateNativeReturn == -1);
        Check("separate_native_immediate_errno_is_ENOTDIR", separateImmediateError == abi["ENOTDIR"]);
        Check("separate_native_tls_matches_main", separateNativeTls == mainPoint.ErrnoTls);

        var protectedState = new CallbackState("protected");
        var protectedHandle = GCHandle.Alloc(protectedState);
        var protectedValues = NewReport(ProtectedKind);
        int protectedReturn;
        try
        {
            fixed (long* values = protectedValues)
                protectedReturn = Native.ProtectedProbe(&OuterCallback, GCHandle.ToIntPtr(protectedHandle), values, (nuint)protectedValues.Length);
        }
        finally { protectedHandle.Free(); }
        var protectedManagedLater = ReadPoint();
        var protectedReport = ReadReport(ProtectedKind, protectedValues);
        Check("protected_helper_return", protectedReturn == 0);
        Check("protected_native_open_failed", protectedReport["open_return"] == -1);
        Check("protected_native_immediate_errno_is_ENOTDIR", protectedReport["native_errno_immediate"] == abi["ENOTDIR"]);
        Check("protected_errno_restored_inside_native_scope", protectedReport["native_errno_restored"] == protectedReport["native_errno_immediate"]);
        Check("protected_native_errno_tls_stable", protectedReport["native_errno_tls_before"] == protectedReport["native_errno_tls_after"]);
        Check("protected_native_marker_tls_stable", protectedReport["native_marker_tls_before"] == protectedReport["native_marker_tls_after"]);
        CheckCallback(protectedState, mainPoint, protectedReport["callback_return"], Check);

        Native.TlsWrite(abi["main_marker"]);
        var threadState = new CallbackState("foreign-pthread");
        var threadHandle = GCHandle.Alloc(threadState);
        var threadValues = NewReport(ThreadKind);
        var releaseThreadHandle = false;
        var threadCallbackCompleted = false;
        int threadReturn;
        Dictionary<string, long> threadReport;
        try
        {
            fixed (long* values = threadValues)
                threadReturn = Native.ThreadProbe(&OuterCallback, GCHandle.ToIntPtr(threadHandle), values, (nuint)threadValues.Length);
            threadCallbackCompleted = Volatile.Read(ref threadState.OuterCompleted) == 1;
            threadReport = ReadReport(ThreadKind, threadValues);
            releaseThreadHandle = threadReport["callback_context_must_remain_live"] == 0;
        }
        finally
        {
            // An unexpected failed join must not free a potentially live callback handle.
            if (releaseThreadHandle) threadHandle.Free();
        }
        Check("pthread_helper_return", threadReturn == 0);
        Check("pthread_create_return_zero", threadReport["create_return"] == 0);
        Check("pthread_join_return_zero", threadReport["join_return"] == 0);
        Check("pthread_callback_acquired_completion", threadCallbackCompleted);
        Check("pthread_worker_entered_and_completed", threadReport["worker_entered"] == 1 && threadReport["worker_completed"] == 1);
        Check("pthread_main_errno_tls_stable", threadReport["native_main_errno_tls_before"] == threadReport["native_main_errno_tls_after"]);
        Check("pthread_worker_errno_tls_stable", threadReport["worker_errno_tls_before"] == threadReport["worker_errno_tls_after"]);
        Check("pthread_errno_tls_distinct_from_main", threadReport["worker_errno_tls_before"] != threadReport["native_main_errno_tls_before"]);
        Check("pthread_marker_tls_distinct_from_main", threadReport["worker_marker_tls_before"] != threadReport["native_main_marker_tls_before"]);
        Check("pthread_main_marker_unchanged", threadReport["native_main_marker_before"] == abi["main_marker"] && threadReport["native_main_marker_after"] == abi["main_marker"]);
        Check("pthread_worker_marker_unchanged", threadReport["worker_marker_before"] == abi["worker_marker"] && threadReport["worker_marker_after"] == abi["worker_marker"]);
        Check("pthread_worker_managed_thread_distinct", threadState.OuterBefore is { } workerManaged && workerManaged.ManagedThreadId != mainPoint.ManagedThreadId);
        Check("pthread_callback_native_errno_tls_matches", threadState.OuterBefore is { } workerPoint && unchecked((long)workerPoint.ErrnoTls) == threadReport["worker_errno_tls_before"]);
        Check("pthread_callback_native_marker_tls_matches", threadState.OuterBefore is { } workerMarker && unchecked((long)workerMarker.MarkerTls) == threadReport["worker_marker_tls_before"]);
        CheckCallback(threadState, threadState.OuterBefore, threadReport["callback_return"], Check);

        var mutexValues = NewReport(MutexKind);
        int mutexReturn;
        fixed (long* values = mutexValues) mutexReturn = Native.MutexProbe(values, (nuint)mutexValues.Length);
        var mutexReport = ReadReport(MutexKind, mutexValues);
        Check("mutex_helper_return", mutexReturn == 0);
        foreach (var name in new[] { "attr_init", "attr_settype", "mutex_init", "lock", "unlock", "mutex_destroy", "attr_destroy" })
            Check("mutex_" + name + "_return_zero", mutexReport[name] == 0);
        Check("mutex_trylock_held_returns_EBUSY", mutexReport["trylock_held"] == abi["EBUSY"]);
        Check("mutex_errorcheck_lock_held_returns_EDEADLK", mutexReport["lock_held_errorcheck"] == abi["EDEADLK"]);
        Check("mutex_unlock_unowned_returns_EPERM", mutexReport["unlock_unowned"] == abi["EPERM"]);
        Check("main_marker_still_unchanged", Native.TlsRead() == abi["main_marker"]);

        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        using (var stream = File.Create(reportPath))
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", "roslynobj-host-contract-probe-v1");
            writer.WriteBoolean("passed", checks.All(check => check.Passed));
            writer.WriteString("runtime", RuntimeInformation.FrameworkDescription);
            writer.WriteString("os", RuntimeInformation.OSDescription);
            writer.WriteString("architecture", RuntimeInformation.ProcessArchitecture.ToString());
            writer.WriteBoolean("dynamicCodeSupported", RuntimeFeature.IsDynamicCodeSupported);
            writer.WriteString("executable", Environment.ProcessPath);
            writer.WriteString("nativeHelper", helperPath);
            writer.WriteString("nativeHelperSha256", Hash(helperPath));
            writer.WriteString("scope", "Finite Linux x64 host observations; no product errno, threading, callback, or mutex implementation is claimed.");
            writer.WriteString("errnoLimit", "Managed-later reads are observations after transitions. No errno equality across those transitions is required. Restoration is checked only inside the protected native scope.");
            writer.WriteString("boundaryLimit", "Callbacks catch managed exceptions and report failure codes. No exception may cross a native boundary. Join failure intentionally retains callback context until process exit.");
            WriteMap(writer, "nativeAbi", abi);
            writer.WriteStartArray("providers");
            foreach (var provider in providers)
            {
                writer.WriteStartObject(); writer.WriteString("symbol", provider.Symbol);
                writer.WriteString("address", Hex(provider.Address)); writer.WriteString("moduleBase", Hex(provider.Base));
                writer.WriteString("path", provider.Path); writer.WriteString("sha256", Hash(provider.Path)); writer.WriteEndObject();
            }
            writer.WriteEndArray();
            WritePoint(writer, "main", mainPoint);
            writer.WriteStartObject("directUnprotected");
            writer.WriteNumber("rawManagedPInvokeOpenReturn", rawOpenReturn);
            WritePoint(writer, "managedLater", rawManagedLater);
            WritePoint(writer, "managedAfterAllocationAndGc", rawAfterAllocationAndGc);
            writer.WriteStartObject("separateNativeInvocation");
            writer.WriteNumber("openReturn", separateNativeReturn); writer.WriteNumber("errnoCapturedImmediatelyInC", separateImmediateError);
            writer.WriteString("nativeErrnoTls", Hex(separateNativeTls)); writer.WriteEndObject();
            WritePoint(writer, "managedAfterSeparateNativeReentry", rawAfterSeparateNativeReentry); writer.WriteEndObject();
            WriteMap(writer, "protectedNativeSnapshotRestore", protectedReport);
            WritePoint(writer, "protectedManagedLater", protectedManagedLater);
            WriteCallback(writer, protectedState);
            WriteMap(writer, "foreignPthreadNative", threadReport);
            WriteCallback(writer, threadState);
            WriteMap(writer, "mutexReturnProtocol", mutexReport);
            writer.WriteStartArray("checks");
            foreach (var check in checks)
            { writer.WriteStartObject(); writer.WriteString("name", check.Name); writer.WriteBoolean("passed", check.Passed); writer.WriteEndObject(); }
            writer.WriteEndArray(); writer.WriteEndObject();
        }
        var failed = checks.Where(check => !check.Passed).Select(check => check.Name).ToArray();
        Console.WriteLine($"checks={checks.Count}; failed={failed.Length}; report={reportPath}");
        foreach (var name in failed) Console.Error.WriteLine(name);
        return failed.Length == 0 ? 0 : 1;
    }

    private static void CheckCallback(CallbackState state, Point? expectedPoint, long nativeResult, Action<string, bool> check)
    {
        var name = state.Mode + "_";
        check(name + "no_managed_exception", state.Failure is null);
        check(name + "outer_and_nested_completed_once", Volatile.Read(ref state.OuterCompleted) == 1 && Volatile.Read(ref state.NestedCompleted) == 1 && state.OuterCount == 1 && state.NestedCount == 1);
        check(name + "native_callback_result", nativeResult == OuterResult);
        check(name + "managed_tls_stable", expectedPoint is { } point && state.OuterBefore?.ErrnoTls == point.ErrnoTls && state.OuterAfter?.ErrnoTls == point.ErrnoTls && state.NestedBefore?.ErrnoTls == point.ErrnoTls && state.NestedAfter?.ErrnoTls == point.ErrnoTls);
        check(name + "managed_marker_tls_stable", expectedPoint is { } marker && state.OuterBefore?.MarkerTls == marker.MarkerTls && state.OuterAfter?.MarkerTls == marker.MarkerTls && state.NestedBefore?.MarkerTls == marker.MarkerTls);
        check(name + "nested_native_open_failed", state.NestedOpenReturn == -1 && state.NestedImmediateError == abi["ENOTDIR"]);
        check(name + "nested_native_errno_tls_matches", expectedPoint is { } nativePoint && state.NestedNativeTls == nativePoint.ErrnoTls);
        check(name + "native_reentry_complete", state.ReentryReturn == 0 && state.Reentry is not null && state.Reentry["callback_return"] == NestedResult && state.Reentry["native_errno_tls_before"] == state.Reentry["native_errno_tls_after"]);
        check(name + "native_reentry_initial_errno_is_ERANGE", state.Reentry is not null && state.Reentry["native_errno_before_callback"] == abi["ERANGE"]);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int OuterCallback(nint opaque)
    {
        CallbackState? state = null;
        try
        {
            state = (CallbackState)GCHandle.FromIntPtr(opaque).Target!;
            Interlocked.Increment(ref state.OuterCount);
            state.OuterBefore = ReadPoint();
            Churn();
            var values = NewReport(ReentryKind);
            fixed (long* pointer = values)
                state.ReentryReturn = Native.ReentryProbe(&NestedCallback, opaque, checked((int)abi["ERANGE"]), pointer, (nuint)values.Length);
            state.Reentry = ReadReport(ReentryKind, values);
            state.OuterAfter = ReadPoint();
            Native.SetErrno(checked((int)abi["EINTR"]));
            Volatile.Write(ref state.OuterCompleted, 1);
            return OuterResult;
        }
        catch (Exception exception)
        {
            if (state is not null) state.Failure = exception.ToString();
            return CallbackFailed;
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static int NestedCallback(nint opaque)
    {
        CallbackState? state = null;
        try
        {
            state = (CallbackState)GCHandle.FromIntPtr(opaque).Target!;
            Interlocked.Increment(ref state.NestedCount);
            state.NestedBefore = ReadPoint();
            state.NestedOpenReturn = Native.FailCapture(out state.NestedImmediateError, out state.NestedNativeTls);
            Churn();
            state.NestedAfter = ReadPoint();
            Native.SetErrno(checked((int)abi["EINTR"]));
            Volatile.Write(ref state.NestedCompleted, 1);
            return NestedResult;
        }
        catch (Exception exception)
        {
            if (state is not null) state.Failure = exception.ToString();
            return CallbackFailed;
        }
    }

    private static Point ReadPoint()
    {
        var pointer = Native.ErrnoLocation();
        var value = *pointer;
        return new((nuint)pointer, value, Native.TlsAddress(), Native.TlsRead(), Environment.CurrentManagedThreadId);
    }
    private static void Churn()
    {
        var allocations = new byte[32][];
        for (var index = 0; index < allocations.Length; ++index) allocations[index] = new byte[8192 + index];
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); GC.KeepAlive(allocations);
    }
    private static long[] NewReport(int kind)
    {
        var count = Native.ReportCount(kind);
        if (count is < 1 or > 128) throw new InvalidOperationException("Unexpected native report schema.");
        return new long[count];
    }
    private static Dictionary<string, long> ReadReport(int kind, long[] values) => Enumerable.Range(0, values.Length)
        .ToDictionary(index => Utf8(Native.ReportName(kind, index)), index => values[index], StringComparer.Ordinal);
    private static string Utf8(nint pointer) => Marshal.PtrToStringUTF8(pointer) ?? throw new InvalidOperationException("Missing native descriptor.");
    private static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexStringLower(SHA256.HashData(stream)); }
    private static string Hex(nuint value) => "0x" + value.ToString("x");
    private static nint ResolveLibrary(string name, Assembly assembly, DllImportSearchPath? searchPath) => name switch
    { "host-contracts-probe" => helperHandle, "host-contracts-libc" => libcHandle, _ => 0 };
    private static void WriteMap(Utf8JsonWriter writer, string name, Dictionary<string, long> values)
    { writer.WriteStartObject(name); foreach (var pair in values) writer.WriteNumber(pair.Key, pair.Value); writer.WriteEndObject(); }
    private static void WritePoint(Utf8JsonWriter writer, string name, Point? point)
    {
        if (point is null) { writer.WriteNull(name); return; }
        writer.WriteStartObject(name); writer.WriteString("errnoTls", Hex(point.ErrnoTls)); writer.WriteNumber("errnoLater", point.Error);
        writer.WriteString("nativeMarkerTls", Hex(point.MarkerTls)); writer.WriteNumber("nativeMarker", point.Marker);
        writer.WriteNumber("managedThreadId", point.ManagedThreadId); writer.WriteEndObject();
    }
    private static void WriteCallback(Utf8JsonWriter writer, CallbackState state)
    {
        writer.WriteStartObject(state.Mode + "ManagedCallback");
        writer.WriteNumber("outerCount", state.OuterCount); writer.WriteNumber("nestedCount", state.NestedCount);
        writer.WriteNumber("outerCompleted", state.OuterCompleted); writer.WriteNumber("nestedCompleted", state.NestedCompleted);
        writer.WriteString("failure", state.Failure);
        WritePoint(writer, "outerBefore", state.OuterBefore); WritePoint(writer, "outerAfter", state.OuterAfter);
        WritePoint(writer, "nestedBefore", state.NestedBefore); WritePoint(writer, "nestedAfter", state.NestedAfter);
        writer.WriteNumber("nestedOpenReturn", state.NestedOpenReturn); writer.WriteNumber("nestedImmediateNativeErrno", state.NestedImmediateError);
        writer.WriteString("nestedNativeErrnoTls", Hex(state.NestedNativeTls));
        writer.WriteNumber("reentryReturn", state.ReentryReturn);
        if (state.Reentry is not null) WriteMap(writer, "nativeReentry", state.Reentry);
        writer.WriteEndObject();
    }
    private sealed record CheckResult(string Name, bool Passed);
    private sealed record Provider(string Symbol, nuint Address, string Path, nuint Base);
    private sealed record Point(nuint ErrnoTls, int Error, nuint MarkerTls, long Marker, int ManagedThreadId);
    private sealed class CallbackState(string mode)
    {
        public string Mode { get; } = mode;
        public int OuterCount, NestedCount, OuterCompleted, NestedCompleted;
        public Point? OuterBefore, OuterAfter, NestedBefore, NestedAfter;
        public int ReentryReturn, NestedOpenReturn, NestedImmediateError;
        public nuint NestedNativeTls;
        public Dictionary<string, long>? Reentry;
        public string? Failure;
    }

    private static class Native
    {
        private const string Helper = "host-contracts-probe", Libc = "host-contracts-libc";
        [DllImport(Helper, EntryPoint = "hc_abi_count", CallingConvention = CallingConvention.Cdecl)] internal static extern int AbiCount();
        [DllImport(Helper, EntryPoint = "hc_abi_name", CallingConvention = CallingConvention.Cdecl)] internal static extern nint AbiName(int index);
        [DllImport(Helper, EntryPoint = "hc_abi_value", CallingConvention = CallingConvention.Cdecl)] internal static extern long AbiValue(int index);
        [DllImport(Helper, EntryPoint = "hc_provider_count", CallingConvention = CallingConvention.Cdecl)] internal static extern int ProviderCount();
        [DllImport(Helper, EntryPoint = "hc_provider_symbol", CallingConvention = CallingConvention.Cdecl)] internal static extern nint ProviderSymbol(int index);
        [DllImport(Helper, EntryPoint = "hc_provider_address", CallingConvention = CallingConvention.Cdecl)] internal static extern nuint ProviderAddress(int index);
        [DllImport(Helper, EntryPoint = "hc_provider_path", CallingConvention = CallingConvention.Cdecl)] internal static extern nint ProviderPath(int index);
        [DllImport(Helper, EntryPoint = "hc_provider_base", CallingConvention = CallingConvention.Cdecl)] internal static extern nuint ProviderBase(int index);
        [DllImport(Helper, EntryPoint = "hc_report_count", CallingConvention = CallingConvention.Cdecl)] internal static extern int ReportCount(int kind);
        [DllImport(Helper, EntryPoint = "hc_report_name", CallingConvention = CallingConvention.Cdecl)] internal static extern nint ReportName(int kind, int index);
        [DllImport(Helper, EntryPoint = "hc_tls_address", CallingConvention = CallingConvention.Cdecl)] internal static extern nuint TlsAddress();
        [DllImport(Helper, EntryPoint = "hc_tls_read", CallingConvention = CallingConvention.Cdecl)] internal static extern long TlsRead();
        [DllImport(Helper, EntryPoint = "hc_tls_write", CallingConvention = CallingConvention.Cdecl)] internal static extern void TlsWrite(long value);
        [DllImport(Helper, EntryPoint = "hc_set_errno", CallingConvention = CallingConvention.Cdecl)] internal static extern void SetErrno(int value);
        [DllImport(Helper, EntryPoint = "hc_fail_capture", CallingConvention = CallingConvention.Cdecl)] internal static extern int FailCapture(out int capturedError, out nuint capturedTls);
        [DllImport(Helper, EntryPoint = "hc_protected_probe", CallingConvention = CallingConvention.Cdecl)] internal static extern int ProtectedProbe(delegate* unmanaged[Cdecl]<nint, int> callback, nint context, long* values, nuint count);
        [DllImport(Helper, EntryPoint = "hc_reentry_probe", CallingConvention = CallingConvention.Cdecl)] internal static extern int ReentryProbe(delegate* unmanaged[Cdecl]<nint, int> callback, nint context, int errorValue, long* values, nuint count);
        [DllImport(Helper, EntryPoint = "hc_thread_probe", CallingConvention = CallingConvention.Cdecl)] internal static extern int ThreadProbe(delegate* unmanaged[Cdecl]<nint, int> callback, nint context, long* values, nuint count);
        [DllImport(Helper, EntryPoint = "hc_mutex_probe", CallingConvention = CallingConvention.Cdecl)] internal static extern int MutexProbe(long* values, nuint count);
        // SetLastError deliberately false: this measures real TLS, not .NET's cached last error.
        [DllImport(Libc, EntryPoint = "open", CallingConvention = CallingConvention.Cdecl, SetLastError = false)] internal static extern int RawOpen(byte* path, int flags);
        [DllImport(Libc, EntryPoint = "__errno_location", CallingConvention = CallingConvention.Cdecl, SetLastError = false)] internal static extern int* ErrnoLocation();
    }
}
