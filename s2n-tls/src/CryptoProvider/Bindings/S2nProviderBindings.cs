using Provider = global::Managed.S2nTls.Crypto.OpenSslAbi;

namespace Managed.Security.S2n;

// Authored source linked into both emitted products. Types come from actual s2n/OpenSSL emission.
public static unsafe partial class S2nTls
{
    public static evp_md_ctx_st* Provider_EVP_MD_CTX_new() => (evp_md_ctx_st*)Provider.EVP_MD_CTX_new();
    public static evp_md_st* Provider_EVP_MD_CTX_md(evp_md_ctx_st* ctx) => (evp_md_st*)Provider.EVP_MD_CTX_md(ctx);
    public static evp_md_st* Provider_EVP_md5() => (evp_md_st*)Provider.EVP_md5();
    public static evp_md_st* Provider_EVP_sha1() => (evp_md_st*)Provider.EVP_sha1();
    public static evp_md_st* Provider_EVP_sha224() => (evp_md_st*)Provider.EVP_sha224();
    public static evp_md_st* Provider_EVP_sha256() => (evp_md_st*)Provider.EVP_sha256();
    public static evp_md_st* Provider_EVP_sha384() => (evp_md_st*)Provider.EVP_sha384();
    public static evp_md_st* Provider_EVP_sha512() => (evp_md_st*)Provider.EVP_sha512();
    public static evp_md_st* Provider_EVP_md5_sha1() => (evp_md_st*)Provider.EVP_md5_sha1();
    public static evp_cipher_ctx_st* Provider_EVP_CIPHER_CTX_new() => (evp_cipher_ctx_st*)Provider.EVP_CIPHER_CTX_new();
    public static evp_cipher_st* Provider_EVP_rc4() => (evp_cipher_st*)Provider.EVP_rc4();
    public static evp_cipher_st* Provider_EVP_des_ede3_cbc() => (evp_cipher_st*)Provider.EVP_des_ede3_cbc();
    public static evp_cipher_st* Provider_EVP_aes_128_cbc() => (evp_cipher_st*)Provider.EVP_aes_128_cbc();
    public static evp_cipher_st* Provider_EVP_aes_256_cbc() => (evp_cipher_st*)Provider.EVP_aes_256_cbc();
    public static evp_cipher_st* Provider_EVP_aes_128_gcm() => (evp_cipher_st*)Provider.EVP_aes_128_gcm();
    public static evp_cipher_st* Provider_EVP_aes_256_gcm() => (evp_cipher_st*)Provider.EVP_aes_256_gcm();
    public static evp_cipher_st* Provider_EVP_chacha20_poly1305() => (evp_cipher_st*)Provider.EVP_chacha20_poly1305();
    public static evp_pkey_st* Provider_EVP_PKEY_new() => (evp_pkey_st*)Provider.EVP_PKEY_new();
    public static evp_pkey_ctx_st* Provider_EVP_PKEY_CTX_new(evp_pkey_st* key,engine_st* engine) => (evp_pkey_ctx_st*)Provider.EVP_PKEY_CTX_new(key,engine);
    public static evp_pkey_ctx_st* Provider_EVP_PKEY_CTX_new_id(int id,engine_st* engine) => (evp_pkey_ctx_st*)Provider.EVP_PKEY_CTX_new_id(id,engine);
    public static int Provider_EVP_PKEY_paramgen(evp_pkey_ctx_st* ctx,evp_pkey_st** key) => Provider.EVP_PKEY_paramgen(ctx,(void**)key);
    public static int Provider_EVP_PKEY_keygen(evp_pkey_ctx_st* ctx,evp_pkey_st** key) => Provider.EVP_PKEY_keygen(ctx,(void**)key);
    public static evp_pkey_st* Provider_d2i_AutoPrivateKey(evp_pkey_st** key,byte** input,long length) => (evp_pkey_st*)Provider.d2i_AutoPrivateKey((void**)key,input,length);
    public static evp_pkey_st* Provider_d2i_PrivateKey(int type,evp_pkey_st** key,byte** input,long length) => (evp_pkey_st*)Provider.d2i_PrivateKey(type,(void**)key,input,length);
    public static ec_key_st* Provider_EVP_PKEY_get1_EC_KEY(evp_pkey_st* key) => (ec_key_st*)Provider.EVP_PKEY_get1_EC_KEY(key);
    public static ec_group_st* Provider_EC_KEY_get0_group(ec_key_st* key) => (ec_group_st*)Provider.EC_KEY_get0_group(key);
    public static bignum_st* Provider_BN_bin2bn(byte* input,int size,bignum_st* output) => (bignum_st*)Provider.BN_bin2bn(input,size,output);
    public static bignum_st* Provider_BN_dup(bignum_st* value) => (bignum_st*)Provider.BN_dup(value);
    public static dh_st* Provider_DH_new() => (dh_st*)Provider.DH_new();
    public static dh_st* Provider_DHparams_dup(dh_st* dh) => (dh_st*)Provider.DHparams_dup(dh);
    public static dh_st* Provider_d2i_DHparams(dh_st** output,byte** input,long length) => (dh_st*)Provider.d2i_DHparams((void**)output,input,length);
    public static void Provider_DH_get0_pqg(dh_st* dh,bignum_st** p,bignum_st** q,bignum_st** g) => Provider.DH_get0_pqg(dh,(void**)p,(void**)q,(void**)g);
    public static void Provider_DH_get0_key(dh_st* dh,bignum_st** publicKey,bignum_st** privateKey) => Provider.DH_get0_key(dh,(void**)publicKey,(void**)privateKey);

    public static asn1_string_st* Provider_ASN1_GENERALIZEDTIME_set(asn1_string_st* value,long time) => (asn1_string_st*)Provider.ASN1_GENERALIZEDTIME_set(value,time);
    public static asn1_string_st* Provider_d2i_ASN1_UTF8STRING(asn1_string_st** output,byte** input,long length) => (asn1_string_st*)Provider.d2i_ASN1_UTF8STRING((void**)output,input,length);
    public static asn1_object_st* Provider_OBJ_txt2obj(byte* input,int noName) => (asn1_object_st*)Provider.OBJ_txt2obj(input,noName);
    public static stack_st* Provider_OPENSSL_sk_new_null() => (stack_st*)Provider.OPENSSL_sk_new_null();
    public static x509_st* Provider_d2i_X509(x509_st** output,byte** input,long length) => (x509_st*)Provider.d2i_X509((void**)output,input,length);
    public static evp_pkey_st* Provider_X509_get_pubkey(x509_st* cert) => (evp_pkey_st*)Provider.X509_get_pubkey(cert);
    public static X509_name_st* Provider_X509_get_subject_name(x509_st* cert) => (X509_name_st*)Provider.X509_get_subject_name(cert);
    public static X509_name_st* Provider_X509_get_issuer_name(x509_st* cert) => (X509_name_st*)Provider.X509_get_issuer_name(cert);
    public static X509_name_entry_st* Provider_X509_NAME_get_entry(X509_name_st* name,int index) => (X509_name_entry_st*)Provider.X509_NAME_get_entry(name,index);
    public static asn1_string_st* Provider_X509_NAME_ENTRY_get_data(X509_name_entry_st* entry) => (asn1_string_st*)Provider.X509_NAME_ENTRY_get_data(entry);
    public static X509_extension_st* Provider_X509_get_ext(x509_st* cert,int index) => (X509_extension_st*)Provider.X509_get_ext(cert,index);
    public static asn1_string_st* Provider_X509_EXTENSION_get_data(X509_extension_st* extension) => (asn1_string_st*)Provider.X509_EXTENSION_get_data(extension);
    public static asn1_object_st* Provider_X509_EXTENSION_get_object(X509_extension_st* extension) => (asn1_object_st*)Provider.X509_EXTENSION_get_object(extension);
    public static x509_store_st* Provider_X509_STORE_new() => (x509_store_st*)Provider.X509_STORE_new();
    public static stack_st_X509_OBJECT* Provider_X509_STORE_get0_objects(x509_store_st* store) => (stack_st_X509_OBJECT*)Provider.X509_STORE_get0_objects(store);
    public static x509_st* Provider_X509_OBJECT_get0_X509(x509_object_st* value) => (x509_st*)Provider.X509_OBJECT_get0_X509(value);
    public static x509_store_ctx_st* Provider_X509_STORE_CTX_new() => (x509_store_ctx_st*)Provider.X509_STORE_CTX_new();
    public static X509_VERIFY_PARAM_st* Provider_X509_STORE_CTX_get0_param(x509_store_ctx_st* ctx) => (X509_VERIFY_PARAM_st*)Provider.X509_STORE_CTX_get0_param(ctx);
    public static stack_st_X509* Provider_X509_STORE_CTX_get0_chain(x509_store_ctx_st* ctx) => (stack_st_X509*)Provider.X509_STORE_CTX_get0_chain(ctx);
    public static void Provider_X509_STORE_CTX_set_verify_cb(x509_store_ctx_st* ctx,delegate*<int,x509_store_ctx_st*,int> callback) => Provider.X509_STORE_CTX_set_verify_cb(ctx,(delegate*<int,void*,int>)callback);
    public static X509_crl_st* Provider_d2i_X509_CRL(X509_crl_st** output,byte** input,long length) => (X509_crl_st*)Provider.d2i_X509_CRL((void**)output,input,length);
    public static X509_name_st* Provider_X509_CRL_get_issuer(X509_crl_st* crl) => (X509_name_st*)Provider.X509_CRL_get_issuer(crl);
    public static asn1_string_st* Provider_X509_CRL_get_lastUpdate(X509_crl_st* crl) => (asn1_string_st*)Provider.X509_CRL_get_lastUpdate(crl);
    public static asn1_string_st* Provider_X509_CRL_get_nextUpdate(X509_crl_st* crl) => (asn1_string_st*)Provider.X509_CRL_get_nextUpdate(crl);

    public static ocsp_response_st* Provider_d2i_OCSP_RESPONSE(ocsp_response_st** output,byte** input,long length) => (ocsp_response_st*)Provider.d2i_OCSP_RESPONSE((void**)output,input,length);
    public static ocsp_basic_response_st* Provider_OCSP_response_get1_basic(ocsp_response_st* response) => (ocsp_basic_response_st*)Provider.OCSP_response_get1_basic(response);
    public static ocsp_cert_id_st* Provider_OCSP_cert_to_id(evp_md_st* digest,x509_st* subject,x509_st* issuer) => (ocsp_cert_id_st*)Provider.OCSP_cert_to_id(digest,subject,issuer);
    public static int Provider_OCSP_resp_find_status(ocsp_basic_response_st* basic,ocsp_cert_id_st* id,int* status,int* reason,asn1_string_st** rev,asn1_string_st** now,asn1_string_st** next) => Provider.OCSP_resp_find_status(basic,id,status,reason,(void**)rev,(void**)now,(void**)next);
}
