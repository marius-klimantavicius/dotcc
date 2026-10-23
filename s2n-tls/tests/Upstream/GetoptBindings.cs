using System.Runtime.InteropServices;

namespace Managed.Security.S2n;

// Test-only original CLI execution service. No TLS or crypto operations cross
// this boundary. GNU libc owns parsing, argv permutation and cursor state.
public static unsafe partial class S2nTls
{
    private static readonly nint GetoptLibc = NativeLibrary.Load("libc.so.6");
    private static readonly nint GetoptArg = NativeLibrary.GetExport(GetoptLibc, "optarg");
    private static readonly nint GetoptIndex = NativeLibrary.GetExport(GetoptLibc, "optind");
    private static readonly nint GetoptError = NativeLibrary.GetExport(GetoptLibc, "opterr");
    private static readonly nint GetoptOption = NativeLibrary.GetExport(GetoptLibc, "optopt");

    [DllImport("libc.so.6", EntryPoint = "getopt_long", CallingConvention = CallingConvention.Cdecl)]
    private static extern int NativeGetoptLong(int argc, byte** argv, byte* shortOptions, void* longOptions, int* longIndex);

    public static int UpstreamGetoptLong(int argc, byte** argv, byte* shortOptions, void* longOptions, int* longIndex)
        => NativeGetoptLong(argc, argv, shortOptions, longOptions, longIndex);
    public static byte** UpstreamOptarg() => (byte**)GetoptArg;
    public static int* UpstreamOptind() => (int*)GetoptIndex;
    public static int* UpstreamOpterr() => (int*)GetoptError;
    public static int* UpstreamOptopt() => (int*)GetoptOption;

    public static void ResetUpstreamGetopt()
    {
        *UpstreamOptind() = 0; // GNU reset includes ordering and next-character state.
        *UpstreamOptarg() = null;
        *UpstreamOpterr() = 1;
        *UpstreamOptopt() = '?';
    }

#if UPSTREAM_HOST_SERVICES
    [DllImport("libc.so.6", EntryPoint = "signal", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    private static extern nint NativeSignal(int signal, nint disposition);

    // Original CLIs only request SIGPIPE/SIG_IGN. Native code must never call
    // an ordinary managed function pointer as an unmanaged signal handler.
    public static delegate*<int, void> UpstreamSignal(int signal, delegate*<int, void> disposition)
    {
        if (signal != 13 || ((nint)disposition != 0 && (nint)disposition != 1))
        {
            Libc.errno = 22;
            return (delegate*<int, void>)(nint)(-1);
        }
        nint previous = NativeSignal(signal, (nint)disposition);
        if (previous == -1) Libc.errno = Marshal.GetLastPInvokeError();
        return (delegate*<int, void>)previous;
    }
#endif
}
