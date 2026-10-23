/* Same-pin native ownership control only. Never linked into the product. */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <openssl/crypto.h>
#include "api/s2n.h"
#include "utils/s2n_init.h"
#include "dotcc_s2n_compat.h"

#define CHECK(x) do { if (!(x)) { fprintf(stderr, "line %d: %s: %s\n", __LINE__, #x, s2n_strerror_name(s2n_errno)); return 1; } } while (0)
int main(int argc, char **argv)
{
    CHECK(argc == 2);
    CHECK(!s2n_is_initialized());
    CHECK(s2n_cleanup_final() < 0);
    CHECK(strcmp(s2n_strerror_name(s2n_errno), "S2N_ERR_NOT_INITIALIZED") == 0);
    CHECK(s2n_disable_atexit() == 0);
    if (strcmp(argv[1], "caller-crypto") == 0) {
        CHECK(OPENSSL_init_crypto(OPENSSL_INIT_LOAD_CRYPTO_STRINGS, NULL) == 1);
        CHECK(s2n_crypto_disable_init() == 0);
    } else CHECK(strcmp(argv[1], "automatic-crypto") == 0);
    for (int cycle = 0; cycle < 3; cycle++) {
        if (cycle != 0) {
            CHECK(s2n_crypto_disable_init() < 0);
            CHECK(strcmp(s2n_strerror_name(s2n_errno), "S2N_ERR_INITIALIZED") == 0);
        }
        CHECK(s2n_disable_atexit() == 0);
        CHECK(s2n_init() == 0);
        CHECK(s2n_is_initialized());
        CHECK(s2n_init() < 0);
        CHECK(strcmp(s2n_strerror_name(s2n_errno), "S2N_ERR_INITIALIZED") == 0);
        CHECK(s2n_disable_atexit() < 0);
        CHECK(strcmp(s2n_strerror_name(s2n_errno), "S2N_ERR_INITIALIZED") == 0);
        int disable_crypto_after_init = s2n_crypto_disable_init();
        CHECK(disable_crypto_after_init < 0);
        CHECK(strcmp(s2n_strerror_name(s2n_errno), "S2N_ERR_INITIALIZED") == 0);
        struct s2n_config *config = s2n_config_new(); CHECK(config);
        CHECK(dotcc_s2n_config_use_compat(config) == 0);
        struct s2n_connection *conn = s2n_connection_new(S2N_SERVER); CHECK(conn);
        CHECK(s2n_connection_set_config(conn, config) == 0);
        struct dotcc_s2n_policy *policy = dotcc_s2n_policy_new(S2N_SSLv3, S2N_SSLv3, 0x0004); CHECK(policy);
        CHECK(dotcc_s2n_connection_use_policy(conn, policy) == 0);
        CHECK(s2n_cleanup() == 0);
        CHECK(s2n_is_initialized()); /* Retained cleanup is a no-op. */
        CHECK(s2n_connection_free(conn) == 0);
        dotcc_s2n_policy_free(policy);
        CHECK(s2n_config_free(config) == 0);
        CHECK(s2n_cleanup_final() == 0);
        CHECK(!s2n_is_initialized());
        CHECK(s2n_cleanup_final() < 0);
        CHECK(strcmp(s2n_strerror_name(s2n_errno), "S2N_ERR_NOT_INITIALIZED") == 0);
        printf("{\"mode\":\"%s\",\"cycle\":%d,\"init\":true,\"cleanup_noop\":true,\"cleanup_final\":true,\"disable_crypto_after_init\":%d}\n", argv[1], cycle, disable_crypto_after_init);
    }
    return 0;
}
