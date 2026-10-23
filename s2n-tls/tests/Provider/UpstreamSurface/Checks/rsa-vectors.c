#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <openssl/bn.h>
#include <openssl/err.h>
#include <openssl/evp.h>
#include <openssl/pem.h>
#include <openssl/rsa.h>

static int errors(void) { int found=0; while(ERR_get_error()) found=1; return found; }
static void hex(const unsigned char *bytes,int length) { for(int i=0;i<length;i++) printf("%02X",bytes[i]); }
static void raw(RSA *rsa,const char *label,const unsigned char *input,int length,int padding)
{
    int size=RSA_size(rsa); unsigned char output[4098]; memset(output,0xA5,sizeof(output)); errors();
    int result=RSA_public_encrypt(length,input,output+1,rsa,padding); int error=errors();
    printf("raw:%s|%d|%d|%d|",label,result,error,output[0]==0xA5&&output[size+1]==0xA5);hex(output+1,size);puts("");
}
int main(int argc,char **argv)
{
    if((argc!=3&&argc!=4)||strncmp(OpenSSL_version(0),argv[2],strlen(argv[2]))) return 2;
    FILE *file=fopen(argv[1],"rb"); if(!file)return 3;
    EVP_PKEY *pkey=PEM_read_PrivateKey(file,NULL,NULL,NULL);fclose(file);if(!pkey)return 4;
    if(argc==4&&strcmp(argv[3],"private-edges")==0){
        EVP_PKEY_CTX *ctx=EVP_PKEY_CTX_new(pkey,NULL);if(!ctx||EVP_PKEY_decrypt_init(ctx)!=1||EVP_PKEY_CTX_set_rsa_padding(ctx,RSA_NO_PADDING)!=1)return 7;
        int size=EVP_PKEY_size(pkey);unsigned char input[4096],output[4098];RSA *view=EVP_PKEY_get1_RSA(pkey);const BIGNUM *n=NULL;RSA_get0_key(view,&n,NULL,NULL);
        for(int kind=0;kind<4;kind++){
            memset(input,0,sizeof(input));if(kind==1)input[size-1]=1;else if(kind>=2){BIGNUM *v=BN_dup(n);if(kind==2)BN_sub_word(v,1);BN_bn2binpad(v,input,size);BN_free(v);}
            memset(output,0xA5,sizeof(output));size_t length=(size_t)size;errors();int result=EVP_PKEY_decrypt(ctx,output+1,&length,input,(size_t)size);
            printf("private:%d|%d|%zu|%d|%d|",kind,result,length,errors(),output[0]==0xA5&&output[size+1]==0xA5);hex(output+1,size);puts("");
        }
        RSA_free(view);EVP_PKEY_CTX_free(ctx);EVP_PKEY_free(pkey);return 0;
    }
    RSA *first=EVP_PKEY_get1_RSA(pkey),*rsa=EVP_PKEY_get1_RSA(pkey); if(!rsa)return 5;
    int size=RSA_size(rsa);if(size>4096)return 6;printf("alias|%d|%d|%d\n",first==rsa,size,errors());
    EVP_PKEY_free(pkey);RSA_free(first);printf("retained|%d|%d\n",RSA_size(rsa),errors());
    unsigned char input[4097];char label[64];memset(input,0,sizeof(input));raw(rsa,"zero",input,size,3);
    input[size-1]=1;raw(rsa,"one",input,size,3);
    const BIGNUM *n=NULL;RSA_get0_key(rsa,&n,NULL,NULL);BIGNUM *minus=BN_dup(n);BN_sub_word(minus,1);BN_bn2binpad(minus,input,size);BN_free(minus);raw(rsa,"n-minus-one",input,size,3);
    BN_bn2binpad(n,input,size);raw(rsa,"n",input,size,3);memset(input,0xFF,sizeof(input));raw(rsa,"ff",input,size,3);
    memset(input,0,sizeof(input));int lengths[]={-1,0,size-1,size+1};for(unsigned i=0;i<sizeof(lengths)/sizeof(lengths[0]);i++){snprintf(label,sizeof(label),"length-%d",lengths[i]);raw(rsa,label,input,lengths[i],3);}
    raw(rsa,"padding-999",input,size,999);
    for(int mutation=-1;mutation<size;mutation++){
        memset(input,0x7F,size);input[0]=0;input[1]=2;int delimiter=size-49;input[delimiter]=0;for(int i=0;i<48;i++)input[delimiter+1+i]=(unsigned char)i;
        if(mutation>=0){if(mutation>1&&mutation<delimiter)input[mutation]=0;else input[mutation]++;}
        snprintf(label,sizeof(label),"encoded-mutation-%d",mutation);raw(rsa,label,input,size,3);
    }
    int lengths1[]={0,1,size-11,size-10};for(unsigned j=0;j<sizeof(lengths1)/sizeof(lengths1[0]);j++){
        int length=lengths1[j];for(int i=0;i<length;i++)input[i]=(unsigned char)i;
        unsigned char cipher[4098],plain[4096];memset(cipher,0xA5,sizeof(cipher));errors();int result=RSA_public_encrypt(length,input,cipher+1,rsa,RSA_PKCS1_PADDING),decoded=-1;
        if(result>=0)decoded=RSA_private_decrypt(result,cipher+1,plain,rsa,RSA_PKCS1_PADDING);
        printf("pkcs1:%d|%d|%d|%d|%d:",length,result,errors(),cipher[0]==0xA5&&cipher[size+1]==0xA5,decoded);if(decoded>=0)hex(plain,decoded);else hex(cipher+1,size);puts("");
    }
    RSA_free(rsa);RSA_free(NULL);
    for(int type=0;type<3;type++){
        EVP_PKEY *empty=EVP_PKEY_new();if(type)EVP_PKEY_set_type(empty,type==1?EVP_PKEY_RSA:EVP_PKEY_EC);errors();RSA *value=EVP_PKEY_get1_RSA(empty);printf("empty:%d|%d|%d\n",type,value==NULL,errors());RSA_free(value);EVP_PKEY_free(empty);
    }
    printf("freed|%d\n",errors());return 0;
}
