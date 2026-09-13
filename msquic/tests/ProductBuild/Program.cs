using System;
using System.Runtime.CompilerServices;
using Managed.Transport;

internal static unsafe class Program
{
    private static int Main()
    {
        // These wrappers use typedef/primitive casts and function-like macros
        // upstream. Their public exports must be usable as C# constants.
        const uint pending = MsQuic.QUIC_STATUS_PENDING;
        const uint addressInUse = MsQuic.QUIC_STATUS_ADDRESS_IN_USE;
        const uint badCertificate = MsQuic.QUIC_STATUS_BAD_CERTIFICATE;
        if (!pending.Equals(4294967294U) || !addressInUse.Equals(98U) || !badCertificate.Equals(200000298U))
            return 1;
        // This checks generated assembly reachability and a real rejecting boundary.
        // Runtime host services and transport behavior have separate phase gates.
        if (MsQuic.MsQuicHostInstall(null) == 0 || MsQuic.MsQuicHostUninstall() != 0)
            return 1;
        // Public ABI types must be usable through the wrapper in a separate
        // project; generated file-local aliases cannot supply this name.
        MsQuic.MSQUIC_HOST_TABLE table = default;
        table.Size = (uint)sizeof(MsQuic.MSQUIC_HOST_TABLE);
        if (MsQuic.MsQuicHostInstall(&table) == 0)
            return 1;
        Console.WriteLine(RuntimeFeature.IsDynamicCodeSupported ? "jit: boundary rejection passed" : "nativeaot: boundary rejection passed");
        return 0;
    }
}
