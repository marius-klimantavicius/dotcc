/* Deterministic native regression: TLS DHE encodes the shared secret unpadded. */
#include <openssl/dh.h>
#include <openssl/bn.h>
#include <openssl/pem.h>
#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include "s2n.h"
#include "api/unstable/cleanup.h"
#include "crypto/s2n_dhe.h"
#include "stuffer/s2n_stuffer.h"
#include "utils/s2n_mem.h"
#define CHECK(x) do { if (!(x)) { fprintf(stderr, "%d: %s: %s\n", __LINE__, #x, s2n_strerror(s2n_errno, "EN")); return 2; } } while (0)
/* Test-only linker interception fixes the client's otherwise random exponent. */
int __wrap_DH_generate_key(DH *dh)
{
    const BIGNUM *g = NULL; DH_get0_pqg(dh, NULL, NULL, &g);
    BIGNUM *priv = BN_new(), *pub = BN_new(); BN_CTX *ctx = BN_CTX_new();
    if (!g || !priv || !pub || !ctx) return 0;
    if (!BN_set_word(priv, 2) || !BN_sqr(pub, g, ctx)) return 0;
    BN_CTX_free(ctx);
    return DH_set0_key(dh, pub, priv);
}
int main(int argc, char **argv)
{
    CHECK(argc == 2); CHECK(s2n_init() == 0);
    FILE *file = fopen(argv[1], "r"); CHECK(file);
    DH *dh = PEM_read_DHparams(file, NULL, NULL, NULL); CHECK(dh); CHECK(fclose(file) == 0);
    /* Server private=2, server public=g^2; client public=g^2. Secret=g^4. */
    const BIGNUM *g = NULL; DH_get0_pqg(dh, NULL, NULL, &g); CHECK(g);
    BIGNUM *priv = BN_new(), *pub = BN_dup(g); CHECK(priv && pub);
    CHECK(BN_set_word(priv, 2) == 1);
    BN_CTX *ctx = BN_CTX_new(); CHECK(ctx);
    CHECK(BN_sqr(pub, g, ctx) == 1); BN_CTX_free(ctx);
    CHECK(DH_set0_key(dh, pub, priv) == 1);
    uint8_t raw[1024] = { 0 };
    int raw_length = DH_compute_key(raw, pub, dh); CHECK(raw_length > 0);
    uint8_t wire[1024] = { 0 };
    int public_length = BN_num_bytes(pub); CHECK(public_length < 1000);
    wire[0] = public_length >> 8; wire[1] = public_length & 255;
    CHECK(BN_bn2bin(pub, wire + 2) == public_length);
    struct s2n_blob input = { .data = wire, .size = public_length + 2 };
    struct s2n_stuffer stuffer = { 0 }; CHECK(s2n_stuffer_init(&stuffer, &input) == 0);
    CHECK(s2n_stuffer_skip_write(&stuffer, input.size) == 0);
    struct s2n_dh_params params = { .dh = dh };
    struct s2n_blob secret = { 0 };
    CHECK(s2n_dh_compute_shared_secret_as_server(&params, &stuffer, &secret) == 0);
    uint8_t client_wire[1024] = { 0 };
    struct s2n_blob client_buffer = { .data = client_wire, .size = sizeof client_wire };
    struct s2n_stuffer client_out = { 0 }; CHECK(s2n_stuffer_init(&client_out, &client_buffer) == 0);
    struct s2n_blob client_secret = { 0 };
    CHECK(s2n_dh_compute_shared_secret_as_client(&params, &client_out, &client_secret) == 0);
    int matches = secret.size == (uint32_t) raw_length && !memcmp(secret.data, raw, raw_length)
            && client_secret.size == (uint32_t) raw_length && !memcmp(client_secret.data, raw, raw_length);
    printf("{\"case\":\"tls-dhe-leading-zero\",\"backend_unpadded_size\":%d,\"s2n_server_shared_secret_size\":%u,\"s2n_client_shared_secret_size\":%u,\"group_size\":%d,\"matches_tls_unpadded\":%s}\n", raw_length, secret.size, client_secret.size, DH_size(dh), matches ? "true" : "false");
    CHECK(s2n_free(&client_secret) == 0);
    CHECK(s2n_free(&secret) == 0); CHECK(s2n_dh_params_free(&params) == 0);
    CHECK(s2n_cleanup_final() == 0);
    return matches ? 0 : 1;
}
