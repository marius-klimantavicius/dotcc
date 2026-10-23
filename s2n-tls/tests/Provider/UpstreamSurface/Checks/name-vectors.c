#include <openssl/crypto.h>
#include <openssl/pem.h>
#include <openssl/x509.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static void print_name(X509_NAME *name)
{
    int count = X509_NAME_entry_count(name);
    printf("entries %d\n", count);
    for (int i = 0; i < count; ++i) {
        ASN1_STRING *data = X509_NAME_ENTRY_get_data(X509_NAME_get_entry(name, i));
        printf("entry %d %d ", OBJ_obj2nid(X509_NAME_ENTRY_get_object(X509_NAME_get_entry(name, i))), ASN1_STRING_type(data));
        for (int j = 0; j < ASN1_STRING_length(data); ++j) printf("%02x", ASN1_STRING_get0_data(data)[j]);
        putchar('\n');
    }
    unsigned char *der = NULL;
    int length = i2d_X509_NAME(name, &der);
    printf("der %d ", length);
    for (int i = 0; i < length; ++i) printf("%02x", der[i]);
    putchar('\n');
    OPENSSL_free(der);
}

int main(int argc, char **argv)
{
    if (argc != 3 || strncmp(OpenSSL_version(0), argv[2], strlen(argv[2]))) return 2;
    FILE *input = fopen(argv[1], "r");
    if (!input) return 3;
    X509 *certificate = PEM_read_X509(input, NULL, NULL, NULL);
    fclose(input);
    X509_NAME *name = X509_NAME_new();
    if (!certificate || !name) return 4;
    unsigned char *original = NULL;
    int original_length = i2d_X509(certificate, &original);
    const int types[] = { V_ASN1_IA5STRING, 29, V_ASN1_IA5STRING };
    const unsigned char *values[] = { (const unsigned char *)"", (const unsigned char *)"invalid", (const unsigned char *)"valid" };
    for (int i = 0; i < 3; ++i) {
        if (X509_NAME_add_entry_by_NID(name, NID_commonName, types[i], values[i], -1, -1, 1) != 1) return 5;
        if (X509_set_subject_name(certificate, name) != 1) return 6;
        print_name(X509_get_subject_name(certificate));
        unsigned char *after = NULL;
        int after_length = i2d_X509(certificate, &after);
        printf("cached-certificate-der %d\n", after_length == original_length && !memcmp(original, after, original_length));
        OPENSSL_free(after);
    }
    X509_NAME_free(name);
    print_name(X509_get_subject_name(certificate));
    OPENSSL_free(original);
    X509_free(certificate);
    return 0;
}
