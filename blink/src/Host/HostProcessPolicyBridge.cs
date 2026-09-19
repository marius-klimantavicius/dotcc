using Managed.Emulation.Host;

namespace Managed.Emulation;

#if BLINK_FULL_CORE
public static partial class BlinkCore
#else
public static partial class Blink
#endif
{
    private static int ProcessPolicyError(int error) { Libc.errno = error; return -1; }
    private static int ProcessCreationDenied() => ProcessPolicyError(identity == null ? 19 : 38);
    private static int ProcessMutationDenied() => ProcessPolicyError(identity == null ? 19 : 1);
    // The profile owns exactly one process and has no creation/exec primitive.
    public static int blink_host_fork() => ProcessCreationDenied();
    public static unsafe int blink_host_execv(byte* path, byte** arguments) => ProcessCreationDenied();
    public static unsafe int blink_host_execve(byte* path, byte** arguments, byte** variables) => ProcessCreationDenied();
    public static unsafe int blink_host_execvp(byte* path, byte** arguments) => ProcessCreationDenied();
    public static unsafe int blink_host_waitpid(int process, int* status, int options)
    {
        if (identity == null) return ProcessPolicyError(19);
        if ((options & ~(1 | 2 | 8)) != 0) return ProcessPolicyError(22);
        return ProcessPolicyError(10); // This process namespace has no children.
    }
    // Identity and process-group/session membership are immutable in this profile.
    public static int blink_host_setuid(uint value) => ProcessMutationDenied();
    public static int blink_host_seteuid(uint value) => ProcessMutationDenied();
    public static int blink_host_setgid(uint value) => ProcessMutationDenied();
    public static int blink_host_setegid(uint value) => ProcessMutationDenied();
    public static int blink_host_setpgid(int process, int group) => ProcessMutationDenied();
    public static int blink_host_setsid() => ProcessMutationDenied();
}
