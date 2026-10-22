#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public sealed partial class RuntimeContext
    {
        internal readonly object BackendHostGate = new();
        internal readonly HashSet<nint> BackendInterfaceLists = new();
    }

    /// <summary>Return the real Linux LP64 interface list. Only this runtime
    /// owner may free it, and owner disposal releases abandoned lists.</summary>
    public static int getifaddrs(void* result)
    {
        if (result == null) { errno = EFAULT; return -1; }
        *(void**)result = null;
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { errno = ENOTSUP; return -1; }
        void* list = null;
        try
        {
            if (BackendNativeGetifaddrs(&list) != 0)
            { errno = Marshal.GetLastPInvokeError(); return -1; }
            if (list != null)
            {
                var owner = RuntimeState;
                lock (owner.BackendHostGate) owner.BackendInterfaceLists.Add((nint)list);
            }
            *(void**)result = list;
            return 0;
        }
        catch (OutOfMemoryException)
        { if (list != null) BackendNativeFreeifaddrs(list); errno = ENOMEM; return -1; }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return -1; }
        catch (DllNotFoundException) { errno = ENOSYS; return -1; }
    }

    public static void freeifaddrs(void* list)
    {
        if (list == null) return;
        var owner = RuntimeState;
        lock (owner.BackendHostGate)
        {
            if (!owner.BackendInterfaceLists.Remove((nint)list)) { errno = EINVAL; return; }
            BackendNativeFreeifaddrs(list);
        }
    }

    // Called by RuntimeContext.Dispose before descriptor and heap disposal,
    // after it has established that no guest bindings/workers remain active.
    private static void DisposeBackendHostServices(RuntimeContext owner)
    {
        lock (owner.BackendHostGate)
        {
            foreach (nint list in owner.BackendInterfaceLists) BackendNativeFreeifaddrs((void*)list);
            owner.BackendInterfaceLists.Clear();
        }
    }

    [DllImport("libc", EntryPoint = "getifaddrs", SetLastError = true)]
    private static extern int BackendNativeGetifaddrs(void** result);
    [DllImport("libc", EntryPoint = "freeifaddrs")]
    private static extern void BackendNativeFreeifaddrs(void* list);
}
