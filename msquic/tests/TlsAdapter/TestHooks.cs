using Managed.Security;
namespace Managed.Transport.Hosting;

// Compile-linked test access only. Product constructors/callbacks remain unchanged.
public sealed unsafe partial class MsQuicHost
{
    internal void ForceRetry(CXPLAT_TLS* token) => Resource<TlsConnection>(token).ForceRetryForTest();
    private sealed partial class TlsConnection
    {
        internal void ForceRetryForTest() => Managed.Security.Picotls.dotcc_ptls_server_properties(Properties, 1);
    }
}
