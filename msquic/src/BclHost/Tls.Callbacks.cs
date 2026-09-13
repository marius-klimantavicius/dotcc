using static Managed.Transport.MsQuic;
using static Managed.Security.PicoTls;
using System.Security.Cryptography;
using Managed.Security;
using Ptls = Managed.Security.PicoTls;

namespace Managed.Transport.Hosting;

public sealed unsafe partial class MsQuicHost
{
    private static TlsConnection TlsOwner(st_ptls_t* tls)
    {
        CallbackScope.RequireActive();
        var context = (TlsCallbackContext*)*Ptls.ptls_get_data_ptr(tls);
        if (context == null || context->Token == null) throw new InvalidOperationException("TLS callback without a live connection");
        return FromContext(context->Host).Resource<TlsConnection>(context->Token);
    }

    private static void TlsRandom(void* output, ulong length)
    {
        try
        {
            CallbackScope.RequireActive();
            if (length > 1024 * 1024 || !PacketReadable(output, (uint)length)) throw new ArgumentException("TLS random request");
            RandomNumberGenerator.Fill(new Span<byte>(output, (int)length));
        }
        catch (Exception error) { CallbackScope.Capture(error); }
    }

    private static ulong TlsTime(st_ptls_get_time_t* self)
    {
        try { CallbackScope.RequireActive(); return checked((ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); }
        catch (Exception error) { CallbackScope.Capture(error); return 0; }
    }

    private static int TlsClientHello(st_ptls_on_client_hello_t* self, st_ptls_t* tls,
        st_ptls_on_client_hello_parameters_t* input)
    {
        try
        {
            var owner = TlsOwner(tls);
            if (input->server_name.len != 0)
            {
                int code = Ptls.ptls_set_server_name(tls, input->server_name.@base, input->server_name.len);
                if (code != 0) return code;
            }
            foreach (var protocol in owner.Protocols)
                for (ulong i = 0; i < input->negotiated_protocols.count; i++)
                {
                    var offered = input->negotiated_protocols.list[i];
                    if (offered.len == (ulong)protocol.Length && new ReadOnlySpan<byte>(offered.@base, protocol.Length).SequenceEqual(protocol))
                        fixed (byte* selected = protocol)
                            return Ptls.ptls_set_negotiated_protocol(tls, selected, (ulong)protocol.Length);
                }
            return 120;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }

    private static int TlsTrafficKey(st_ptls_update_traffic_key_t* self, st_ptls_t* tls,
        int encryption, ulong epoch, void* bytes)
    {
        CXPLAT_SECRET secret = default;
        try
        {
            var owner = TlsOwner(tls);
            if (epoch is not (2 or 3) || encryption is not (0 or 1)) return 10;
            var suite = Ptls.ptls_get_cipher(tls);
            if (suite == null || suite->id is not (0x1301 or 0x1302)) return 80;
            int length = suite->id == 0x1301 ? 32 : 48;
            if (suite->hash->digest_size != (ulong)length || bytes == null) return 80;
            secret.Hash = suite->id == 0x1301 ? CXPLAT_HASH_TYPE.CXPLAT_HASH_SHA256 : CXPLAT_HASH_TYPE.CXPLAT_HASH_SHA384;
            secret.Aead = suite->id == 0x1301 ? CXPLAT_AEAD_TYPE.CXPLAT_AEAD_AES_128_GCM : CXPLAT_AEAD_TYPE.CXPLAT_AEAD_AES_256_GCM;
            new ReadOnlySpan<byte>(bytes, length).CopyTo(new Span<byte>(secret.Secret, length));
            int index = (int)epoch;
            if ((encryption != 0 ? owner.State->WriteKeys[index].Value : owner.State->ReadKeys[index].Value) != null) return 10;
            QUIC_PACKET_KEY* key = null;
            var labels = owner.Labels;
            uint status = MsQuic.QuicPacketKeyDerive((QUIC_PACKET_KEY_TYPE)index, &labels, &secret, null, 1, &key);
            if (status != Status.Success) return 80;
            if (encryption != 0)
            {
                owner.State->WriteKeys[index].Value = key;
                owner.State->WriteKey = (QUIC_PACKET_KEY_TYPE)index;
                owner.Results |= CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_WRITE_KEY_UPDATED;
            }
            else
            {
                owner.State->ReadKeys[index].Value = key;
                // The server must consume the client's Finished at Handshake level.
                if (!owner.Server || index != 3)
                {
                    owner.State->ReadKey = (QUIC_PACKET_KEY_TYPE)index;
                    owner.Results |= CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_READ_KEY_UPDATED;
                }
            }
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
        finally { CryptographicOperations.ZeroMemory(new Span<byte>(&secret, sizeof(CXPLAT_SECRET))); }
    }

    private static int TlsCollectExtension(st_ptls_t* tls, st_ptls_handshake_properties_t* properties, ushort type)
        => type == 0x39 ? 1 : 0;

    private static int TlsCollectedExtensions(st_ptls_t* tls, st_ptls_handshake_properties_t* properties,
        st_ptls_raw_extension_t* extensions)
    {
        try
        {
            var owner = TlsOwner(tls);
            for (int i = 0; i < 32 && extensions[i].type != ushort.MaxValue; i++)
            {
                if (extensions[i].type != 0x39) continue;
                var data = extensions[i].data;
                if (data.len > ushort.MaxValue || !PacketReadable(data.@base, (uint)data.len)) return 50;
                var value = new ReadOnlySpan<byte>(data.@base, (int)data.len);
                if (owner.TransportParametersReceived) return value.SequenceEqual(owner.PeerParameters) ? 0 : 47;
                owner.PeerParameters = value.ToArray();
                // MsQuic has already parsed the server's ClientHello transport
                // parameters before selecting the TLS configuration. Its callback
                // specifically parses EncryptedExtensions on the client.
                if (!owner.Server && owner.Security.Callbacks.ReceiveTP(owner.Connection, (ushort)data.len, data.@base) == 0) return 47;
                owner.TransportParametersReceived = true;
                return 0;
            }
            return 109;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }

    private static int TlsSaveTicket(st_ptls_save_ticket_t* self, st_ptls_t* tls,
        st_ptls_iovec_t ticket, st_ptls_save_ticket_properties_t* properties)
    {
        try
        {
            var owner = TlsOwner(tls);
            if (properties->early_data != 0 || properties->max_early_data_size != 0) return 47;
            if (ticket.len is 0 or > 65535 || ticket.@base == null) return 50;
            if (owner.Security.ResumptionEnabled && owner.Security.Callbacks.ReceiveTicket != null)
                owner.Security.Callbacks.ReceiveTicket(owner.Connection, (uint)ticket.len, ticket.@base);
            return 0;
        }
        catch (Exception error) { CallbackScope.Capture(error); return 0x203; }
    }
}
