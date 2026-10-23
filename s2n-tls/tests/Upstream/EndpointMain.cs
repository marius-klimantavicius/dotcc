using System.Runtime.InteropServices;
using System.Reflection;
using Raw = Managed.Security.S2n.S2nTls;

// Test-only entry into the original translated s2nc/s2nd main. All option
// parsing, TLS calls and upstream diagnostic text remain in the original C.
internal static unsafe class EndpointMain
{
    private static int Main(string[] arguments)
    {
#if UPSTREAM_EXECUTION_IDENTITY
        if (arguments is ["--identity"]) { ManagedConsumer.ExecutionIdentity.Print(); return 0; }
#endif
        nint[] storage = new nint[arguments.Length + 1];
        byte** argv = stackalloc byte*[storage.Length + 1];
        try
        {
            for (int i = 0; i < storage.Length; i++)
            {
                storage[i] = Marshal.StringToCoTaskMemUTF8(i == 0 ? Assembly.GetExecutingAssembly().GetName().Name! : arguments[i - 1]);
                argv[i] = (byte*)storage[i];
            }
            argv[storage.Length] = null;
            Raw.ResetUpstreamGetopt();
            return Raw.main(storage.Length, argv);
        }
        finally { foreach (nint allocation in storage) if (allocation != 0) Marshal.FreeCoTaskMem(allocation); }
    }
}
