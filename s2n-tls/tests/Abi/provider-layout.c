/* Compile against the selected pinned headers; compare with ACTUAL emitted types. */
#include <stddef.h>
#include <stdio.h>
#include <openssl/asn1.h>
#include <openssl/x509v3.h>
#include <openssl/md5.h>
#include <openssl/sha.h>

int main(void)
{
    printf("{\"pointer\":%zu,\"long\":%zu,\"size_t\":%zu,", sizeof(void *), sizeof(long), sizeof(size_t));
    printf("\"ASN1_STRING\":{\"size\":%zu,\"align\":%zu,\"length\":%zu,\"type\":%zu,\"data\":%zu,\"flags\":%zu},",
        sizeof(ASN1_STRING), _Alignof(ASN1_STRING), offsetof(ASN1_STRING, length), offsetof(ASN1_STRING, type), offsetof(ASN1_STRING, data), offsetof(ASN1_STRING, flags));
    printf("\"GENERAL_NAME\":{\"size\":%zu,\"align\":%zu,\"type\":%zu,\"d\":%zu},",
        sizeof(GENERAL_NAME), _Alignof(GENERAL_NAME), offsetof(GENERAL_NAME, type), offsetof(GENERAL_NAME, d));
    printf("\"MD5_CTX\":%zu,\"SHA_CTX\":%zu,\"SHA256_CTX\":%zu,\"SHA512_CTX\":%zu}\n", sizeof(MD5_CTX), sizeof(SHA_CTX), sizeof(SHA256_CTX), sizeof(SHA512_CTX));
    return 0;
}
