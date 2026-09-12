namespace Managed.Transport.Hosting;

// Pinned Linux LP64 values from msquic_posix.h. The host ABI tests compare these
// with the actual native macros; these are not Windows HRESULT values.
internal static class Status
{
    internal const uint Success = 0, Pending = unchecked((uint)-2), Continue = unchecked((uint)-1);
    internal const uint OutOfMemory = 12, InvalidParameter = 22, InvalidState = 1;
    internal const uint NotSupported = 95, NotFound = 2, BufferTooSmall = 75;
    internal const uint HandshakeFailure = 103, Aborted = 125, AddressInUse = 98, InvalidAddress = 97;
    internal const uint ConnectionTimeout = 110, ConnectionIdle = 62, InternalError = 5, ConnectionRefused = 111;
    internal const uint ProtocolError = 71, VersionNegotiationError = 93, Unreachable = 113, TlsError = 126;
    internal const uint UserCanceled = 130, AlpnNegotiationFailure = 92, StreamLimitReached = 86;
    internal const uint AlpnInUse = 91, AddressNotAvailable = 99;
    internal const uint TlsErrorBase = 200000256, CertificateErrorBase = 200000512;
    internal const uint CertificateExpired = CertificateErrorBase + 1, CertificateUntrustedRoot = CertificateErrorBase + 2;
    internal const uint CertificateMissing = CertificateErrorBase + 3;
    internal static uint TlsAlert(byte alert) => TlsErrorBase + alert;
    internal static bool Failed(uint status) => unchecked((int)status) > 0;
}
