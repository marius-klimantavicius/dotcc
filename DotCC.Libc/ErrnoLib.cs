#nullable enable

using System.Runtime.CompilerServices;

namespace DotCC.Libc;

/// <summary>
/// C <c>&lt;errno.h&gt;</c> surface plus the <c>strerror</c> / <c>perror</c>
/// reporting pair (declared in <c>&lt;string.h&gt;</c> / <c>&lt;stdio.h&gt;</c>
/// respectively, but implemented here next to the error-number table).
/// </summary>
/// <remarks>
/// <para>
/// <c>errno</c> is a thread-local <c>int</c> exposed as a settable static
/// property — emitted user code references the bare name <c>errno</c> which
/// binds to <see cref="errno"/> through <c>using static Libc;</c>, so both
/// <c>errno = 0;</c> and <c>if (errno == ERANGE)</c> work. Real C makes
/// <c>errno</c> a macro expanding to a thread-local lvalue; the property is the
/// idiomatic .NET equivalent (per-thread storage, no shared global).
/// </para>
/// <para>
/// The numeric values match the Linux/glibc <c>asm-generic</c> assignments,
/// consistent with dotcc's LP64 / Linux-leaning model. They are mirrored as
/// numeric <c>#define</c>s in <c>&lt;errno.h&gt;</c> so user code and this
/// switch agree.
/// </para>
/// </remarks>
public static unsafe partial class Libc
{
    private static ref int _errno => ref RuntimeThread.Errno;

    /// <summary><c>errno</c> — the thread-local error indicator. Settable and
    /// readable; initialised to 0 per thread.</summary>
    public static int errno
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _errno;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _errno = value;
    }

    // Error numbers (Linux/glibc asm-generic values). Mirrored in <errno.h>.
    public const int EPERM = 1;     // Operation not permitted
    public const int ENOENT = 2;    // No such file or directory
    public const int ESRCH = 3;     // No such process
    public const int EINTR = 4;     // Interrupted system call
    public const int EIO = 5;       // Input/output error
    public const int ENXIO = 6;     // No such device or address
    public const int E2BIG = 7;     // Argument list too long
    public const int ENOEXEC = 8;   // Exec format error
    public const int EBADF = 9;     // Bad file descriptor
    public const int ECHILD = 10;   // No child processes
    public const int EAGAIN = 11;   // Resource temporarily unavailable
    public const int ENOMEM = 12;   // Cannot allocate memory
    public const int EACCES = 13;   // Permission denied
    public const int EFAULT = 14;   // Bad address
    public const int EBUSY = 16;    // Device or resource busy
    public const int EEXIST = 17;   // File exists
    public const int EXDEV = 18;    // Invalid cross-device link
    public const int ENODEV = 19;   // No such device
    public const int ENOTDIR = 20;  // Not a directory
    public const int EISDIR = 21;   // Is a directory
    public const int EINVAL = 22;   // Invalid argument
    public const int ENFILE = 23;   // Too many open files in system
    public const int EMFILE = 24;   // Too many open files
    public const int ENOTTY = 25;   // Inappropriate ioctl for device
    public const int EFBIG = 27;    // File too large
    public const int ENOSPC = 28;   // No space left on device
    public const int ESPIPE = 29;   // Illegal seek
    public const int EROFS = 30;    // Read-only file system
    public const int EMLINK = 31;   // Too many links
    public const int EPIPE = 32;    // Broken pipe
    public const int EDOM = 33;     // Numerical argument out of domain   (C std)
    public const int ERANGE = 34;   // Numerical result out of range      (C std)
    public const int EDEADLK = 35;  // Resource deadlock avoided
    public const int ENOTSUP = 95;  // Operation not supported
    public const int EILSEQ = 84;   // Invalid or incomplete multibyte/wide char (C std)
    // ---- socket/network errnos (Linux/glibc asm-generic values; SocketLib) ----
    public const int ENOTSOCK = 88;        // Socket operation on non-socket
    public const int EMSGSIZE = 90;        // Message too long
    public const int EPROTONOSUPPORT = 93; // Protocol not supported
    public const int EOPNOTSUPP = 95;      // Operation not supported on socket
    public const int EAFNOSUPPORT = 97;    // Address family not supported
    public const int EADDRINUSE = 98;      // Address already in use
    public const int EADDRNOTAVAIL = 99;   // Cannot assign requested address
    public const int ENETUNREACH = 101;    // Network is unreachable
    public const int ECONNABORTED = 103;   // Software caused connection abort
    public const int ECONNRESET = 104;     // Connection reset by peer
    public const int ENOBUFS = 105;        // No buffer space available
    public const int EISCONN = 106;        // Transport endpoint is already connected
    public const int ENOTCONN = 107;       // Transport endpoint is not connected
    public const int ETIMEDOUT = 110;      // Connection timed out
    public const int ECANCELED = 125;      // Operation canceled
    public const int ETIME = 62;           // Timer expired
    public const int EPROTO = 71;          // Protocol error
    public const int EOVERFLOW = 75;       // Value too large for defined data type
    public const int EPROTOTYPE = 91;      // Protocol wrong type for socket
    public const int ENOPROTOOPT = 92;     // Protocol not available
    public const int EOWNERDEAD = 130;     // Owner died
    public const int ECONNREFUSED = 111;   // Connection refused
    public const int EHOSTUNREACH = 113;   // No route to host
    public const int EINPROGRESS = 115;    // Operation now in progress

    public const int ETXTBSY = 26; // Text file busy
    public const int ENAMETOOLONG = 36; // File name too long
    public const int ENOTEMPTY = 39; // Directory not empty
    public const int ELOOP = 40; // Too many levels of symbolic links
    public const int ENODATA = 61; // No data available
    public const int ENOLINK = 67; // Link has been severed
    public const int ENETRESET = 102; // Network dropped connection on reset
    public const int EWOULDBLOCK = EAGAIN;

    /// <summary>
    /// <c>strerror(errnum)</c> — map an error number to a human-readable message
    /// pointer. The returned <c>byte*</c> points into the rooted pinned literal pool
    /// while the owning library remains alive — matching C's "pointer to static,
    /// may be reused" contract without allocating after pool initialization.
    /// </summary>
    public static byte* strerror(int errnum) => errnum switch
    {
        0       => (LiteralPool.Pointer + LiteralPool.ErrorSuccess),
        EPERM   => (LiteralPool.Pointer + LiteralPool.ErrorEPERM),
        ENOENT  => (LiteralPool.Pointer + LiteralPool.ErrorENOENT),
        ESRCH   => (LiteralPool.Pointer + LiteralPool.ErrorESRCH),
        EINTR   => (LiteralPool.Pointer + LiteralPool.ErrorEINTR),
        EIO     => (LiteralPool.Pointer + LiteralPool.ErrorEIO),
        ENXIO   => (LiteralPool.Pointer + LiteralPool.ErrorENXIO),
        E2BIG   => (LiteralPool.Pointer + LiteralPool.ErrorE2BIG),
        ENOEXEC => (LiteralPool.Pointer + LiteralPool.ErrorENOEXEC),
        EBADF   => (LiteralPool.Pointer + LiteralPool.ErrorEBADF),
        ECHILD  => (LiteralPool.Pointer + LiteralPool.ErrorECHILD),
        EAGAIN  => (LiteralPool.Pointer + LiteralPool.ErrorEAGAIN),
        ENOMEM  => (LiteralPool.Pointer + LiteralPool.ErrorENOMEM),
        EACCES  => (LiteralPool.Pointer + LiteralPool.ErrorEACCES),
        EFAULT  => (LiteralPool.Pointer + LiteralPool.ErrorEFAULT),
        EBUSY   => (LiteralPool.Pointer + LiteralPool.ErrorEBUSY),
        EEXIST  => (LiteralPool.Pointer + LiteralPool.ErrorEEXIST),
        EXDEV   => (LiteralPool.Pointer + LiteralPool.ErrorEXDEV),
        ENODEV  => (LiteralPool.Pointer + LiteralPool.ErrorENODEV),
        ENOTDIR => (LiteralPool.Pointer + LiteralPool.ErrorENOTDIR),
        EISDIR  => (LiteralPool.Pointer + LiteralPool.ErrorEISDIR),
        EINVAL  => (LiteralPool.Pointer + LiteralPool.ErrorEINVAL),
        ENFILE  => (LiteralPool.Pointer + LiteralPool.ErrorENFILE),
        EMFILE  => (LiteralPool.Pointer + LiteralPool.ErrorEMFILE),
        ENOTTY  => (LiteralPool.Pointer + LiteralPool.ErrorENOTTY),
        EFBIG   => (LiteralPool.Pointer + LiteralPool.ErrorEFBIG),
        ENOSPC  => (LiteralPool.Pointer + LiteralPool.ErrorENOSPC),
        ESPIPE  => (LiteralPool.Pointer + LiteralPool.ErrorESPIPE),
        EROFS   => (LiteralPool.Pointer + LiteralPool.ErrorEROFS),
        EMLINK  => (LiteralPool.Pointer + LiteralPool.ErrorEMLINK),
        EPIPE   => (LiteralPool.Pointer + LiteralPool.ErrorEPIPE),
        EDOM    => (LiteralPool.Pointer + LiteralPool.ErrorEDOM),
        ERANGE  => (LiteralPool.Pointer + LiteralPool.ErrorERANGE),
        EDEADLK => (LiteralPool.Pointer + LiteralPool.ErrorEDEADLK),
        ENOTSUP => (LiteralPool.Pointer + LiteralPool.ErrorENOTSUP),
        ETIMEDOUT => (LiteralPool.Pointer + LiteralPool.ErrorETIMEDOUT),
        ECANCELED => (LiteralPool.Pointer + LiteralPool.ErrorECANCELED),
        ETIME => (LiteralPool.Pointer + LiteralPool.ErrorETIME),
        EPROTO => (LiteralPool.Pointer + LiteralPool.ErrorEPROTO),
        EOVERFLOW => (LiteralPool.Pointer + LiteralPool.ErrorEOVERFLOW),
        EPROTOTYPE => (LiteralPool.Pointer + LiteralPool.ErrorEPROTOTYPE),
        ENOPROTOOPT => (LiteralPool.Pointer + LiteralPool.ErrorENOPROTOOPT),
        EOWNERDEAD => (LiteralPool.Pointer + LiteralPool.ErrorEOWNERDEAD),
        EILSEQ  => (LiteralPool.Pointer + LiteralPool.ErrorEILSEQ),
        ETXTBSY => (LiteralPool.Pointer + LiteralPool.ErrorETXTBSY),
        ENAMETOOLONG => (LiteralPool.Pointer + LiteralPool.ErrorENAMETOOLONG),
        ENOTEMPTY => (LiteralPool.Pointer + LiteralPool.ErrorENOTEMPTY),
        ELOOP => (LiteralPool.Pointer + LiteralPool.ErrorELOOP),
        ENODATA => (LiteralPool.Pointer + LiteralPool.ErrorENODATA),
        ENOLINK => (LiteralPool.Pointer + LiteralPool.ErrorENOLINK),
        ENETRESET => (LiteralPool.Pointer + LiteralPool.ErrorENETRESET),
        _       => (LiteralPool.Pointer + LiteralPool.ErrorUnknown),
    };

    /// <summary>
    /// <c>perror(s)</c> — write <c><paramref name="s"/>: &lt;message&gt;</c>
    /// (then a newline) for the current <see cref="errno"/> to
    /// <see cref="stderr"/>. A null or empty <paramref name="s"/> prints just the
    /// message (matches C, which omits the prefix and separator).
    /// </summary>
    public static void perror(byte* s)
    {
        if (s != null && *s != 0)
        {
            fputs(s, stderr);
            WriterFor(stderr).Write(": ");
        }
        fputs(strerror(errno), stderr);
        WriteByteTo(stderr, (byte)'\n');
    }
}
