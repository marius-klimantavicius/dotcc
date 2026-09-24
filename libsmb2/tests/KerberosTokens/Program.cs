using System.Formats.Asn1;
using System.Security.Authentication;
using System.Security.Cryptography;
using Kerberos.NET.Client;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;
using Managed.Smb;
using static Managed.Smb.LibSmb2;

internal static unsafe class Program
{
    private enum Result { Complete, Incomplete, Reject, RequestMic }
    private static int _checks;

    private static void Main()
    {
        var request = ManagedServiceTicketRequest("files.example.test");
        Require(request.ServicePrincipalName == "cifs/files.example.test", "service principal");
        Require(request.ApOptions == default && (request.GssContextFlags & GssContextEstablishmentFlag.GSS_C_MUTUAL_FLAG) == 0,
            "managed credentials request non-mutual GSS");
        var ticketKey = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
        KrbApReq.CreateApReq(new KrbTgsRep
        {
            CRealm = "EXAMPLE.TEST", CName = new KrbPrincipalName { Name = ["test"] }
        }, ticketKey.AsKey(), request, out var authenticator);
        Require(authenticator.Subkey == null, "non-mutual AP-REQ has no client subkey");

        byte[] completed = Spnego(Result.Complete);
        byte[] berLong = [0xa1, 0x82, 0, (byte)(completed.Length - 1), 0x30, 0x81, (byte)(completed.Length - 4), .. completed[4..]];
        byte[] berIndefinite = [0xa1, 0x80, 0x30, 0x80, .. completed[4..], 0, 0, 0, 0];
        foreach (byte[] token in new[] { completed, berLong, berIndefinite, OuterSpnego(completed),
            Spnego(Result.Complete, mechanism: KerberosTokenResponse.MicrosoftKerberosOid) })
        {
            Require(KerberosTokenResponse.Read(token).IsEmpty, "completion without AP-REP");
            using var fixture = new AuthenticationFixture();
            fixture.SendInitialToken();
            Require(fixture.CompleteWithServerToken(token) == 0 && fixture.State.Authenticated, "provider completes SPNEGO");
            Require(fixture.ExportKey().SequenceEqual(fixture.Session.ServiceTicketSessionKey.KeyValue.ToArray()),
                "no AP-REP exports service-ticket key");
        }

        // Match the actual non-mutual call sequence, including an empty final
        // security buffer. A prepared ticket or its initial emission is not completion.
        foreach (var (encrypt, nonNullBuffer) in new[] { (false, false), (false, true), (true, false), (true, true) })
        {
            using var fixture = new AuthenticationFixture();
            Reject(() => fixture.ExportKey(), "prepared ticket cannot export key");
            fixture.SendInitialToken();
            Reject(() => fixture.ExportKey(), "initial emission cannot export key");
            int result = nonNullBuffer ? fixture.CompleteWithNonNullEmptyBuffer() : fixture.CompleteWithServerToken(null);
            Require(result == 0 && fixture.State.Authenticated,
                "empty final SMB security buffer completes non-mutual exchange regardless of pointer");
            Require(fixture.State.ExchangeState == KerberosExchangeState.Completed, "explicit completed state");
            Require(krb5_get_output_token_length(fixture.Auth) == 0 && krb5_get_output_token_buffer(fixture.Auth) == null,
                "final empty response clears outgoing AP-REQ");
            fixture.Context->sign = (byte)(encrypt ? 0 : 1);
            fixture.Context->seal = (byte)(encrypt ? 1 : 0);
            fixture.Context->hdr.flags = 0;
            Reject(() => fixture.ExportKey(), "empty final response still requires signed SMB session setup");
            fixture.Context->hdr.flags = SMB2_FLAGS_SIGNED;
            Require(fixture.ExportKey().SequenceEqual(fixture.Session.ServiceTicketSessionKey.KeyValue.ToArray()),
                "empty completion exports service-ticket session key");
        }

        // Each response form gets its own exchange, including the initial call.
        for (int form = 0; form < 5; form++)
        {
            using var fixture = new AuthenticationFixture(mutual: true);
            fixture.SendInitialToken();
            var subkey = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
            var reply = new KrbApRep
            {
                EncryptedPart = KrbEncryptedData.Encrypt(new KrbEncApRepPart
                {
                    CTime = fixture.Session.CTime, CuSec = fixture.Session.CuSec, SubSessionKey = subkey
                }.EncodeApplication(), fixture.Session.SessionKey.AsKey(), KeyUsage.EncApRepPart)
            };
            byte[] raw = reply.EncodeApplication().ToArray();
            var oidWriter = new AsnWriter(AsnEncodingRules.DER);
            oidWriter.WriteObjectIdentifier(MechType.KerberosGssApi);
            byte[] payload = [.. oidWriter.Encode(), 2, 0, .. raw];
            byte[] gss = [0x60, 0x82, (byte)(payload.Length >> 8), (byte)payload.Length, .. payload];
            byte[] token = form switch
            {
                0 => raw, 1 => gss, 2 => Spnego(Result.Complete, raw),
                3 => Spnego(Result.Complete, gss), _ => OuterSpnego(Spnego(Result.Complete, gss))
            };
            Require(KerberosTokenResponse.Read(token).Span.SequenceEqual(raw), "only AP-REP reaches Kerberos.NET");
            Require(fixture.CompleteWithServerToken(token) == 0, "AP-REP decrypts and validates");
            Require(fixture.ExportKey().SequenceEqual(subkey.KeyValue.ToArray()), "AP-REP subkey is exported");
            Require(!fixture.Session.SessionKey.KeyValue.Span.SequenceEqual(subkey.KeyValue.Span),
                "AP-REP result is retained independently of Session.SessionKey");
            Require(fixture.CompleteWithServerToken(null) == 0, "empty callback after validated AP-REP remains complete");
            Require(fixture.ExportKey().SequenceEqual(subkey.KeyValue.ToArray()), "empty callback does not replace validated subkey");
        }

        foreach (byte[] token in new[] { Spnego(Result.Reject), Spnego(Result.Incomplete), Spnego(Result.RequestMic),
            Spnego(Result.Complete, mechanism: "1.3.6.1.4.1.311.2.2.10"), Spnego(Result.Complete, mechanism: null),
            Spnego(null), Array.Empty<byte>(), completed[..^1], completed.Concat(new byte[] { 0 }).ToArray() })
        {
            Reject(() => KerberosTokenResponse.Read(token), "invalid or incomplete negotiation");
            if (token.Length == 0) continue;
            using var fixture = new AuthenticationFixture();
            fixture.SendInitialToken();
            Require(fixture.CompleteWithServerToken(token) == -1, "provider reports negotiation failure");
            Reject(() => fixture.ExportKey(), "failed provider cannot export key");
            Require(fixture.CompleteWithServerToken(completed) == -1, "provider failure is sticky");
            Require(fixture.CompleteWithServerToken(null) == -1 && fixture.State.ExchangeState == KerberosExchangeState.Failed,
                "empty callback cannot recover a failed exchange");
            Require(fixture.CompleteWithNonNullEmptyBuffer() == -1 && fixture.State.ExchangeState == KerberosExchangeState.Failed,
                "non-null empty buffer cannot recover a failed exchange");
            Reject(() => fixture.ExportKey(), "sticky failure refuses key export");
        }

        using (var fixture = new AuthenticationFixture())
        {
            Reject(() => fixture.ExportKey(), "cannot export before completion");
        }
        using (var fixture = new AuthenticationFixture())
        {
            fixture.SendInitialToken();
            Require(fixture.CompleteWithServerToken(completed) == 0, "complete before signature check");
            fixture.Context->sign = 0;
            fixture.Context->seal = 1;
            fixture.Context->hdr.flags = 0;
            Reject(() => fixture.ExportKey(), "encryption does not permit unsigned session setup");
        }
        foreach (byte[]? token in new byte[]?[] { null, completed })
        {
            using var fixture = new AuthenticationFixture(mutual: true);
            fixture.SendInitialToken();
            Require(fixture.CompleteWithServerToken(token) == -1, "no AP-REP cannot complete a mutual context");
            Reject(() => fixture.ExportKey(), "mutual context cannot guess the session key");
        }
        using (var fixture = new AuthenticationFixture(mutual: true))
        {
            fixture.SendInitialToken();
            Require(fixture.CompleteWithNonNullEmptyBuffer() == -1 && fixture.State.ExchangeState == KerberosExchangeState.Failed,
                "non-null empty buffer cannot complete a mutual context");
            Reject(() => fixture.ExportKey(), "mutual context still requires a validated AP-REP");
        }
        using (var fixture = new AuthenticationFixture())
        {
            fixture.SendInitialToken();
            var invalidReply = new KrbApRep
            {
                EncryptedPart = KrbEncryptedData.Encrypt(new KrbEncApRepPart
                {
                    CTime = fixture.Session.CTime, CuSec = fixture.Session.CuSec + 1
                }.EncodeApplication(), fixture.Session.SessionKey.AsKey(), KeyUsage.EncApRepPart)
            };
            Require(fixture.CompleteWithServerToken(Spnego(Result.Complete, invalidReply.EncodeApplication().ToArray())) == -1,
                "AP-REP must validate against the actual request");
            Require(fixture.CompleteWithServerToken(null) == -1, "invalid AP-REP cannot become empty completion");
            Reject(() => fixture.ExportKey(), "invalid AP-REP cannot export key");
        }
        // The native calls require Windows. These checks cover the completion
        // policy used immediately after SEC_E_OK and NEGOTIATION_INFO querying.
        WindowsKerberosContext.ValidateNegotiatedPackage("Kerberos", 0x2);
        WindowsKerberosContext.ValidateNegotiatedPackage("kerberos", 0x2);
        foreach (string? package in new[] { "NTLM", "Negotiate", "", null })
            Reject(() => WindowsKerberosContext.ValidateNegotiatedPackage(package, 0x2), "SSPI selected package");
        Reject(() => WindowsKerberosContext.ValidateNegotiatedPackage("Kerberos", 0), "SSPI mutual authentication");
        Console.WriteLine($"PASS: {_checks} Kerberos token, session-key and completion-policy checks (no KDC/SSPI integration)");
    }

    private sealed class AuthenticationFixture : IDisposable
    {
        internal readonly smb2_context* Context;
        internal readonly private_auth_data* Auth;
        internal readonly KerberosAuthState State;
        internal readonly ApplicationSessionContext Session;
        internal readonly byte[] InitialToken;
        private bool _initialSent;
        internal AuthenticationFixture(bool mutual = false)
        {
            var key = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
            var request = ManagedServiceTicketRequest("files.example.test");
            if (mutual) request.ApOptions = ApOptions.MutualRequired;
            var apReq = KrbApReq.CreateApReq(new KrbTgsRep
            {
                CRealm = "EXAMPLE.TEST", CName = new KrbPrincipalName { Name = ["test"] },
                // A local opaque ticket is sufficient: no KDC/server reads it.
                Ticket = new KrbTicket
                {
                    Realm = "EXAMPLE.TEST", SName = new KrbPrincipalName { Name = ["cifs", "files.example.test"] },
                    EncryptedPart = new KrbEncryptedData { EType = key.EType, Cipher = new byte[32] }
                }
            }, key.AsKey(), request, out var authenticator);
            Session = new ApplicationSessionContext
            {
                ApReq = apReq, SessionKey = authenticator.Subkey ?? key, ServiceTicketSessionKey = key,
                ClientSubSessionKey = authenticator.Subkey,
                CTime = authenticator.CTime, CuSec = authenticator.CuSec, SequenceNumber = authenticator.SequenceNumber
            };
            InitialToken = GssApiToken.Encode(new Oid(MechType.KerberosGssApi), apReq).ToArray();
            State = new KerberosAuthState(new KerberosClient(), Session);
            State.SetOutputToken(InitialToken);
            Context = smb2_init_context();
            if (Context == null) throw new OutOfMemoryException();
            Context->hdr.flags = SMB2_FLAGS_SIGNED;
            using var prepared = new PreparedKerberosAuthentication(State);
            RegisterPreparedKerberos((nint)Context, prepared);
            Auth = krb5_negotiate_reply(Context, null, null, null, null);
            if (Auth == null) throw new InvalidOperationException("Auth allocation failed");
        }
        internal void SendInitialToken()
        {
            Require(!_initialSent && State.ExchangeState == KerberosExchangeState.InitialTokenPending, "initial token pending");
            Require(krb5_session_request(Context, Auth, null, 0) == 0, "initial token emitted");
            Require(State.ExchangeState == KerberosExchangeState.InitialTokenSent && !State.Authenticated,
                "initial emission does not authenticate");
            Require(new ReadOnlySpan<byte>(krb5_get_output_token_buffer(Auth), krb5_get_output_token_length(Auth)).SequenceEqual(InitialToken),
                "prepared AP-REQ is returned unchanged");
            _initialSent = true;
        }
        internal int CompleteWithServerToken(byte[]? token)
        {
            if (!_initialSent) throw new InvalidOperationException("Test must send the AP-REQ before simulating a server response");
            fixed (byte* bytes = token) return krb5_session_request(Context, Auth, bytes, token?.Length ?? 0);
        }
        internal int CompleteWithNonNullEmptyBuffer()
        {
            if (!_initialSent) throw new InvalidOperationException("Test must send the AP-REQ before simulating a server response");
            // An empty fixed managed array may yield null. This buffer is
            // genuinely non-null but has zero protocol bytes to consume.
            byte* buffer = stackalloc byte[1];
            return krb5_session_request(Context, Auth, buffer, 0);
        }
        internal byte[] ExportKey()
        {
            krb5_session_get_session_key(Context, Auth);
            return new ReadOnlySpan<byte>(Context->session_key, Context->session_key_size).ToArray();
        }
        public void Dispose()
        {
            krb5_free_auth_data(Auth);
            smb2_destroy_context(Context);
        }
    }

    private static byte[] Spnego(Result? state, byte[]? response = null, string? mechanism = MechType.KerberosGssApi)
    {
        var writer = new AsnWriter(AsnEncodingRules.DER);
        var choice = new Asn1Tag(TagClass.ContextSpecific, 1, true);
        writer.PushSequence(choice);
        writer.PushSequence();
        if (state != null)
        {
            var tag = new Asn1Tag(TagClass.ContextSpecific, 0, true);
            writer.PushSequence(tag); writer.WriteEnumeratedValue(state.Value); writer.PopSequence(tag);
        }
        if (mechanism != null)
        {
            var tag = new Asn1Tag(TagClass.ContextSpecific, 1, true);
            writer.PushSequence(tag); writer.WriteObjectIdentifier(mechanism); writer.PopSequence(tag);
        }
        if (response != null)
        {
            var tag = new Asn1Tag(TagClass.ContextSpecific, 2, true);
            writer.PushSequence(tag); writer.WriteOctetString(response); writer.PopSequence(tag);
        }
        writer.PopSequence(); writer.PopSequence(choice);
        return writer.Encode();
    }

    private static byte[] OuterSpnego(byte[] token)
    {
        var writer = new AsnWriter(AsnEncodingRules.BER);
        var tag = new Asn1Tag(TagClass.Application, 0, true);
        writer.PushSequence(tag); writer.WriteObjectIdentifier(MechType.SPNEGO);
        writer.WriteEncodedValue(token); writer.PopSequence(tag);
        return writer.Encode();
    }

    private static void Require(bool condition, string label)
    {
        if (!condition) throw new InvalidOperationException(label);
        _checks++;
    }
    private static void Reject(Action action, string label)
    {
        try { action(); } catch (AuthenticationException) { _checks++; return; }
        throw new InvalidOperationException("Expected rejection: " + label);
    }
}
