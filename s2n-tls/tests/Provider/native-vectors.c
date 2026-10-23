/* Independent native primitive oracle. Link only in this test executable. */
#include <openssl/evp.h>
#include <openssl/pem.h>
#include <openssl/rsa.h>
#include <openssl/dh.h>
#include <openssl/err.h>
#include <openssl/x509v3.h>
#include <openssl/ocsp.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>

#define CHECK(x) do { if (!(x)) { fprintf(stderr, "line %d: %s\n", __LINE__, #x); ERR_print_errors_fp(stderr); return 1; } } while(0)
static unsigned char *unhex(const char *s, size_t *n)
{
    *n = strlen(s) / 2;
    if (strlen(s) % 2) return NULL;
    unsigned char *p = malloc(*n + 1);
    if (!p) return NULL;
    for (size_t i = 0; i < *n; i++) { unsigned int v; if (sscanf(s + i * 2, "%2x", &v) != 1) { free(p); return NULL; } p[i] = v; }
    return p;
}
static void printhex(const unsigned char *s, size_t n) { for (size_t i=0; i<n; i++) printf("%02x", s[i]); putchar('\n'); }
static EVP_PKEY *readkey(const char *path, int public_only)
{
    FILE *f = fopen(path, "rb"); if (!f) return NULL;
    EVP_PKEY *key = public_only ? PEM_read_PUBKEY(f, NULL, NULL, NULL) : PEM_read_PrivateKey(f, NULL, NULL, NULL);
    fclose(f); return key;
}
static X509 *readcert(const char *path){FILE *f=fopen(path,"rb");if(!f)return NULL;X509 *x=PEM_read_X509(f,NULL,NULL,NULL);fclose(f);return x;}
static void printtime(ASN1_GENERALIZEDTIME *time){if(!time){printf("-");return;}fwrite(ASN1_STRING_get0_data(time),1,ASN1_STRING_length(time),stdout);}
static int ignore_crl_time(int ok,X509_STORE_CTX *ctx){int e=X509_STORE_CTX_get_error(ctx);return e>=11&&e<=16&&e!=13&&e!=14?1:ok;}
int main(int argc, char **argv)
{
    CHECK(argc >= 2);
    if (!strcmp(argv[1], "version")) { puts(OpenSSL_version(OPENSSL_VERSION)); return 0; }
    unsigned char out[131072]; size_t outlen = sizeof(out), n=0, kn=0, ivn=0, an=0;
    if (!strcmp(argv[1], "asn1-utf8")) {
        CHECK(argc == 5);
        unsigned char *input = unhex(argv[3], &n); CHECK(input);
        ASN1_STRING *value = NULL;
        if (strcmp(argv[2], "null")) {
            value = ASN1_STRING_type_new(atoi(argv[2])); CHECK(value);
            CHECK(ASN1_STRING_set(value, input, (int)n) == 1);
            value->length = atoi(argv[4]);
        }
        unsigned char *converted = (unsigned char *)1;
        ERR_clear_error();
        int result = ASN1_STRING_to_UTF8(&converted, value);
        int error = ERR_get_error() != 0;
        while (ERR_get_error()) { }
        printf("%d %d %d ", result, error, converted == (unsigned char *)1);
        if (result >= 0) { printhex(converted, result + 1); OPENSSL_free(converted); }
        else putchar('\n');
        ASN1_STRING_free(value); free(input); return 0;
    }
    if (!strcmp(argv[1], "base64-encode") || !strcmp(argv[1], "base64-decode")) {
        CHECK(argc==3);unsigned char *in=unhex(argv[2],&n);CHECK(in&&n<=64);
        unsigned char guarded[128];memset(guarded,0xa5,sizeof(guarded));ERR_clear_error();
        int result=!strcmp(argv[1],"base64-encode")?EVP_EncodeBlock(guarded+8,in,n):EVP_DecodeBlock(guarded+8,in,n);
        printf("%d %lu ",result,ERR_get_error());printhex(guarded,sizeof(guarded));free(in);return 0;
    }
    if (!strcmp(argv[1], "hash")) {
        CHECK(argc==4); unsigned char *in = unhex(argv[3], &n); CHECK(in);
        if(!strcmp(argv[2],"shake256-64")){EVP_MD_CTX *ctx=EVP_MD_CTX_new();CHECK(ctx&&EVP_DigestInit_ex(ctx,EVP_shake256(),NULL)==1&&EVP_DigestUpdate(ctx,in,n)==1&&EVP_DigestFinalXOF(ctx,out,64)==1);printhex(out,64);EVP_MD_CTX_free(ctx);free(in);return 0;}
        const EVP_MD *md = EVP_get_digestbyname(argv[2]); CHECK(md);
        unsigned int length=0; CHECK(EVP_Digest(in,n,out,&length,md,NULL)==1); printhex(out,length); free(in); return 0;
    }
    if (!strcmp(argv[1], "hash-xof")) {
        CHECK(argc==4);unsigned char *in=unhex(argv[2],&n);CHECK(in);size_t size=strtoul(argv[3],NULL,10);CHECK(size<=sizeof(out));EVP_MD_CTX *ctx=EVP_MD_CTX_new();CHECK(ctx&&EVP_DigestInit_ex(ctx,EVP_shake256(),NULL)==1&&EVP_DigestUpdate(ctx,in,n)==1&&EVP_DigestFinalXOF(ctx,out,size)==1);printhex(out,size);EVP_MD_CTX_free(ctx);free(in);return 0;
    }
    if (!strcmp(argv[1], "cipher")) {
        CHECK(argc==8); const EVP_CIPHER *cipher=EVP_get_cipherbyname(argv[2]); CHECK(cipher);
        unsigned char *key=unhex(argv[3],&kn), *iv=unhex(argv[4],&ivn), *in=unhex(argv[5],&n); CHECK(key&&iv&&in);
        CHECK(kn==(size_t)EVP_CIPHER_key_length(cipher)); CHECK(ivn==(size_t)EVP_CIPHER_iv_length(cipher));
        int split=atoi(argv[6]), enc=atoi(argv[7]), l=0, total=0; CHECK(split>=0 && (size_t)split<=n && n<sizeof(out)-64);
        EVP_CIPHER_CTX *ctx=EVP_CIPHER_CTX_new(); CHECK(ctx);
        CHECK(EVP_CipherInit_ex(ctx,cipher,NULL,key,iv,enc)==1); CHECK(EVP_CIPHER_CTX_set_padding(ctx,0)==1);
        CHECK(EVP_CipherUpdate(ctx,out,&l,in,split)==1); total+=l;
        CHECK(EVP_CipherUpdate(ctx,out+total,&l,in+split,(int)n-split)==1); total+=l;
        CHECK(EVP_CipherFinal_ex(ctx,out+total,&l)==1); total+=l;
        printhex(out,total); EVP_CIPHER_CTX_free(ctx); free(key);free(iv);free(in);return 0;
    }
    if (!strcmp(argv[1], "aead")) {
        CHECK(argc==8); const EVP_CIPHER *cipher=EVP_get_cipherbyname(argv[2]); CHECK(cipher);
        unsigned char *key=unhex(argv[3],&kn),*iv=unhex(argv[4],&ivn),*in=unhex(argv[5],&n),*aad=unhex(argv[6],&an);
        CHECK(key&&iv&&in&&aad&&kn==(size_t)EVP_CIPHER_key_length(cipher)&&n<sizeof(out)-32);
        int enc=!strcmp(argv[7],"-"), l=0,total=0; EVP_CIPHER_CTX *ctx=EVP_CIPHER_CTX_new(); CHECK(ctx);
        CHECK(EVP_CipherInit_ex(ctx,cipher,NULL,NULL,NULL,enc)==1); CHECK(EVP_CIPHER_CTX_ctrl(ctx,EVP_CTRL_AEAD_SET_IVLEN,ivn,NULL)==1);
        CHECK(EVP_CipherInit_ex(ctx,NULL,NULL,key,iv,enc)==1);
        if (!enc) { size_t tn; unsigned char *tag=unhex(argv[7],&tn); CHECK(tag&&tn==16); CHECK(EVP_CIPHER_CTX_ctrl(ctx,EVP_CTRL_AEAD_SET_TAG,16,tag)==1); free(tag); }
        CHECK(EVP_CipherUpdate(ctx,NULL,&l,aad,an)==1); CHECK(EVP_CipherUpdate(ctx,out,&l,in,n)==1);total+=l;
        int ok=EVP_CipherFinal_ex(ctx,out+total,&l); if (!enc && !ok) { puts("REJECT"); EVP_CIPHER_CTX_free(ctx);free(key);free(iv);free(in);free(aad); return 0; }
        CHECK(ok==1);total+=l;
        if (enc) { CHECK(EVP_CIPHER_CTX_ctrl(ctx,EVP_CTRL_AEAD_GET_TAG,16,out+total)==1); total+=16; }
        printhex(out,total);EVP_CIPHER_CTX_free(ctx);free(key);free(iv);free(in);free(aad);return 0;
    }
    if (!strncmp(argv[1],"rsa-",4) || !strncmp(argv[1],"ecdsa-",6)) {
        int sign=strstr(argv[1],"sign")!=NULL, verify=strstr(argv[1],"verify")!=NULL, enc=strstr(argv[1],"encrypt")!=NULL;
        CHECK(argc==(verify?7:(sign?6:4)));
        EVP_PKEY *key=readkey(argv[2],verify||enc);CHECK(key);EVP_PKEY_CTX *ctx=EVP_PKEY_CTX_new(key,NULL);CHECK(ctx);
        const char *hex=(sign||verify)?argv[5]:argv[3]; unsigned char *in=unhex(hex,&n);CHECK(in);
        if (sign||verify) {
            CHECK((sign?EVP_PKEY_sign_init(ctx):EVP_PKEY_verify_init(ctx))==1);
            const EVP_MD *md=EVP_get_digestbyname(argv[3]);CHECK(md);CHECK(EVP_PKEY_CTX_set_signature_md(ctx,md)==1);
            if (!strncmp(argv[1],"rsa-",4)) {
                int pss=!strcmp(argv[4],"pss"); CHECK(EVP_PKEY_CTX_set_rsa_padding(ctx,pss?RSA_PKCS1_PSS_PADDING:RSA_PKCS1_PADDING)==1);
                if (pss) CHECK(EVP_PKEY_CTX_set_rsa_pss_saltlen(ctx,RSA_PSS_SALTLEN_DIGEST)==1);
            }
            if (sign) { CHECK(EVP_PKEY_sign(ctx,out,&outlen,in,n)==1);printhex(out,outlen); }
            else { size_t sn;unsigned char *sig=unhex(argv[6],&sn);CHECK(sig);printf("%d\n",EVP_PKEY_verify(ctx,sig,sn,in,n)==1);free(sig); }
        } else {
            CHECK((enc?EVP_PKEY_encrypt_init(ctx):EVP_PKEY_decrypt_init(ctx))==1);CHECK(EVP_PKEY_CTX_set_rsa_padding(ctx,strstr(argv[1],"raw")?RSA_NO_PADDING:RSA_PKCS1_PADDING)==1);
            CHECK((enc?EVP_PKEY_encrypt(ctx,out,&outlen,in,n):EVP_PKEY_decrypt(ctx,out,&outlen,in,n))==1);printhex(out,outlen);
        }
        free(in);EVP_PKEY_CTX_free(ctx);EVP_PKEY_free(key);return 0;
    }
    if (!strcmp(argv[1],"dh-check")) {
        CHECK(argc==4);BIGNUM *p=NULL,*g=NULL;CHECK(BN_hex2bn(&p,argv[2]));CHECK(BN_hex2bn(&g,argv[3]));DH *dh=DH_new();CHECK(dh);CHECK(DH_set0_pqg(dh,p,NULL,g)==1);int codes=0;CHECK(DH_check(dh,&codes)==1);printf("%d\n",codes);DH_free(dh);return 0;
    }
    if (!strcmp(argv[1],"dh")) {
        CHECK(argc==6); BIGNUM *p=NULL,*g=NULL,*x=NULL,*peer=NULL;CHECK(BN_hex2bn(&p,argv[2]));CHECK(BN_hex2bn(&g,argv[3]));CHECK(BN_hex2bn(&x,argv[4]));CHECK(BN_hex2bn(&peer,argv[5]));
        DH *dh=DH_new();CHECK(dh);CHECK(DH_set0_pqg(dh,p,NULL,g)==1);CHECK(DH_set0_key(dh,NULL,x)==1);
        int len=DH_compute_key(out,peer,dh);CHECK(len>0);printhex(out,len);DH_free(dh);BN_free(peer);return 0;
    }
    if (!strcmp(argv[1],"ecdh")) {
        CHECK(argc==4);EVP_PKEY *key=readkey(argv[2],0),*peer=readkey(argv[3],1);CHECK(key&&peer);
        EVP_PKEY_CTX *ctx=EVP_PKEY_CTX_new(key,NULL);CHECK(ctx);CHECK(EVP_PKEY_derive_init(ctx)==1);CHECK(EVP_PKEY_derive_set_peer(ctx,peer)==1);CHECK(EVP_PKEY_derive(ctx,out,&outlen)==1);printhex(out,outlen);
        EVP_PKEY_CTX_free(ctx);EVP_PKEY_free(key);EVP_PKEY_free(peer);return 0;
    }
    if (!strcmp(argv[1],"cert")) {
        CHECK(argc==3);FILE *f=fopen(argv[2],"rb");CHECK(f);X509 *cert=PEM_read_X509(f,NULL,NULL,NULL);fclose(f);CHECK(cert);
        unsigned char *der=NULL; int len=i2d_X509(cert,&der);CHECK(len>0);printhex(der,len);OPENSSL_free(der);X509_free(cert);return 0;
    }
    if (!strcmp(argv[1],"x509-meta")) {
        CHECK(argc==3);X509 *x=readcert(argv[2]);CHECK(x);printf("%lu %lu %d %d %d %d\n",X509_NAME_hash(X509_get_subject_name(x)),X509_issuer_name_hash(x),X509_get_signature_nid(x),X509_get_ext_count(x),X509_check_purpose(x,1,0),X509_check_purpose(x,2,0));X509_free(x);return 0;
    }
    if (!strcmp(argv[1],"x509-purpose")) {
        CHECK(argc==4);X509 *x=readcert(argv[2]);CHECK(x);int ca=atoi(argv[3]);printf("%d %d\n",X509_check_purpose(x,1,ca),X509_check_purpose(x,2,ca));X509_free(x);return 0;
    }
    if (!strcmp(argv[1],"x509-chain")) {
        CHECK(argc==4);X509 *root=readcert(argv[3]);CHECK(root);FILE *file=fopen(argv[2],"rb");CHECK(file);X509 *leaf=PEM_read_X509(file,NULL,NULL,NULL);CHECK(leaf);STACK_OF(X509) *wire=sk_X509_new_null();CHECK(wire);X509 *item;while((item=PEM_read_X509(file,NULL,NULL,NULL))!=NULL)CHECK(sk_X509_push(wire,item));fclose(file);ERR_clear_error();X509_STORE *store=X509_STORE_new();CHECK(store&&X509_STORE_add_cert(store,root)==1);X509_STORE_CTX *ctx=X509_STORE_CTX_new();CHECK(ctx&&X509_STORE_CTX_init(ctx,store,leaf,wire)==1);X509_STORE_CTX_set_flags(ctx,X509_V_FLAG_PARTIAL_CHAIN);int ok=X509_verify_cert(ctx);printf("%d %d\n",ok,X509_STORE_CTX_get_error(ctx));X509_STORE_CTX_free(ctx);X509_STORE_free(store);sk_X509_pop_free(wire,X509_free);X509_free(leaf);X509_free(root);return 0;
    }
    if (!strcmp(argv[1],"ocsp-basic")) {
        CHECK(argc==4);X509 *root=readcert(argv[2]);CHECK(root);BIO *file=BIO_new_file(argv[3],"rb");CHECK(file);OCSP_RESPONSE *response=d2i_OCSP_RESPONSE_bio(file,NULL);BIO_free(file);CHECK(response);OCSP_BASICRESP *basic=OCSP_response_get1_basic(response);CHECK(basic);X509_STORE *store=X509_STORE_new();CHECK(store&&X509_STORE_add_cert(store,root)==1);printf("%d\n",OCSP_basic_verify(basic,NULL,store,0)>0);X509_STORE_free(store);OCSP_BASICRESP_free(basic);OCSP_RESPONSE_free(response);X509_free(root);return 0;
    }
    if (!strcmp(argv[1],"x509-verify")) {
        CHECK(argc==6);X509 *leaf=readcert(argv[2]),*root=readcert(argv[3]);CHECK(leaf&&root);X509_STORE *store=X509_STORE_new();CHECK(store);CHECK(X509_STORE_add_cert(store,root)==1);X509_STORE_CTX *ctx=X509_STORE_CTX_new();CHECK(ctx);CHECK(X509_STORE_CTX_init(ctx,store,leaf,NULL)==1);unsigned long flags=strtoul(argv[5],NULL,10);X509_STORE_CTX_set_flags(ctx,flags);if(!(flags&X509_V_FLAG_NO_CHECK_TIME))X509_STORE_CTX_set_time(ctx,0,strtoll(argv[4],NULL,10));int ok=X509_verify_cert(ctx);printf("%d %d\n",ok,X509_STORE_CTX_get_error(ctx));X509_STORE_CTX_free(ctx);X509_STORE_free(store);X509_free(leaf);X509_free(root);return 0;
    }
    if (!strcmp(argv[1],"ocsp")) {
        CHECK(argc==5);X509 *leaf=readcert(argv[2]),*root=readcert(argv[3]);CHECK(leaf&&root);BIO *file=BIO_new_file(argv[4],"rb");CHECK(file);OCSP_RESPONSE *response=d2i_OCSP_RESPONSE_bio(file,NULL);BIO_free(file);CHECK(response);OCSP_BASICRESP *basic=OCSP_response_get1_basic(response);CHECK(basic);X509_STORE *store=X509_STORE_new();CHECK(store&&X509_STORE_add_cert(store,root)==1);STACK_OF(X509) *chain=sk_X509_new_null();CHECK(chain&&sk_X509_push(chain,leaf)&&sk_X509_push(chain,root));int verified=OCSP_basic_verify(basic,chain,store,0);OCSP_CERTID *id=OCSP_cert_to_id(EVP_sha1(),leaf,root);CHECK(id);int status=-1,reason=-1;ASN1_GENERALIZEDTIME *rev=NULL,*now=NULL,*next=NULL;CHECK(OCSP_resp_find_status(basic,id,&status,&reason,&rev,&now,&next)==1);printf("%d %d ",verified,status);printtime(now);putchar(' ');printtime(next);putchar('\n');OCSP_CERTID_free(id);sk_X509_free(chain);X509_STORE_free(store);OCSP_BASICRESP_free(basic);OCSP_RESPONSE_free(response);X509_free(leaf);X509_free(root);return 0;
    }
    if (!strcmp(argv[1],"crl")) {
        CHECK(argc>=6&&argc<=8);X509 *leaf=readcert(argv[2]),*root=readcert(argv[3]);CHECK(leaf&&root);FILE *file=fopen(argv[4],"rb");CHECK(file);X509_CRL *crl=PEM_read_X509_CRL(file,NULL,NULL,NULL);fclose(file);CHECK(crl);X509_STORE *store=X509_STORE_new();CHECK(store&&X509_STORE_add_cert(store,root)==1);X509_STORE_CTX *ctx=X509_STORE_CTX_new();CHECK(ctx&&X509_STORE_CTX_init(ctx,store,leaf,NULL)==1);STACK_OF(X509_CRL) *crls=sk_X509_CRL_new_null();CHECK(crls&&sk_X509_CRL_push(crls,crl));X509_STORE_CTX_set0_crls(ctx,crls);X509_STORE_CTX_set_flags(ctx,argc>=7?strtoul(argv[6],NULL,10):X509_V_FLAG_CRL_CHECK|X509_V_FLAG_PARTIAL_CHAIN);X509_STORE_CTX_set_time(ctx,0,strtoll(argv[5],NULL,10));if(argc==8)X509_STORE_CTX_set_verify_cb(ctx,ignore_crl_time);int ok=X509_verify_cert(ctx);printf("%d %d\n",ok,X509_STORE_CTX_get_error(ctx));X509_STORE_CTX_free(ctx);X509_STORE_free(store);sk_X509_CRL_free(crls);X509_CRL_free(crl);X509_free(leaf);X509_free(root);return 0;
    }
    fprintf(stderr,"unknown operation: %s\n",argv[1]);return 2;
}
