using System.Runtime.InteropServices;
using Raw = Managed.Security.S2n.S2nTls;

// Authored executable entry. The upstream main and all assertion macros remain
// translated from their original C, rather than being rewritten as C# tests.
internal static unsafe class HarnessMain
{
    private static int Main(string[] arguments)
    {
#if UPSTREAM_EXECUTION_IDENTITY
        if (arguments is ["--identity"]) { ManagedConsumer.ExecutionIdentity.Print(); return 0; }
#endif
#if UPSTREAM_MAIN_ARGUMENTS
        byte* name = (byte*)Marshal.StringToCoTaskMemUTF8("translated-upstream-test");
        try { byte** argv = stackalloc byte*[2]; argv[0] = name; argv[1] = null; return Raw.main(1, argv); }
        finally { Marshal.FreeCoTaskMem((nint)name); }
#else
        return Raw.main();
#endif
    }
}
