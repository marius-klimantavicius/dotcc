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
static int rsa_contract_width;
static unsigned long rsa_contract_seed;
static void rsa_contract_clear(void) { while(ERR_get_error()){} }
static void rsa_contract_check(EVP_PKEY_CTX *ctx,const char *label,const unsigned char *input,int count,int capacity,int query,int seeded,int null_input)
{
    rsa_contract_clear();size_t length=(size_t)capacity;
    if(seeded){EVP_PKEY_decrypt(NULL,NULL,&length,NULL,0);length=(size_t)capacity;}
    unsigned char output[4099];memset(output,0xA5,sizeof(output));
    int result=EVP_PKEY_decrypt(ctx,query?NULL:output+1,&length,null_input?NULL:input,count<0?(size_t)-1:(size_t)count);
    unsigned long error;int errors=0,preserved=!seeded;
    while((error=ERR_get_error())!=0){if(errors==0&&seeded)preserved=error==rsa_contract_seed;errors++;}
    printf("%s|%d|%d|%zu|%d|%d|",label,seeded,result,length,errors>(seeded?1:0),preserved);
    for(int i=0;i<rsa_contract_width+3;i++)printf("%02X",output[i]);
    putchar('\n');
}
static void rsa_contract_both(EVP_PKEY_CTX *ctx,const char *label,const unsigned char *input,int count,int capacity,int query,int null_input)
{ rsa_contract_check(ctx,label,input,count,capacity,query,0,null_input);rsa_contract_check(ctx,label,input,count,capacity,query,1,null_input); }
static void rsa_contract_init(EVP_PKEY_CTX *ctx,const char *label)
{
    rsa_contract_clear();int result=EVP_PKEY_decrypt_init(ctx);unsigned long error;int errors=0;
    while((error=ERR_get_error())!=0){errors++;}
    printf("init:%s|%d|%d\n",label,result,errors>0);
}
static int rsa_private_contract(int argc,char **argv)
{
    if(argc!=3||strncmp(OpenSSL_version(0),argv[2],strlen(argv[2])))return 2;
    FILE *file=fopen(argv[1],"rb");if(!file)return 3;
    EVP_PKEY *key=PEM_read_PrivateKey(file,NULL,NULL,NULL);fclose(file);if(!key)return 4;
    RSA *rsa=EVP_PKEY_get1_RSA(key);if(!rsa)return 5;rsa_contract_width=RSA_size(rsa);if(rsa_contract_width>4096)return 6;
    const BIGNUM *modulus;RSA_get0_key(rsa,&modulus,NULL,NULL);
    size_t seedlen=0;EVP_PKEY_decrypt(NULL,NULL,&seedlen,NULL,0);rsa_contract_seed=ERR_get_error();if(!rsa_contract_seed||ERR_get_error())return 7;
    EVP_PKEY_CTX *ctx=EVP_PKEY_CTX_new(key,NULL),*retained=EVP_PKEY_CTX_new(key,NULL);if(!ctx||!retained)return 8;
    unsigned char zeros[4097]={0};char label[128];
    rsa_contract_both(ctx,"before-init",zeros,rsa_contract_width,rsa_contract_width,0,0);rsa_contract_both(ctx,"before-init-query",zeros,rsa_contract_width,0,1,0);
    if(EVP_PKEY_encrypt_init(ctx)!=1)return 9;
    rsa_contract_both(ctx,"encrypt-operation",zeros,rsa_contract_width,rsa_contract_width,0,0);
    rsa_contract_init(ctx,"rsa");rsa_contract_init(retained,"retained");EVP_PKEY_free(key);
    int paddings[]={3,1},lengths[]={0,1,rsa_contract_width-1,rsa_contract_width,rsa_contract_width+1},capacities[]={0,1,rsa_contract_width-1,rsa_contract_width,rsa_contract_width+1};
    for(int p=0;p<2;p++){
        int padding=paddings[p];if(EVP_PKEY_CTX_set_rsa_padding(ctx,padding)!=1)return 10;
        for(int kind=0;kind<5;kind++){
            BIGNUM *value=BN_dup(modulus);if(kind==0)BN_zero(value);else if(kind==1)BN_one(value);else if(kind==2)BN_sub_word(value,1);else if(kind==4)BN_add_word(value,1);
            unsigned char full[4096];BN_bn2binpad(value,full,rsa_contract_width);
            for(int n=0;n<5;n++){
                int count=lengths[n];unsigned char input[4097]={0};
                if(count>=rsa_contract_width)memcpy(input+count-rsa_contract_width,full,rsa_contract_width);else if(count)memcpy(input,full+rsa_contract_width-count,count);
                for(int c=0;c<5;c++){
                    int capacity=capacities[c];snprintf(label,sizeof(label),"p%d:v%d:n%d:cap%d",padding,kind,count,capacity);
                    rsa_contract_both(ctx,label,input,count,capacity,0,0);
                }
                snprintf(label,sizeof(label),"p%d:v%d:n%d:query",padding,kind,count);rsa_contract_both(ctx,label,input,count,7,1,0);
            }
            BN_free(value);
        }
        snprintf(label,sizeof(label),"p%d:null-zero",padding);rsa_contract_both(ctx,label,zeros,0,rsa_contract_width,0,1);
        snprintf(label,sizeof(label),"p%d:query-invalid-input",padding);rsa_contract_both(ctx,label,zeros,-1,0,1,1);
        snprintf(label,sizeof(label),"p%d:small-invalid-input",padding);rsa_contract_both(ctx,label,zeros,-1,rsa_contract_width-1,0,1);
    }
    for(int mutation=-1;mutation<rsa_contract_width;mutation++){
        unsigned char encoded[4096],cipher[4096];memset(encoded,0x7F,rsa_contract_width);encoded[0]=0;encoded[1]=2;
        int delimiter=rsa_contract_width-49;encoded[delimiter]=0;for(int i=0;i<48;i++)encoded[delimiter+1+i]=(unsigned char)i;
        if(mutation>=0){if(mutation>1&&mutation<delimiter)encoded[mutation]=0;else encoded[mutation]++;}
        if(RSA_public_encrypt(rsa_contract_width,encoded,cipher,rsa,RSA_NO_PADDING)!=rsa_contract_width)return 11;
        snprintf(label,sizeof(label),"pkcs1-mutation-%d",mutation);rsa_contract_both(ctx,label,cipher,rsa_contract_width,rsa_contract_width,0,0);
    }
    int delimiters[]={2,3,8,9,10,11,rsa_contract_width-2,rsa_contract_width-1,rsa_contract_width};
    for(int d=0;d<9;d++){
        int delimiter=delimiters[d];unsigned char encoded[4096],cipher[4096];memset(encoded,0x7F,rsa_contract_width);encoded[0]=0;encoded[1]=2;
        if(delimiter<rsa_contract_width)encoded[delimiter]=0;
        if(RSA_public_encrypt(rsa_contract_width,encoded,cipher,rsa,RSA_NO_PADDING)!=rsa_contract_width)return 12;
        snprintf(label,sizeof(label),"pkcs1-delimiter-%d",delimiter);rsa_contract_both(ctx,label,cipher,rsa_contract_width,rsa_contract_width,0,0);
    }
    EVP_PKEY_CTX_free(ctx);rsa_contract_both(retained,"other-context-survives",zeros,rsa_contract_width,rsa_contract_width,0,0);
    if(EVP_PKEY_sign_init(retained)!=1)return 13;
    rsa_contract_both(retained,"sign-operation",zeros,rsa_contract_width,rsa_contract_width,0,0);
    rsa_contract_init(retained,"reinit");if(EVP_PKEY_CTX_set_rsa_padding(retained,3)!=1)return 14;
    rsa_contract_both(retained,"reinit-raw",zeros,rsa_contract_width,rsa_contract_width,0,0);EVP_PKEY_CTX_free(retained);RSA_free(rsa);
    int types[]={6,408,1034};for(int t=0;t<3;t++){
        int type=types[t];EVP_PKEY_CTX *missing=EVP_PKEY_CTX_new_id(type,NULL);if(!missing)return 15;
        snprintf(label,sizeof(label),"id-%d",type);rsa_contract_init(missing,label);rsa_contract_both(missing,label,zeros,rsa_contract_width,rsa_contract_width,0,0);
        snprintf(label,sizeof(label),"id-query-%d",type);rsa_contract_both(missing,label,zeros,rsa_contract_width,0,1,0);EVP_PKEY_CTX_free(missing);
    }
    rsa_contract_init(NULL,"null");rsa_contract_both(NULL,"null-context",zeros,rsa_contract_width,rsa_contract_width,0,0);if(ERR_get_error())return 16;return 0;
}

int main(int argc, char **argv)
{
    CHECK(argc >= 2);
    if (!strcmp(argv[1], "rsa-private-contract")) return rsa_private_contract(argc-1,argv+1);
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
