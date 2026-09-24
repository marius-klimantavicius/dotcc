#nullable enable

namespace DotCC.Libc;

/// <summary>
/// The minimal POSIX <c>&lt;unistd.h&gt;</c> surface dotcc's synthetic header
/// declares — just enough for portable Unix C (chibi-scheme's non-<c>_WIN32</c>
/// path) to compile and behave honestly.
/// </summary>
public static unsafe partial class Libc
{
    /// <summary>POSIX <c>usleep</c> — suspend for at least <paramref name="usec"/>
    /// microseconds. <c>Thread.Sleep</c> has millisecond granularity; sub-ms
    /// requests round up to 1ms (a sleep may always be longer than asked).</summary>
    public static int usleep(uint usec)
    {
        global::System.Threading.Thread.Sleep(usec == 0 ? 0 : (int)global::System.Math.Max(1, usec / 1000));
        return 0;
    }

    /// <summary>POSIX <c>isatty</c> — 1 when the standard stream for
    /// <paramref name="fd"/> (0/1/2) is attached to a console, 0 otherwise
    /// (including unknown fds; dotcc has no other fd table).</summary>
    public static int isatty(int fd) => fd switch
    {
        0 => global::System.Console.IsInputRedirected ? 0 : 1,
        1 => global::System.Console.IsOutputRedirected ? 0 : 1,
        2 => global::System.Console.IsErrorRedirected ? 0 : 1,
        _ => 0,
    };

}
