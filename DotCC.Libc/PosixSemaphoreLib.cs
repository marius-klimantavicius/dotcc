#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    internal sealed class PosixSemaphore
    {
        internal readonly nint Native;
        internal int Users;
        internal PosixSemaphore(nint native) { Native = native; }
    }
    public sealed partial class RuntimeContext
    {
        internal readonly object PosixSemaphoreGate = new();
        internal readonly Dictionary<nint, PosixSemaphore> posixSemaphores = new();
    }

    public static int sem_init(void* address, int shared, uint value)
    {
        if (address == null) { errno = EFAULT; return -1; }
        if (((nuint)address & 7) != 0 || value > int.MaxValue) { errno = EINVAL; return -1; }
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return -1; }
        var owner = RuntimeState;
        lock (owner.PosixSemaphoreGate)
        {
            if (owner.posixSemaphores.ContainsKey((nint)address)) { errno = EBUSY; return -1; }
            void* storage = NativeMemory.AllocZeroed(32);
            if (storage == null) { errno = ENOMEM; return -1; }
            int result = HostSemInit(storage, shared, value);
            if (result != 0) { errno = Marshal.GetLastPInvokeError(); NativeMemory.Free(storage); return -1; }
            owner.posixSemaphores.Add((nint)address, new PosixSemaphore((nint)storage));
            return 0;
        }
    }
    public static int sem_destroy(void* address)
    {
        var owner = RuntimeState;
        lock (owner.PosixSemaphoreGate)
        {
            if (!owner.posixSemaphores.TryGetValue((nint)address, out var item)) { errno = EINVAL; return -1; }
            if (item.Users != 0) { errno = EBUSY; return -1; }
            int result = HostSemDestroy((void*)item.Native);
            if (result != 0) { errno = Marshal.GetLastPInvokeError(); return -1; }
            owner.posixSemaphores.Remove((nint)address);
            NativeMemory.Free((void*)item.Native);
            return 0;
        }
    }
    public static int sem_wait(void* address) => SemaphoreOperation(address, 0, null);
    public static int sem_trywait(void* address) => SemaphoreOperation(address, 1, null);
    public static int sem_post(void* address) => SemaphoreOperation(address, 2, null);
    public static int sem_getvalue(void* address, int* value)
    {
        if (value == null) { errno = EFAULT; return -1; }
        return SemaphoreOperation(address, 3, value);
    }
    private static int SemaphoreOperation(void* address, int operation, int* value)
    {
        var owner = RuntimeState;
        PosixSemaphore item;
        lock (owner.PosixSemaphoreGate)
        {
            if (!owner.posixSemaphores.TryGetValue((nint)address, out item!)) { errno = EINVAL; return -1; }
            item.Users++;
        }
        try
        {
            int result = operation switch
            {
                0 => HostSemWait((void*)item.Native),
                1 => HostSemTryWait((void*)item.Native),
                2 => HostSemPost((void*)item.Native),
                _ => HostSemGetValue((void*)item.Native, value)
            };
            if (result != 0) errno = Marshal.GetLastPInvokeError();
            return result;
        }
        finally { lock (owner.PosixSemaphoreGate) item.Users--; }
    }
    private static void DisposePosixSemaphores(RuntimeContext owner)
    {
        lock (owner.PosixSemaphoreGate)
        {
            foreach (var item in owner.posixSemaphores.Values)
            {
                HostSemDestroy((void*)item.Native);
                NativeMemory.Free((void*)item.Native);
            }
            owner.posixSemaphores.Clear();
        }
    }
    [DllImport("libc", EntryPoint = "sem_init", SetLastError = true)]
    private static extern int HostSemInit(void* storage, int shared, uint value);
    [DllImport("libc", EntryPoint = "sem_destroy", SetLastError = true)]
    private static extern int HostSemDestroy(void* storage);
    [DllImport("libc", EntryPoint = "sem_wait", SetLastError = true)]
    private static extern int HostSemWait(void* storage);
    [DllImport("libc", EntryPoint = "sem_trywait", SetLastError = true)]
    private static extern int HostSemTryWait(void* storage);
    [DllImport("libc", EntryPoint = "sem_post", SetLastError = true)]
    private static extern int HostSemPost(void* storage);
    [DllImport("libc", EntryPoint = "sem_getvalue", SetLastError = true)]
    private static extern int HostSemGetValue(void* storage, int* value);
}
