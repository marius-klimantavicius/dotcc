using System;
using System.Runtime.CompilerServices;
using Managed.Transport;

internal static unsafe class Program
{
    private static int Main()
    {
        // This checks generated assembly reachability and a real rejecting boundary.
        // Runtime host services and transport behavior have separate phase gates.
        if (MsQuic.MsQuicHostInstall(null) == 0 || MsQuic.MsQuicHostUninstall() != 0)
            return 1;
        Console.WriteLine(RuntimeFeature.IsDynamicCodeSupported ? "jit: boundary rejection passed" : "nativeaot: boundary rejection passed");
        return 0;
    }
}
