#include <stddef.h>
#include <stdint.h>
#include <string.h>
#include <openssl/asn1.h>
#include <openssl/x509v3.h>
#include <openssl/evp.h>
#include <openssl/md5.h>
#include <openssl/sha.h>
#include <openssl/ec.h>
#include <openssl/dh.h>
#include <openssl/ocsp.h>
#include "dotcc_provider.h"

size_t provider_layout_value(int field)
{
    switch(field) {
    case 0:return sizeof(void*);case 1:return sizeof(long);case 2:return sizeof(size_t);
    case 3:return sizeof(ASN1_STRING);case 4:return _Alignof(ASN1_STRING);
    case 5:return offsetof(ASN1_STRING,length);case 6:return offsetof(ASN1_STRING,type);
    case 7:return offsetof(ASN1_STRING,data);case 8:return offsetof(ASN1_STRING,flags);
    case 9:return sizeof(GENERAL_NAME);case 10:return _Alignof(GENERAL_NAME);
    case 11:return offsetof(GENERAL_NAME,type);case 12:return offsetof(GENERAL_NAME,d);
    case 13:return sizeof(MD5_CTX);case 14:return sizeof(SHA_CTX);case 15:return sizeof(SHA256_CTX);case 16:return sizeof(SHA512_CTX);
    default:return (size_t)-1;
    }
}

typedef int (*provider_callback)(void*,const unsigned char*,size_t);
static provider_callback saved_callback;
static void *saved_context;
int provider_register_callback(provider_callback callback,void *context)
{
    if(!callback||!context)return 0;
    saved_callback=callback;saved_context=context;return 1;
}
int provider_invoke_callback(const unsigned char *bytes,size_t length)
{
    if(!saved_callback)return -1;
    return saved_callback(saved_context,bytes,length);
}
int provider_callback_identity(provider_callback callback){return saved_callback==callback;}
void provider_clear_callback(void){saved_callback=NULL;saved_context=NULL;}

#if !defined(PROVIDER_NATIVE_PROBE)
uint64_t provider_runtime_contract_probe(void)
{
    const char *name=dotcc_crypto_provider_name();
    if(dotcc_crypto_provider_abi()!=DOTCC_CRYPTO_PROVIDER_ABI||!name
        ||memcmp(name,"DotCC managed s2n crypto",23)!=0
        ||OPENSSL_init_crypto(OPENSSL_INIT_LOAD_CRYPTO_STRINGS|OPENSSL_INIT_ADD_ALL_CIPHERS|OPENSSL_INIT_ADD_ALL_DIGESTS,NULL)!=1)return 0;
    return dotcc_crypto_provider_capabilities();
}
#endif

int provider_digest_probe(const unsigned char *bytes,size_t length,unsigned char *digest)
{
    EVP_MD_CTX *first=EVP_MD_CTX_new(),*copy=EVP_MD_CTX_new();
    if(!first||!copy){EVP_MD_CTX_free(first);EVP_MD_CTX_free(copy);return 0;}
    unsigned int size=0;
    int ok=EVP_DigestInit_ex(first,EVP_md5_sha1(),NULL)==1
        &&EVP_DigestUpdate(first,bytes,length/2)==1
        &&EVP_MD_CTX_copy_ex(copy,first)==1
        &&EVP_DigestUpdate(first,bytes+length/2,length-length/2)==1
        &&EVP_DigestFinal_ex(first,digest,&size)==1&&size==36;
    unsigned char other[36];
    ok=ok&&EVP_DigestUpdate(copy,bytes+length/2,length-length/2)==1
        &&EVP_DigestFinal_ex(copy,other,&size)==1&&size==36
        &&memcmp(digest,other,36)==0;
    EVP_MD_CTX_free(first);EVP_MD_CTX_free(copy);return ok;
}

int provider_shake_probe(const unsigned char *bytes,size_t length,unsigned char *digest)
{
    EVP_MD_CTX *first=EVP_MD_CTX_new(),*copy=EVP_MD_CTX_new();unsigned char other[64];
    int ok=first&&copy&&EVP_MD_size(EVP_shake256())==32
        &&EVP_DigestInit_ex(first,EVP_shake256(),NULL)==1
        &&EVP_DigestUpdate(first,bytes,length/2)==1&&EVP_MD_CTX_copy_ex(copy,first)==1
        &&EVP_DigestUpdate(first,bytes+length/2,length-length/2)==1&&EVP_DigestFinalXOF(first,digest,64)==1
        &&EVP_DigestUpdate(copy,bytes+length/2,length-length/2)==1&&EVP_DigestFinalXOF(copy,other,64)==1
        &&memcmp(digest,other,64)==0;
    EVP_MD_CTX_free(first);EVP_MD_CTX_free(copy);return ok;
}

/* Read actual exposed vendor fields through emitted C, not a C# mirror. */
int provider_certificate_san_probe(const unsigned char *der,size_t length,unsigned char *dns,int capacity)
{
    const unsigned char *cursor=der;
    X509 *certificate=d2i_X509(NULL,&cursor,(long)length);
    if(!certificate||cursor!=der+length){X509_free(certificate);return -1;}
    GENERAL_NAMES *names=X509_get_ext_d2i(certificate,NID_subject_alt_name,NULL,NULL);
    int result=-1;
    if(names)for(int i=0;i<sk_GENERAL_NAME_num(names);i++) {
        GENERAL_NAME *name=sk_GENERAL_NAME_value(names,i);
        if(name->type==GEN_DNS){result=ASN1_STRING_length(name->d.dNSName);if(result>=0&&result<=capacity)memcpy(dns,ASN1_STRING_get0_data(name->d.dNSName),result);else result=-1;break;}
    }
    GENERAL_NAMES_free(names);X509_free(certificate);return result;
}

#if defined(PROVIDER_NATIVE_PROBE)
#include <stdio.h>
static int native_callback(void *context,const unsigned char *bytes,size_t length)
{
    int *sum=context;for(size_t i=0;i<length;i++)*sum+=bytes[i];return *sum;
}
int main(void)
{
    for(int i=0;i<17;i++)printf("%zu\n",provider_layout_value(i));
    unsigned char bytes[]={0,1,128,255},digest[36];int sum=7;
    if(!provider_register_callback(native_callback,&sum)||!provider_callback_identity(native_callback)||provider_invoke_callback(bytes,sizeof(bytes))!=391)return 1;
    provider_clear_callback();if(provider_invoke_callback(bytes,sizeof(bytes))!=-1)return 2;
    if(!provider_digest_probe(bytes,sizeof(bytes),digest))return 3;
    for(int i=0;i<36;i++)printf("%02x",digest[i]);putchar('\n');
    unsigned char shake[64];if(!provider_shake_probe(bytes,sizeof(bytes),shake))return 4;
    for(int i=0;i<64;i++)printf("%02x",shake[i]);putchar('\n');return 0;
}
#endif
