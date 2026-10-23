using System.Security.Cryptography;
using Raw = Managed.Security.S2n.S2nTls;

namespace Managed.Security.S2n;

/// <summary>Owns a parsed translated certificate chain/private key. A configuration
/// retains its own reference, so disposing this owner cannot invalidate a connection.</summary>
public sealed class S2nCredentials : IDisposable
{
    private readonly object _gate = new();
    private S2nRuntime.Lease? _runtime;
    private nint _handle;
    private int _references = 1;
    private bool _disposed;

    public unsafe S2nCredentials(S2nRuntime runtime, string certificateChainPem, string privateKeyPem)
    {
        ArgumentNullException.ThrowIfNull(runtime);
        _runtime = runtime.Retain();
        byte[]? key = null;
        try
        {
            _handle = Native.Require(Raw.s2n_cert_chain_and_key_new(), "s2n_cert_chain_and_key_new");
            byte[] cert = Native.Utf8(certificateChainPem); key = Native.Utf8(privateKeyPem);
            fixed (byte* c = cert, k = key)
                Native.Check(Raw.s2n_cert_chain_and_key_load_pem((Raw.s2n_cert_chain_and_key*)_handle, c, k), "s2n_cert_chain_and_key_load_pem");
        }
        catch { _disposed = true; Release(); throw; }
        finally { if (key != null) CryptographicOperations.ZeroMemory(key); }
    }

    internal nint Retain()
    {
        lock (_gate)
        { ObjectDisposedException.ThrowIf(_disposed, this); _references++; return _handle; }
    }
    internal unsafe void Release()
    {
        lock (_gate)
        {
            if (--_references != 0) return;
            try
            {
                if (_handle != 0) Native.Check(Raw.s2n_cert_chain_and_key_free((Raw.s2n_cert_chain_and_key*)_handle), "s2n_cert_chain_and_key_free");
                _handle = 0; _runtime?.Dispose(); _runtime = null;
            }
            catch (Exception error) { S2nRuntime.Quarantine(error); throw; }
        }
    }
    public void Dispose()
    {
        lock (_gate) { if (_disposed) return; _disposed = true; }
        Release(); GC.SuppressFinalize(this);
    }
    ~S2nCredentials() { try { Dispose(); } catch (Exception error) { S2nRuntime.Quarantine(error); } }
}
