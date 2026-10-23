#include "utils/s2n_prelude.h"
#include "dotcc_s2n_compat.h"
#include "api/unstable/cleanup.h"
#include "tls/s2n_security_policies.h"
#include "tls/s2n_connection.h"
#include "tls/s2n_config.h"
#include <stdio.h>
#include <stdlib.h>

#define CHECK(x) do { if (!(x)) { fprintf(stderr, "policy assertion %d: %s\n", __LINE__, #x); return 1; } } while (0)
int main(void)
{
    CHECK(s2n_init() == 0);
    const struct s2n_security_policy *policy = dotcc_s2n_compatibility_policy();
    CHECK(policy->minimum_protocol_version == S2N_SSLv3);
    CHECK(policy->cipher_preferences->count == 36);
    CHECK(policy->kem_preferences->kem_count == 0 && policy->kem_preferences->tls13_kem_group_count == 0);
    CHECK(policy->signature_preferences->count == 17);
    unsigned sslv3 = 0;
    for (unsigned i = 0; i < policy->cipher_preferences->count; ++i) {
        struct s2n_cipher_suite *cipher = policy->cipher_preferences->suites[i];
        CHECK(cipher == cipher_preferences_test_all.suites[i]);
        CHECK(cipher->available);
        if (cipher->sslv3_record_alg != NULL) {
            CHECK(cipher->sslv3_record_alg->cipher->is_available());
            sslv3++;
        }
    }
    CHECK(sslv3 == 14);
    struct s2n_config *config = s2n_config_new(); CHECK(config);
    CHECK(dotcc_s2n_config_use_compat(config) == 0);
    CHECK(config->security_policy == policy);
    for (unsigned role = S2N_SERVER; role <= S2N_CLIENT; ++role) {
        struct s2n_connection *conn = s2n_connection_new(role); CHECK(conn);
        CHECK(s2n_connection_set_config(conn, config) == 0);
        struct dotcc_s2n_policy *selection = dotcc_s2n_policy_new(S2N_SSLv3, S2N_SSLv3, 0x0004); CHECK(selection);
        CHECK(dotcc_s2n_connection_use_policy(conn, selection) == 0);
        const struct s2n_security_policy *actual = NULL;
        CHECK(s2n_connection_get_security_policy(conn, &actual) == 0);
        CHECK(actual->minimum_protocol_version == S2N_SSLv3 && actual->cipher_preferences->count == 1);
        CHECK(actual->cipher_preferences->suites[0]->iana_value[1] == 4);
        CHECK((role == S2N_CLIENT ? conn->client_protocol_version : conn->server_protocol_version) == S2N_SSLv3);
        CHECK(s2n_connection_free(conn) == 0);
        dotcc_s2n_policy_free(selection);
    }
    CHECK(dotcc_s2n_policy_new(S2N_TLS13, S2N_TLS12, 0) == NULL);
    CHECK(dotcc_s2n_policy_new(S2N_SSLv3, S2N_TLS13, 0xffff) == NULL);
    CHECK(s2n_config_free(config) == 0);
    CHECK(s2n_cleanup_final() == 0);
    printf("{\"suites\":36,\"sslv3\":14,\"classical_signatures\":17,\"roles\":2,\"policy\":\"compat\"}\n");
    return 0;
}
