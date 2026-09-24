using System.Formats.Asn1;
using System.Security.Authentication;
using Kerberos.NET.Entities;

namespace Managed.Smb;

// SPNEGO envelopes are BER, whereas Kerberos.NET's generated token decoders
// expect DER. Only an actual AP-REP is passed to AuthenticateServiceResponse.
internal static class KerberosTokenResponse
{
    internal const string MicrosoftKerberosOid = "1.2.840.48018.1.2.2";
    private static readonly Asn1Tag GssEnvelope = new(TagClass.Application, 0, true);
    private static readonly Asn1Tag NegTokenResponse = new(TagClass.ContextSpecific, 1, true);
    private enum NegotiationState { AcceptCompleted, AcceptIncomplete, Reject, RequestMic }

    internal static ReadOnlyMemory<byte> Read(ReadOnlyMemory<byte> token)
    {
        try
        {
            if (token.IsEmpty) throw new AuthenticationException("The SMB server returned an empty Kerberos response");
            if (token.Span[0] == 0x60)
            {
                var (mechanism, payload) = ReadGssEnvelope(token);
                if (mechanism == MechType.SPNEGO) token = payload;
                else return ReadKerberosEnvelope(mechanism, payload);
            }
            if (token.Span[0] == 0xa1)
            {
                var reader = new AsnReader(token, AsnEncodingRules.BER);
                var choice = reader.ReadSequence(NegTokenResponse);
                var sequence = choice.ReadSequence();
                reader.ThrowIfNotEmpty();
                choice.ThrowIfNotEmpty();
                var state = sequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0, true));
                if (state.ReadEnumeratedValue<NegotiationState>() != NegotiationState.AcceptCompleted)
                    throw new AuthenticationException("SPNEGO did not accept and complete Kerberos authentication");
                state.ThrowIfNotEmpty();
                var selected = sequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 1, true));
                RequireKerberos(selected.ReadObjectIdentifier());
                selected.ThrowIfNotEmpty();
                ReadOnlyMemory<byte> response = ReadOnlyMemory<byte>.Empty;
                if (sequence.HasData && sequence.PeekTag().HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 2)))
                {
                    var value = sequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 2, true));
                    response = value.ReadOctetString();
                    value.ThrowIfNotEmpty();
                }
                if (sequence.HasData && sequence.PeekTag().HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 3)))
                {
                    var mic = sequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 3, true));
                    // There is no mechanism fallback. The entire response, including
                    // these bytes, must subsequently pass SMB signature verification.
                    mic.ReadOctetString();
                    mic.ThrowIfNotEmpty();
                }
                sequence.ThrowIfNotEmpty();
                if (response.IsEmpty) return response;
                token = response;
                if (token.Span[0] == 0x60)
                {
                    var (mechanism, payload) = ReadGssEnvelope(token);
                    return ReadKerberosEnvelope(mechanism, payload);
                }
            }
            return RequireApRep(token);
        }
        catch (AsnContentException error)
        {
            throw new AuthenticationException("Invalid Kerberos/SPNEGO response", error);
        }
    }

    private static (string Mechanism, ReadOnlyMemory<byte> Payload) ReadGssEnvelope(ReadOnlyMemory<byte> token)
    {
        var tag = AsnDecoder.ReadEncodedValue(token.Span, AsnEncodingRules.BER,
            out int offset, out int length, out int consumed);
        if (tag != GssEnvelope || consumed != token.Length)
            throw new AuthenticationException("Invalid GSS token envelope");
        var content = token.Slice(offset, length);
        var reader = new AsnReader(content, AsnEncodingRules.BER);
        var oid = reader.ReadEncodedValue();
        string mechanism = new AsnReader(oid, AsnEncodingRules.BER).ReadObjectIdentifier();
        var payload = content[oid.Length..];
        if (payload.IsEmpty) throw new AuthenticationException("Empty GSS token payload");
        return (mechanism, payload);
    }

    private static ReadOnlyMemory<byte> ReadKerberosEnvelope(string mechanism, ReadOnlyMemory<byte> payload)
    {
        RequireKerberos(mechanism);
        // RFC 4121 TOK_ID 02 00 identifies AP-REP, not another ASN.1 wrapper.
        if (payload.Length < 2 || payload.Span[0] != 2 || payload.Span[1] != 0)
            throw new AuthenticationException("Expected a Kerberos AP-REP GSS token");
        return RequireApRep(payload[2..]);
    }

    private static ReadOnlyMemory<byte> RequireApRep(ReadOnlyMemory<byte> token)
    {
        var reader = new AsnReader(token, AsnEncodingRules.BER);
        reader.ReadSequence(new Asn1Tag(TagClass.Application, 15, true));
        reader.ThrowIfNotEmpty();
        return token; // Kerberos.NET validates and decrypts the AP-REP contents.
    }

    private static void RequireKerberos(string mechanism)
    {
        if (mechanism != MechType.KerberosGssApi && mechanism != MicrosoftKerberosOid)
            throw new AuthenticationException("SPNEGO selected a non-Kerberos mechanism");
    }
}
