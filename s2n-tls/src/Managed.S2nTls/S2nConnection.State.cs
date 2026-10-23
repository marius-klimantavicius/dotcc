using Raw = Managed.Security.S2n.S2nTls;

namespace Managed.Security.S2n;

public sealed partial class S2nConnection
{
    /// <summary>Exports plaintext traffic keys without integrity protection.
    /// The caller must protect the returned bytes. Upstream rejects RC4 and
    /// buffered I/O. Successful serialization closes the source connection.</summary>
    public unsafe byte[] ExportUnprotectedState() => Enter(c =>
    {
        uint length = 0;
        Native.Check(Raw.s2n_connection_serialization_length(c, &length), "s2n_connection_serialization_length");
        byte[] state = new byte[checked((int)length)];
        fixed (byte* bytes = state) Native.Check(Raw.s2n_connection_serialize(c, bytes, length), "s2n_connection_serialize");
        _shutdown = true;
        return state;
    });

    /// <summary>Imports caller-authenticated state. Reapplies this owner's config;
    /// no intrinsic integrity/authenticity check is provided by upstream.</summary>
    public unsafe void ImportUnprotectedState(ReadOnlySpan<byte> state)
    {
        byte[] copy = state.ToArray();
        try
        {
            Enter(c =>
            {
                fixed (byte* bytes = copy) Native.Check(Raw.s2n_connection_deserialize(c, bytes, checked((uint)copy.Length)), "s2n_connection_deserialize");
                // Upstream requires the configuration to be set after import.
                Native.Check(Raw.s2n_connection_set_config(c, (Raw.s2n_config*)_configHandle), "s2n_connection_set_config");
                if (_policy != 0) Native.Check(Raw.dotcc_s2n_connection_use_policy(c, (Raw.dotcc_s2n_policy*)_policy), "dotcc_s2n_connection_use_policy");
                _started = true;
                return 0;
            }, true);
        }
        finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(copy); }
    }

    public unsafe byte[] ExportSession() => Enter(c =>
    {
        int length = Raw.s2n_connection_get_session_length(c);
        Native.Check(length, "s2n_connection_get_session_length");
        if (length == 0) return [];
        byte[] session = new byte[length];
        fixed (byte* bytes = session)
        {
            int actual = Raw.s2n_connection_get_session(c, bytes, checked((ulong)length));
            Native.Check(actual, "s2n_connection_get_session");
            if (actual != length) throw new IOException("Unexpected session length.");
        }
        return session;
    });

    public unsafe void ImportSession(ReadOnlySpan<byte> session)
    {
        byte[] copy = session.ToArray();
        try { Enter(c => { fixed (byte* bytes = copy) Native.Check(Raw.s2n_connection_set_session(c, bytes, checked((ulong)copy.Length)), "s2n_connection_set_session"); return 0; }, true); }
        finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(copy); }
    }

    public unsafe void RequestKeyUpdate(bool requestPeerUpdate = false)
        => Enter(c => { Native.Check(Raw.s2n_connection_request_key_update(c, (Raw.s2n_peer_key_update)(requestPeerUpdate ? 1 : 0)), "s2n_connection_request_key_update"); return 0; });

    public unsafe (byte Sent, byte Received) KeyUpdates => Enter(c =>
    {
        byte sent = 0, received = 0;
        Native.Check(Raw.s2n_connection_get_key_update_counts(c, &sent, &received), "s2n_connection_get_key_update_counts");
        return (sent, received);
    });

    /// <summary>Reuses this drained connection with its existing byte transport.
    /// A replacement peer must already be attached by the transport owner.</summary>
    public unsafe void Reset()
    {
        Enter(c =>
        {
            if (!_shutdown) throw new InvalidOperationException("Complete TLS shutdown before reuse.");
            Native.Check(Raw.s2n_connection_wipe(c), "s2n_connection_wipe");
            Native.Check(Raw.s2n_connection_set_config(c, (Raw.s2n_config*)_configHandle), "s2n_connection_set_config");
            if (_policy != 0) Native.Check(Raw.dotcc_s2n_connection_use_policy(c, (Raw.dotcc_s2n_policy*)_policy), "dotcc_s2n_connection_use_policy");
            void* context = (void*)System.Runtime.InteropServices.GCHandle.ToIntPtr(_ioRoot);
            Native.Check(Raw.s2n_connection_set_recv_ctx(c, context), "s2n_connection_set_recv_ctx");
            Native.Check(Raw.s2n_connection_set_send_ctx(c, context), "s2n_connection_set_send_ctx");
            Native.Check(Raw.s2n_connection_set_recv_cb(c, &IoCallbackState.Receive), "s2n_connection_set_recv_cb");
            Native.Check(Raw.s2n_connection_set_send_cb(c, &IoCallbackState.Send), "s2n_connection_set_send_cb");
            Native.Check(Raw.s2n_connection_set_blinding(c, (Raw.s2n_blinding)(int)S2nBlinding.SelfService), "s2n_connection_set_blinding");
            if (_verifyRoot.IsAllocated) { _verifyRoot.Free(); _verifyRoot = default; }
            _started = false; _shutdown = false; _peerClosed = false; _validation = null;
            return 0;
        });
    }
}
