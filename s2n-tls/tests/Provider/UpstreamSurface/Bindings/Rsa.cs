using Provider = global::Managed.S2nTls.Crypto.OpenSslAbi;

namespace Managed.Security.S2n;

public static unsafe partial class S2nTls
{
    public static rsa_st* EVP_PKEY_get1_RSA(evp_pkey_st* key) => (rsa_st*)Provider.EVP_PKEY_get1_RSA(key);
    public static void RSA_free(rsa_st* rsa) => Provider.RSA_free(rsa);
    public static int RSA_size(rsa_st* rsa) => Provider.RSA_size(rsa);
    public static int RSA_public_encrypt(int length,byte* input,byte* output,rsa_st* rsa,int padding)
        => Provider.RSA_public_encrypt(length,input,output,rsa,padding);
}
