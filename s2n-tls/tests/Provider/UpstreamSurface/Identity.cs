namespace Managed.S2nTls.Crypto;

public static unsafe partial class OpenSslAbi
{
    // OPENSSL_VERSION requests the actual implementation identity. The native
    // header version describes declarations and must never identify this runtime.
    // Other OpenSSL build selectors have no defined managed equivalent here.
    public static byte* OpenSSL_version(int selector)
    {
        if (selector == 0) return dotcc_crypto_provider_name();
        Error();
        return null;
    }
}
