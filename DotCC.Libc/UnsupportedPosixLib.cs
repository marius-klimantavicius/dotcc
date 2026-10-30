#nullable enable
using System;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // These process-wide services have no managed owner implementation. Fail
    // explicitly; never register callbacks or mutate the containing process.
    private static int UnsupportedPosix() { errno = ENOTSUP; return -1; }
    public static int getrlimit(int resource, void* limit) => BackendGetrlimit(resource, limit);
    public static int setrlimit(int resource, void* limit) => UnsupportedPosix();
    public static int setitimer(int which, void* value, void* previous) => UnsupportedPosix();
    public static int execve(byte* path, byte** arguments, byte** environment) => UnsupportedPosix();
    public static int setsid() => UnsupportedPosix();
    public static uint umask(uint mask) => BackendUmask(mask);
    // Installing/dispatching POSIX signal handlers is outside the managed
    // runtime contract. A pause must fail visibly, never report fake wakeup.
    public static int pause() => UnsupportedPosix();
    public static int ioctl(int descriptor, ulong request, params ReadOnlySpan<VaArg> arguments)
    { errno = SlotByFd(descriptor) is null ? EBADF : ENOTTY; return -1; }
    public static int dladdr(void* address, void* information) => 0;
    public static void* getgrnam(byte* name) { UnsupportedPosix(); return null; }
    public static void openlog(byte* identity, int options, int facility) => UnsupportedPosix();
    public static void syslog(int priority, byte* format, params ReadOnlySpan<VaArg> arguments) => UnsupportedPosix();
    // Calendar queries use TimeZoneInfo directly; process TZ reconfiguration is unsupported.
    public static void tzset() => UnsupportedPosix();
}
