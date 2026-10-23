using System.Runtime.InteropServices;
using System.Text;
using Raw = Managed.Security.S2n.S2nTls;
using RuntimeLibc = Managed.Security.S2n.S2nTls.Libc;

namespace Managed.Security.S2n;

public enum S2nProtocol : byte
{ Ssl3 = Raw.S2N_SSLv3, Tls10 = Raw.S2N_TLS10, Tls11 = Raw.S2N_TLS11, Tls12 = Raw.S2N_TLS12, Tls13 = Raw.S2N_TLS13 }
public enum S2nRole
{ Server = (int)Raw.s2n_mode.S2N_SERVER, Client = (int)Raw.s2n_mode.S2N_CLIENT }
public enum S2nClientAuthentication
{ None = (int)Raw.s2n_cert_auth_type.S2N_CERT_AUTH_NONE, Required = (int)Raw.s2n_cert_auth_type.S2N_CERT_AUTH_REQUIRED, Optional = (int)Raw.s2n_cert_auth_type.S2N_CERT_AUTH_OPTIONAL }
public enum S2nBlinding
{ BuiltIn = (int)Raw.s2n_blinding.S2N_BUILT_IN_BLINDING, SelfService = (int)Raw.s2n_blinding.S2N_SELF_SERVICE_BLINDING }
public enum S2nRenegotiation
{ Ignore = (int)Raw.s2n_renegotiate_response.S2N_RENEGOTIATE_IGNORE, Reject = (int)Raw.s2n_renegotiate_response.S2N_RENEGOTIATE_REJECT, Accept = (int)Raw.s2n_renegotiate_response.S2N_RENEGOTIATE_ACCEPT }

public sealed record S2nPolicy
{
    public S2nProtocol Minimum { get; }
    public S2nProtocol Maximum { get; }
    /// <summary>Zero retains all compatible suites. Other values force one IANA suite.</summary>
    public ushort CipherSuite { get; }
    public S2nPolicy(S2nProtocol minimum = S2nProtocol.Ssl3, S2nProtocol maximum = S2nProtocol.Tls13, ushort cipherSuite = 0)
    {
        if (!Enum.IsDefined(minimum) || !Enum.IsDefined(maximum) || minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(minimum));
        Minimum = minimum; Maximum = maximum; CipherSuite = cipherSuite;
    }
}

public sealed class S2nException : IOException
{
    public int ErrorCode { get; }
    public int ErrorType { get; }
    public string ErrorName { get; }
    public int SystemError { get; }
    internal S2nException(string operation, int error, int type, string name, string message, int systemError)
        : base($"{operation}: {name}: {message} (s2n={error}, errno={systemError})")
    { ErrorCode = error; ErrorType = type; ErrorName = name; SystemError = systemError; }
}

internal static unsafe class Native
{
    internal static string Text(byte* pointer) => pointer == null ? "" : Marshal.PtrToStringUTF8((nint)pointer) ?? "";
    internal static S2nException Error(string operation)
    {
        int code = *Raw.s2n_errno_location();
        return new(operation, code, Raw.s2n_error_get_type(code), Text(Raw.s2n_strerror_name(code)),
            Text(Raw.s2n_strerror(code, null)), RuntimeLibc.errno);
    }
    internal static void Check(int result, string operation)
    { if (result < 0) throw Error(operation); }
    internal static nint Require(void* pointer, string operation)
    { if (pointer == null) throw Error(operation); return (nint)pointer; }
    internal static byte[] Utf8(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Contains('\0')) throw new ArgumentException("Embedded NUL is not allowed.", nameof(value));
        return Encoding.UTF8.GetBytes(value + "\0");
    }
}
