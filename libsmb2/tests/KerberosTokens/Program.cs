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
            Require(fixture.Process(token) == 0 && fixture.State.Authenticated, "provider completes SPNEGO");
            Require(fixture.ExportKey().SequenceEqual(fixture.Session.ServiceTicketSessionKey.KeyValue.ToArray()),
                "no AP-REP exports service-ticket key");
        }

        using (var fixture = new AuthenticationFixture())
        {
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
            foreach (var token in new[] { raw, gss, Spnego(Result.Complete, raw), Spnego(Result.Complete, gss),
                OuterSpnego(Spnego(Result.Complete, gss)) })
            {
                Require(KerberosTokenResponse.Read(token).Span.SequenceEqual(raw), "only AP-REP reaches Kerberos.NET");
                Require(fixture.Process(token) == 0, "AP-REP decrypts and validates");
                Require(fixture.ExportKey().SequenceEqual(subkey.KeyValue.ToArray()), "AP-REP subkey is exported");
            }
        }

        foreach (byte[] token in new[] { Spnego(Result.Reject), Spnego(Result.Incomplete), Spnego(Result.RequestMic),
            Spnego(Result.Complete, mechanism: "1.3.6.1.4.1.311.2.2.10"), Spnego(Result.Complete, mechanism: null),
            Spnego(null), Array.Empty<byte>(), completed[..^1], completed.Concat(new byte[] { 0 }).ToArray() })
        {
            Reject(() => KerberosTokenResponse.Read(token), "invalid or incomplete negotiation");
            if (token.Length == 0) continue;
            using var fixture = new AuthenticationFixture();
            Require(fixture.Process(token) == -1, "provider reports negotiation failure");
            Reject(() => fixture.ExportKey(), "failed provider cannot export key");
            Require(fixture.Process(completed) == -1, "provider failure is sticky");
        }

        using (var fixture = new AuthenticationFixture())
        {
            Reject(() => fixture.ExportKey(), "cannot export before completion");
        }
        using (var fixture = new AuthenticationFixture())
        {
            Require(fixture.Process(completed) == 0, "complete before signature check");
            fixture.Context->sign = 0;
            fixture.Context->seal = 1;
            fixture.Context->hdr.flags = 0;
            Reject(() => fixture.ExportKey(), "encryption does not permit unsigned session setup");
        }
        using (var fixture = new AuthenticationFixture())
        {
            fixture.Session.ApReq.ApOptions = ApOptions.MutualRequired;
            Require(fixture.Process(completed) == -1, "no AP-REP cannot complete an explicitly mutual context");
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
        internal AuthenticationFixture()
        {
            var key = KrbEncryptionKey.Generate(EncryptionType.AES256_CTS_HMAC_SHA1_96);
            Session = new ApplicationSessionContext
            {
                ApReq = new KrbApReq(), SessionKey = key, ServiceTicketSessionKey = key,
                CTime = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()), CuSec = 123
            };
            State = new KerberosAuthState(new KerberosClient(), Session);
            Context = smb2_init_context();
            if (Context == null) throw new OutOfMemoryException();
            Context->hdr.flags = SMB2_FLAGS_SIGNED;
            using var prepared = new PreparedKerberosAuthentication(State);
            RegisterPreparedKerberos((nint)Context, prepared);
            Auth = krb5_negotiate_reply(Context, null, null, null, null);
            if (Auth == null) throw new InvalidOperationException("Auth allocation failed");
        }
        internal int Process(byte[] token)
        {
            fixed (byte* bytes = token) return krb5_session_request(Context, Auth, bytes, token.Length);
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
