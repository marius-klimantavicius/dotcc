/* Independent OpenSSL TLS implementation, separately pinned from the backend. */
#include <openssl/ssl.h>
#include <openssl/err.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <signal.h>
#include <unistd.h>
#include "payload.h"
static void fail(const char *what, int line)
{
    fprintf(stderr, "line %d: %s\n", line, what); ERR_print_errors_fp(stderr); exit(1);
}
#define CHECK(x) do { if (!(x)) fail(#x, __LINE__); } while (0)
static int version(const char *name)
{
    if (!strcmp(name, "S2N_SSLv3")) return SSL3_VERSION;
    if (!strcmp(name, "S2N_TLS10")) return TLS1_VERSION;
    if (!strcmp(name, "S2N_TLS11")) return TLS1_1_VERSION;
    if (!strcmp(name, "S2N_TLS12")) return TLS1_2_VERSION;
#ifdef TLS1_3_VERSION
    if (!strcmp(name, "S2N_TLS13")) return TLS1_3_VERSION;
#endif
    return -1;
}
static void transfer(SSL *ssl, int sending, unsigned direction)
{
    uint8_t data[PAYLOAD_LENGTH], expected[PAYLOAD_LENGTH];
    payload(expected, direction);
    if (sending) memcpy(data, expected, sizeof data); else memset(data, 0, sizeof data);
    size_t count = 0;
    while (count < sizeof data) {
        size_t length = sizeof data - count;
        if (sending && length > 389) length = 389;
        int result = sending ? SSL_write(ssl, data + count, length) : SSL_read(ssl, data + count, length);
        CHECK(result > 0); count += result;
    }
    CHECK(memcmp(data, expected, sizeof data) == 0);
}
int main(int argc, char **argv)
{
    CHECK(argc == 9); signal(SIGPIPE, SIG_IGN);
    int client = !strcmp(argv[1], "client"), fd = atoi(argv[2]), requested = version(argv[3]);
    unsigned id = (unsigned) strtoul(argv[4], NULL, 16);
#if OPENSSL_VERSION_NUMBER < 0x10100000L
    SSL_library_init(); SSL_load_error_strings();
    CHECK(requested == SSL3_VERSION);
    SSL_CTX *ctx = SSL_CTX_new(SSLv3_method()); CHECK(ctx);
#else
    SSL_CTX *ctx = SSL_CTX_new(TLS_method()); CHECK(ctx);
    SSL_CTX_set_security_level(ctx, 0);
    CHECK(SSL_CTX_set_min_proto_version(ctx, requested) == 1);
    CHECK(SSL_CTX_set_max_proto_version(ctx, requested) == 1);
#endif
    SSL_CTX_set_options(ctx, SSL_OP_NO_TICKET);
#if OPENSSL_VERSION_NUMBER >= 0x10101000L
    if (requested == TLS1_3_VERSION) CHECK(SSL_CTX_set_ciphersuites(ctx, argv[5]) == 1);
    else CHECK(SSL_CTX_set_cipher_list(ctx, argv[5]) == 1);
#else
    CHECK(SSL_CTX_set_cipher_list(ctx, argv[5]) == 1);
#endif
    char cert_path[4096], key_path[4096];
    if (client) {
        for (int ec = 0; ec <= 1; ++ec) {
            snprintf(cert_path, sizeof cert_path, "%s/%s.pem", argv[6], ec ? "ecdsa" : "rsa");
            CHECK(SSL_CTX_load_verify_locations(ctx, cert_path, NULL) == 1);
        }
        SSL_CTX_set_verify(ctx, !strcmp(argv[8], "bypass") ? SSL_VERIFY_NONE : SSL_VERIFY_PEER, NULL);
    } else {
        int ec = strstr(argv[5], "ECDSA") != NULL;
        snprintf(cert_path, sizeof cert_path, "%s/%s.pem", argv[6], ec ? "ecdsa" : "rsa");
        snprintf(key_path, sizeof key_path, "%s/%s.key", argv[6], ec ? "ecdsa" : "rsa");
        CHECK(SSL_CTX_use_certificate_chain_file(ctx, cert_path) == 1);
        CHECK(SSL_CTX_use_PrivateKey_file(ctx, key_path, SSL_FILETYPE_PEM) == 1);
        CHECK(SSL_CTX_check_private_key(ctx) == 1);
#if OPENSSL_VERSION_NUMBER >= 0x10100000L
        CHECK(SSL_CTX_set_dh_auto(ctx, 1) == 1);
#else
        snprintf(cert_path, sizeof cert_path, "%s/tests/pems/dhparams_2048.pem", argv[7]);
        FILE *dhfile = fopen(cert_path, "r"); CHECK(dhfile);
        DH *dh = PEM_read_DHparams(dhfile, NULL, NULL, NULL); CHECK(dh); CHECK(fclose(dhfile) == 0);
        CHECK(SSL_CTX_set_tmp_dh(ctx, dh) == 1); DH_free(dh);
        CHECK(SSL_CTX_set_ecdh_auto(ctx, 1) == 1);
#endif
    }
    SSL *ssl = SSL_new(ctx); CHECK(ssl); CHECK(SSL_set_fd(ssl, fd) == 1);
    if (client) { CHECK(SSL_set_tlsext_host_name(ssl, "localhost") == 1); CHECK(X509_VERIFY_PARAM_set1_host(SSL_get0_param(ssl), "localhost", 0) == 1); }
    CHECK((client ? SSL_connect(ssl) : SSL_accept(ssl)) == 1);
    CHECK(SSL_version(ssl) == requested);
    CHECK((SSL_CIPHER_get_id(SSL_get_current_cipher(ssl)) & 0xffffu) == id);
    if (client && strcmp(argv[8], "bypass")) CHECK(SSL_get_verify_result(ssl) == X509_V_OK);
    if (client) { transfer(ssl, 1, 0); transfer(ssl, 0, 1); }
    else { transfer(ssl, 0, 0); transfer(ssl, 1, 1); }
    int shutdown_result = SSL_shutdown(ssl);
    if (shutdown_result == 0) shutdown_result = SSL_shutdown(ssl);
    CHECK(shutdown_result == 1);
    SSL_free(ssl); SSL_CTX_free(ctx); CHECK(close(fd) == 0);
    printf("{\"engine\":\"openssl-peer\",\"role\":\"%s\",\"version\":\"%s\",\"iana_id\":\"0x%04X\",\"sent\":%d,\"received\":%d,\"shutdown\":true}\n", argv[1], argv[3], id, PAYLOAD_LENGTH, PAYLOAD_LENGTH);
    return 0;
}
