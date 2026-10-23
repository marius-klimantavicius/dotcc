using Provider = global::Managed.S2nTls.Crypto.OpenSslAbi;

namespace Managed.Security.S2n;

// Canonical names resolve already emitted original-C imports without changing
// production overrides, original C, or generated C#.
public static unsafe partial class S2nTls
{
    public static byte* OpenSSL_version(int selector) => Provider.OpenSSL_version(selector);
}
