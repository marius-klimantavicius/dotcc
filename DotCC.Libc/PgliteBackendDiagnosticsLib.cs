#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Capture real native host frames. JIT managed frames need not
    /// be visible to the system unwinder; these are native diagnostic addresses.</summary>
    public static int backtrace(void* buffer, int capacity)
    {
        if (capacity < 0) { errno = EINVAL; return 0; }
        if (buffer == null && capacity != 0) { errno = EFAULT; return 0; }
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return 0; }
        try { return BackendNativeBacktrace(buffer, capacity); }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return 0; }
        catch (DllNotFoundException) { errno = ENOSYS; return 0; }
    }

    /// <summary>Copy system symbolization into one guest-owned malloc block,
    /// preserving the C free() contract and normal runtime owner cleanup.</summary>
    public static byte** backtrace_symbols(void* buffer, int count)
    {
        if (count < 0) { errno = EINVAL; return null; }
        if (buffer == null && count != 0) { errno = EFAULT; return null; }
        if (!OperatingSystem.IsLinux()) { errno = ENOTSUP; return null; }
        byte** native = null;
        try
        {
            native = BackendNativeBacktraceSymbols(buffer, count);
            if (native == null) { errno = Marshal.GetLastPInvokeError(); return null; }
            ulong bytes = checked((ulong)count * (ulong)sizeof(byte*));
            for (int i = 0; i < count; i++) bytes = checked(bytes + (ulong)strlen(native[i]) + 1);
            byte** result = (byte**)malloc(bytes);
            if (result == null) { errno = ENOMEM; return null; }
            byte* destination = (byte*)result + (ulong)count * (ulong)sizeof(byte*);
            for (int i = 0; i < count; i++)
            {
                ulong length = (ulong)strlen(native[i]) + 1;
                result[i] = destination;
                NativeMemory.Copy(native[i], destination, (nuint)length);
                destination += length;
            }
            return result;
        }
        catch (OverflowException) { errno = ENOMEM; return null; }
        catch (EntryPointNotFoundException) { errno = ENOSYS; return null; }
        catch (DllNotFoundException) { errno = ENOSYS; return null; }
        finally { if (native != null) BackendNativeFree(native); }
    }

    public static void backtrace_symbols_fd(void* buffer, int count, int descriptor)
    {
        byte** symbols = backtrace_symbols(buffer, count);
        if (symbols == null) return;
        try
        {
            for (int i = 0; i < count; i++)
            {
                byte* text = symbols[i];
                ulong remaining = (ulong)strlen(text);
                while (remaining != 0)
                {
                    long written = write(descriptor, text, remaining);
                    if (written < 0 && errno == EINTR) continue;
                    if (written <= 0) return;
                    text += written; remaining -= (ulong)written;
                }
                byte newline = 10;
                long result;
                do { result = write(descriptor, &newline, 1); } while (result < 0 && errno == EINTR);
                if (result != 1) return;
            }
        }
        finally { free(symbols); }
    }

    // The shared syslog service explicitly reports ENOTSUP; it owns no native
    // host registration to close. Preserve that explicit unsupported contract.
    public static void closelog() => UnsupportedPosix();

    [DllImport("libc", EntryPoint = "backtrace")]
    private static extern int BackendNativeBacktrace(void* buffer, int capacity);
    [DllImport("libc", EntryPoint = "backtrace_symbols", SetLastError = true)]
    private static extern byte** BackendNativeBacktraceSymbols(void* buffer, int count);
    [DllImport("libc", EntryPoint = "free")]
    private static extern void BackendNativeFree(void* pointer);
}
