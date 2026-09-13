using static Managed.Transport.MsQuic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Managed.Transport;
using Managed.Transport.Hosting;

internal sealed unsafe class AdapterPeer : IDisposable
{
    private static readonly Dictionary<nint, AdapterPeer> Peers = [];
    private readonly MsQuicHost host;
    private readonly List<nint> allocations = [];
    private CXPLAT_SEC_CONFIG* security;
    internal CXPLAT_TLS* Token;
    internal readonly CXPLAT_TLS_PROCESS_STATE* State;
    internal readonly QUIC_CONNECTION* Connection;
    internal readonly bool Server;
    internal readonly byte[] Parameters;
    internal readonly List<byte[]> Tickets = [];
    internal byte[] ReceivedParameters = [];
    internal bool AcceptTicket = true, AcceptParameters = true;
    internal int TpCallbacks, TicketCallbacks, Compactions;
    internal bool AcceptCertificate = true;
    internal int CertificateCallbacks, ConfigCompletions;
    internal uint DeferredCertificateStatus;
    internal uint CombinedFlags;
    internal bool Complete => State->HandshakeComplete != 0;
    internal bool Resumed => State->SessionResumed != 0;
    internal ulong ReadEpoch => (ulong)State->ReadKey;
    internal uint Total => State->BufferTotalLength;

    internal AdapterPeer(MsQuicHost host, MsQuicHost.CredentialRegistration credential, bool server,
        string protocol = "dotcc-quic", string name = "localhost", byte[]? ticket = null, byte key = 0,
        bool retry = false, bool resumption = true, uint credentialFlags = 0,
        bool? asyncHandler = null, bool certificateCallback = true)
    {
        this.host = host; Server = server;
        Parameters = server ? [0x0f, 4, 5, 6, 7, 8, 0, 4, 9, 10, 11, 12] : [0x0f, 4, 1, 2, 3, 4];
        State = (CXPLAT_TLS_PROCESS_STATE*)Allocate(sizeof(CXPLAT_TLS_PROCESS_STATE));
        Connection = (QUIC_CONNECTION*)Allocate(sizeof(QUIC_CONNECTION));
        Peers.Add((nint)Connection, this);
        byte* parameters = null, resumed = null;
        try
        {
            QUIC_CREDENTIAL_CONFIG config = default;
            config.Type = (QUIC_CREDENTIAL_TYPE)MsQuicHost.ManagedCredentialType;
            config.Flags = (QUIC_CREDENTIAL_FLAGS)(0x2000 | (server ? 0u : 1u) | credentialFlags);
            if (asyncHandler ?? ((credentialFlags & 2) != 0)) config.AsyncHandler = &UnexpectedApplicationCompletion;
            config.CertificateContext = (void*)credential.Token;
            config.AllowedCipherSuites = (QUIC_ALLOWED_CIPHER_SUITE_FLAGS)(credential.CipherSuite == 0x1301 ? 1 : credential.CipherSuite == 0x1302 ? 2 : 3);
            CXPLAT_TLS_CALLBACKS callbacks = new() { ReceiveTP = &ReceiveParameters, ReceiveTicket = &ReceiveTicket };
            if (certificateCallback) callbacks.CertificateReceived = &ReceiveCertificate;
            uint result = MsQuic.CxPlatTlsSecConfigCreate(&config, (CXPLAT_TLS_CREDENTIAL_FLAGS)(resumption ? 0 : 1), &callbacks, Connection, &Configured);
            if (result != ((credentialFlags & 2) != 0 ? Status.Pending : Status.Success)) throw new CredentialConfigError(result);
            Program.Require(ConfigCompletions == 1 && security != null, "inline security config completion before Success/Pending return");
            if (server && key != 0) ImportKeys(key);
            QUIC_HKDF_LABELS labels = new() { KeyLabel = Copy("quic key\0"u8), IvLabel = Copy("quic iv\0"u8), HpLabel = Copy("quic hp\0"u8), KuLabel = Copy("quic ku\0"u8) };
            byte[] alpn = Encoding.UTF8.GetBytes(protocol);
            byte* protocols = (byte*)Allocate(alpn.Length + 1); protocols[0] = checked((byte)alpn.Length);
            alpn.CopyTo(new Span<byte>(protocols + 1, alpn.Length));
            parameters = PlatformCopy(Parameters, (uint)MsQuic.QUIC_POOL_TLS_TRANSPARAMS);
            if (ticket != null) resumed = PlatformCopy(ticket, (uint)MsQuic.QUIC_POOL_CRYPTO_RESUMPTION_TICKET);
            CXPLAT_TLS_CONFIG tls = new()
            {
                IsServer = server ? (byte)1 : (byte)0, Connection = Connection, SecConfig = security,
                HkdfLabels = &labels, AlpnBuffer = protocols, AlpnBufferLength = checked((ushort)(alpn.Length + 1)),
                TPType = 57, ServerName = Copy(Encoding.UTF8.GetBytes(name + "\0")),
                LocalTPBuffer = parameters, LocalTPLength = (uint)Parameters.Length,
                ResumptionTicketBuffer = resumed, ResumptionTicketLength = (uint)(ticket?.Length ?? 0)
            };
            CXPLAT_TLS* output = null;
            result = MsQuic.CxPlatTlsInitialize(&tls, State, &output);
            if (result != Status.Success) throw new ArgumentException("TLS initialize status " + result);
            parameters = null; resumed = null; Token = output;
            Program.Require(Token != null && State->EarlyDataState == CXPLAT_TLS_EARLY_DATA_STATE.CXPLAT_TLS_EARLY_DATA_UNSUPPORTED,
                "TLS initialize disables early data");
            if (retry) host.ForceRetry(Token);
        }
        catch { Dispose(); throw; }
        finally
        {
            if (parameters != null) host.FreePlatformMemory(parameters, (uint)MsQuic.QUIC_POOL_TLS_TRANSPARAMS);
            if (resumed != null) host.FreePlatformMemory(resumed, (uint)MsQuic.QUIC_POOL_CRYPTO_RESUMPTION_TICKET);
        }
    }
    internal void ImportKeys(params byte[] ids)
    {
        QUIC_TICKET_KEY_CONFIG* keys = stackalloc QUIC_TICKET_KEY_CONFIG[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            keys[i] = default; keys[i].MaterialLength = 64;
            new Span<byte>(keys[i].Id, 16).Fill(ids[i]); new Span<byte>(keys[i].Material, 64).Fill(ids[i]);
        }
        Program.Require(MsQuic.CxPlatTlsSecConfigSetTicketKeys(security, keys, (byte)ids.Length) == Status.Success, "typed imported ticket keys");
        CryptographicOperations.ZeroMemory(new Span<byte>(keys, sizeof(QUIC_TICKET_KEY_CONFIG) * ids.Length));
    }
    private static void Configured(QUIC_CREDENTIAL_CONFIG* config, void* context, uint status, CXPLAT_SEC_CONFIG* security)
    {
        Program.Require(status == Status.Success && security != null, "security completion success");
        var peer = Peers[(nint)context]; peer.ConfigCompletions++; peer.security = security;
    }
    private static void UnexpectedApplicationCompletion(QUIC_HANDLE* configuration, void* context, uint status)
        => throw new InvalidOperationException("The TLS provider must complete through its supplied core completion, not the application handler.");
    private static byte ReceiveCertificate(QUIC_CONNECTION* connection, void* certificatePointer, void* chainPointer, uint errors, uint status)
    {
        var peer = Peers[(nint)connection]; peer.CertificateCallbacks++; peer.DeferredCertificateStatus = status;
        Program.Require(certificatePointer != null && chainPointer != null && errors == 0, "portable certificate callback shape");
        var certificate = (QUIC_BUFFER*)certificatePointer; var chain = (QUIC_BUFFER*)chainPointer;
        using var leaf = X509CertificateLoader.LoadCertificate(new ReadOnlySpan<byte>(certificate->Buffer, checked((int)certificate->Length)));
        byte[] encoded = new ReadOnlySpan<byte>(chain->Buffer, checked((int)chain->Length)).ToArray();
        Program.Require(X509Certificate2.GetCertContentType(encoded) == X509ContentType.Pkcs7, "portable chain is actual PKCS7");
        var collection = new X509Certificate2Collection();
        try
        {
#pragma warning disable SYSLIB0057 // Collection.Import is the BCL PKCS7 decoder; the loader handles leaf DER above.
            collection.Import(encoded);
#pragma warning restore SYSLIB0057
            Program.Require(collection.Cast<X509Certificate2>().Any(item => item.RawData.AsSpan().SequenceEqual(leaf.RawData)), "PKCS7 contains indicated DER leaf");
        }
        finally { foreach (var item in collection) item.Dispose(); }
        return peer.AcceptCertificate ? (byte)1 : (byte)0;
    }
    private static byte ReceiveParameters(QUIC_CONNECTION* connection, ushort length, byte* bytes)
    {
        var peer = Peers[(nint)connection]; peer.TpCallbacks++;
        peer.ReceivedParameters = new ReadOnlySpan<byte>(bytes, length).ToArray();
        return peer.AcceptParameters ? (byte)1 : (byte)0;
    }
    private static byte ReceiveTicket(QUIC_CONNECTION* connection, uint length, byte* bytes)
    {
        var peer = Peers[(nint)connection]; peer.TicketCallbacks++;
        peer.Tickets.Add(new ReadOnlySpan<byte>(bytes, checked((int)length)).ToArray());
        return peer.AcceptTicket ? (byte)1 : (byte)0;
    }
    internal List<RawFlight> Process(ReadOnlySpan<byte> input, bool ticket = false, ushort? expectedAlert = null)
    {
        uint supplied = (uint)input.Length, length = supplied, total = Total;
        CXPLAT_TLS_RESULT_FLAGS flags;
        fixed (byte* bytes = input)
            flags = MsQuic.CxPlatTlsProcessData(Token, ticket ? CXPLAT_TLS_DATA_TYPE.CXPLAT_TLS_TICKET_DATA : CXPLAT_TLS_DATA_TYPE.CXPLAT_TLS_CRYPTO_DATA, bytes, &length, State);
        CombinedFlags |= (uint)flags;
        bool failed = (flags & CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_ERROR) != 0;
        if (expectedAlert != null)
        {
            Program.Require(failed && State->AlertCode == expectedAlert && length == 0, "exact alert and unconsumed rejected input");
            return [];
        }
        if (failed) throw new AdapterAlert(State->AlertCode);
        Program.Require(length == supplied, "input BufferLength reports exact consumption");
        Program.Require(((flags & CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_DATA) != 0) == (Total > total), "DATA result matches appended output");
        if ((flags & CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_READ_KEY_UPDATED) != 0)
            Program.Require(State->ReadKeys[(int)State->ReadKey].Value != null, "read key flag publishes live packet key");
        if ((flags & CXPLAT_TLS_RESULT_FLAGS.CXPLAT_TLS_RESULT_WRITE_KEY_UPDATED) != 0)
            Program.Require(State->WriteKeys[(int)State->WriteKey].Value != null, "write key flag publishes live packet key");
        return Drain();
    }
    private List<RawFlight> Drain()
    {
        var output = new List<RawFlight>();
        uint total = Total, handshake = State->BufferOffsetHandshake, app = State->BufferOffset1Rtt;
        Program.Require(handshake <= total && app <= total && (app == 0 || handshake <= app), "absolute output offsets monotonic");
        while (State->BufferLength != 0)
        {
            uint start = total - State->BufferLength;
            ulong epoch = app != 0 && start >= app ? 3UL : handshake != 0 && start >= handshake ? 2UL : 0UL;
            uint end = epoch == 0 && handshake > start ? handshake : epoch == 2 && app > start ? app : total;
            int count = checked((int)(end - start));
            Program.Require(count > 0 && count <= State->BufferLength, "nonempty bounded epoch range");
            byte[] bytes = new ReadOnlySpan<byte>(State->Buffer, count).ToArray();
            output.Add(new(epoch, bytes));
            // Mirror core ACK draining at crypto.c:567: overlapping move + length decrease.
            int first = Math.Max(1, count / 2);
            Compact(first); if (first != count) Compact(count - first);
            Program.Require(Total == total && State->BufferOffsetHandshake == handshake && State->BufferOffset1Rtt == app,
                "core compaction preserves total and absolute epoch offsets");
        }
        return output;
    }
    private void Compact(int count)
    {
        new Span<byte>(State->Buffer + count, State->BufferLength - count).CopyTo(new Span<byte>(State->Buffer, State->BufferLength - count));
        State->BufferLength -= (ushort)count; Compactions++;
    }
    internal void DeleteSecurityOwner() { if (security != null) { MsQuic.CxPlatTlsSecConfigDelete(security); security = null; } }
    private byte* PlatformCopy(byte[] source, uint tag)
    {
        byte* target = (byte*)host.AllocatePlatformMemory((ulong)source.Length, tag);
        if (target == null) throw new OutOfMemoryException();
        source.CopyTo(new Span<byte>(target, source.Length)); return target;
    }
    private void* Allocate(int size)
    {
        void* result = NativeMemory.AllocZeroed((nuint)size); if (result == null) throw new OutOfMemoryException();
        allocations.Add((nint)result); return result;
    }
    private byte* Copy(ReadOnlySpan<byte> source) { byte* result = (byte*)Allocate(source.Length); source.CopyTo(new Span<byte>(result, source.Length)); return result; }
    public void Dispose()
    {
        if (Token != null) { MsQuic.CxPlatTlsUninitialize(Token); Token = null; }
        DeleteSecurityOwner();
        if (State != null)
        {
            for (int i = 0; i < 6; i++) { MsQuic.QuicPacketKeyFree(State->ReadKeys[i].Value); MsQuic.QuicPacketKeyFree(State->WriteKeys[i].Value); }
            if (State->Buffer != null) host.FreePlatformMemory(State->Buffer, (uint)MsQuic.QUIC_POOL_TLS_BUFFER);
        }
        Peers.Remove((nint)Connection);
        foreach (nint pointer in allocations) NativeMemory.Free((void*)pointer); allocations.Clear();
        foreach (byte[] ticket in Tickets) CryptographicOperations.ZeroMemory(ticket); Tickets.Clear();
    }
}

internal sealed class AdapterAlert(int alert) : Exception("TLS adapter alert " + alert) { internal int Alert => alert; }
internal sealed class CredentialConfigError(uint status) : Exception("TLS security config status " + status) { internal uint Status => status; }
internal sealed record RawFlight(ulong Epoch, byte[] Data);
internal sealed record RawStep(int Code, List<RawFlight> Flights);
