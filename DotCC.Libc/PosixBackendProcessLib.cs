#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public sealed partial class RuntimeContext
    {
        internal readonly object CreationMaskGate = new();
        internal uint? CreationMask;
    }

    // Read the host's mask without the process-wide, mutating umask(2) query.
    // A missing procfs is an explicit unsupported host capability, not a
    // guessed permission policy. Each owner captures its initial mask once.
    private static uint ReadBackendHostCreationMask()
    {
        if (!OperatingSystem.IsLinux()) throw new PlatformNotSupportedException();
        foreach (string line in File.ReadLines("/proc/self/status"))
        {
            if (!line.StartsWith("Umask:", StringComparison.Ordinal)) continue;
            string value = line[6..].Trim();
            uint mask = 0;
            if (value.Length == 0) break;
            foreach (char digit in value)
            {
                if (digit < '0' || digit > '7' || mask > 0x3f)
                    throw new IOException("Invalid host creation mask in /proc/self/status.");
                mask = (mask << 3) | (uint)(digit - '0');
            }
            return mask;
        }
        throw new PlatformNotSupportedException("Linux procfs does not expose the host creation mask.");
    }

    internal static uint EffectiveCreationMask
    {
        get
        {
            var owner = RuntimeState;
            lock (owner.CreationMaskGate) return owner.CreationMask ??= ReadBackendHostCreationMask();
        }
    }

    /// <summary>Creation hooks apply the guest owner's mask to permission bits.
    /// Existing objects and explicit chmod/fchmod must not call this hook.</summary>
    internal static uint ApplyCreationMode(uint mode) => mode & ~EffectiveCreationMask;

    internal static uint BackendUmask(uint mask)
    {
        var owner = RuntimeState;
        lock (owner.CreationMaskGate)
        {
            try
            {
                uint previous = owner.CreationMask ?? ReadBackendHostCreationMask();
                owner.CreationMask = mask & 0x1ff;
                return previous;
            }
            catch (PlatformNotSupportedException) { errno = ENOTSUP; }
            catch (UnauthorizedAccessException) { errno = EACCES; }
            catch (IOException) { errno = EIO; }
            return uint.MaxValue;
        }
    }

    /// <summary>Query real Linux LP64 process resource limits. Guest storage is
    /// written only on success; the host's limits are never changed.</summary>
    internal static int BackendGetrlimit(int resource, void* limit)
    {
        if (limit == null) { errno = EFAULT; return -1; }
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { errno = ENOTSUP; return -1; }
        ulong* result = stackalloc ulong[2];
        try
        {
            if (BackendNativeGetrlimit(resource, result) != 0)
            { errno = Marshal.GetLastPInvokeError(); return -1; }
            ((ulong*)limit)[0] = result[0];
            ((ulong*)limit)[1] = result[1];
            return 0;
        }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return -1; }
        catch (DllNotFoundException) { errno = ENOSYS; return -1; }
    }

    [DllImport("libc", EntryPoint = "getrlimit", SetLastError = true)]
    private static extern int BackendNativeGetrlimit(int resource, void* limit);
}
