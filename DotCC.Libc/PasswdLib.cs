#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public static uint getuid() => OperatingSystem.IsLinux() ? PosixGetuid() : UnsupportedIdentity();
    public static uint geteuid() => OperatingSystem.IsLinux() ? PosixGeteuid() : UnsupportedIdentity();
    public static uint getgid() => OperatingSystem.IsLinux() ? PosixGetgid() : UnsupportedIdentity();
    public static uint getegid() => OperatingSystem.IsLinux() ? PosixGetegid() : UnsupportedIdentity();
    private static uint UnsupportedIdentity() { errno = ENOTSUP; return uint.MaxValue; }
    [DllImport("libc", EntryPoint = "getuid")]
    private static extern uint PosixGetuid();
    [DllImport("libc", EntryPoint = "geteuid")]
    private static extern uint PosixGeteuid();
    [DllImport("libc", EntryPoint = "getgid")]
    private static extern uint PosixGetgid();
    [DllImport("libc", EntryPoint = "getegid")]
    private static extern uint PosixGetegid();

    // Linux LP64/glibc ABI. Only the _r OS functions fill this storage; the
    // public C record is independently emitted from pwd.h and ABI-tested.
    #pragma warning disable CS0649 // Written by the native reentrant NSS service.
    private struct LinuxPasswd
    {
        public byte* Name;
        public byte* Password;
        public uint Uid;
        public uint Gid;
        public byte* Gecos;
        public byte* Directory;
        public byte* Shell;
    }
    #pragma warning restore CS0649

    private sealed class PasswdStorage
    {
        internal byte* Block;
        internal ulong Capacity;
    }

    /// <summary>Returned record and strings are borrowed, runtime/thread-local
    /// storage, overwritten by a subsequent lookup on that thread. Native NSS
    /// resolution uses reentrant calls, so concurrent workers never share libc's
    /// static passwd record. Context disposal releases all worker buffers.</summary>
    public static void* getpwuid(uint uid) => LookupPasswd(uid, null);

    /// <summary>Caller-owned reentrant NSS lookup on Linux LP64. The return
    /// value is the numeric error, not -1/guest errno. A missing entry returns
    /// 0 and NULL. Native strings are borrowed according to NSS's contract;
    /// this function neither copies, frees nor assumes ownership of them.</summary>
    public static int getpwuid_r(uint uid, void* record, byte* buffer, ulong length, void* result)
    {
        // Struct** cannot implicitly convert to void** in emitted C#. Accept
        // its untyped address and perform the ABI cast only inside the runtime.
        if (result == null) return EFAULT;
        *(void**)result = null;
        if (record == null || buffer == null) return EFAULT;
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return ENOTSUP;
        return PosixGetpwuidR(uid, (LinuxPasswd*)record, buffer, length, (LinuxPasswd**)result);
    }
    public static void* getpwnam(byte* name)
    {
        if (name == null) { errno = EFAULT; return null; }
        // name may point into our previous result; _r output/growth must not
        // overwrite or move the lookup key while NSS is reading it.
        try
        {
            byte[] key = new ReadOnlySpan<byte>(name, checked(strlen(name) + 1)).ToArray();
            fixed (byte* copy = key) return LookupPasswd(0, copy);
        }
        catch (OutOfMemoryException) { errno = ENOMEM; return null; }
    }

    private static void* LookupPasswd(uint uid, byte* name)
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) { errno = ENOTSUP; return null; }
        var paths = OwnedPaths;
        // The dictionary is owner-scoped; one buffer per actual managed thread.
        int thread = Environment.CurrentManagedThreadId;
        lock (paths.Sync)
        {
            if (!paths.Passwd.TryGetValue(thread, out var storage))
                paths.Passwd.Add(thread, storage = new());
            ulong capacity = storage.Capacity == 0 ? 1024 : storage.Capacity;
            while (true)
            {
                if (capacity != storage.Capacity)
                {
                    byte* replacement;
                    try { replacement = (byte*)NativeMemory.Realloc(storage.Block, checked((nuint)(capacity + (ulong)sizeof(LinuxPasswd)))); }
                    catch (OutOfMemoryException) { errno = ENOMEM; return null; }
                    storage.Block = replacement;
                    storage.Capacity = capacity;
                }
                LinuxPasswd* result = null;
                var record = (LinuxPasswd*)storage.Block;
                int error = name == null
                    ? PosixGetpwuidR(uid, record, storage.Block + sizeof(LinuxPasswd), capacity, &result)
                    : PosixGetpwnamR(name, record, storage.Block + sizeof(LinuxPasswd), capacity, &result);
                if (error == ERANGE)
                {
                    if (capacity > (ulong)nint.MaxValue / 2 - (ulong)sizeof(LinuxPasswd)) { errno = ENOMEM; return null; }
                    capacity *= 2;
                    continue;
                }
                if (error != 0) { errno = error; return null; }
                // Not found returns NULL without changing the caller's errno.
                return result;
            }
        }
    }

    [DllImport("libc", EntryPoint = "getpwuid_r")]
    private static extern int PosixGetpwuidR(uint uid, LinuxPasswd* record, byte* buffer, ulong length, LinuxPasswd** result);
    [DllImport("libc", EntryPoint = "getpwnam_r")]
    private static extern int PosixGetpwnamR(byte* name, LinuxPasswd* record, byte* buffer, ulong length, LinuxPasswd** result);
}
