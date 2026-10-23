using Provider = global::Managed.S2nTls.Crypto.OpenSslAbi;

namespace Managed.Security.S2n;

public static unsafe partial class S2nTls
{
    public static X509_name_st* X509_NAME_new() => (X509_name_st*)Provider.X509_NAME_new();
    public static void X509_NAME_free(X509_name_st* name) => Provider.X509_NAME_free(name);
    public static int X509_NAME_add_entry_by_NID(X509_name_st* name, int nid, int type, byte* data, int length, int location, int set)
        => Provider.X509_NAME_add_entry_by_NID(name, nid, type, data, length, location, set);
    public static int X509_set_subject_name(x509_st* certificate, X509_name_st* name) => Provider.X509_set_subject_name(certificate, name);
}
