/* Authored compatibility policy adapter. TLS behavior remains in s2n. */
#include "utils/s2n_prelude.h"
#include "dotcc_s2n_compat.h"
#include "tls/s2n_connection.h"
#include "tls/s2n_config.h"
#include "tls/s2n_cipher_suites.h"
#include "tls/s2n_cipher_preferences.h"
#include "tls/s2n_security_policies.h"
#include "utils/s2n_safety.h"
#include <stdlib.h>

/* Same selected negotiable table, with an authored policy identity. */
static struct s2n_cipher_suite *dotcc_compat_suites[] = {
    &s2n_rsa_with_rc4_128_md5,
    &s2n_rsa_with_rc4_128_sha,
    &s2n_rsa_with_3des_ede_cbc_sha,
    &s2n_dhe_rsa_with_3des_ede_cbc_sha,
    &s2n_rsa_with_aes_128_cbc_sha,
    &s2n_dhe_rsa_with_aes_128_cbc_sha,
    &s2n_rsa_with_aes_256_cbc_sha,
    &s2n_dhe_rsa_with_aes_256_cbc_sha,
    &s2n_rsa_with_aes_128_cbc_sha256,
    &s2n_rsa_with_aes_256_cbc_sha256,
    &s2n_dhe_rsa_with_aes_128_cbc_sha256,
    &s2n_dhe_rsa_with_aes_256_cbc_sha256,
    &s2n_rsa_with_aes_128_gcm_sha256,
    &s2n_rsa_with_aes_256_gcm_sha384,
    &s2n_dhe_rsa_with_aes_128_gcm_sha256,
    &s2n_dhe_rsa_with_aes_256_gcm_sha384,
    &s2n_tls13_aes_128_gcm_sha256,
    &s2n_tls13_aes_256_gcm_sha384,
    &s2n_tls13_chacha20_poly1305_sha256,
    &s2n_ecdhe_ecdsa_with_aes_128_cbc_sha,
    &s2n_ecdhe_ecdsa_with_aes_256_cbc_sha,
    &s2n_ecdhe_rsa_with_rc4_128_sha,
    &s2n_ecdhe_rsa_with_3des_ede_cbc_sha,
    &s2n_ecdhe_rsa_with_aes_128_cbc_sha,
    &s2n_ecdhe_rsa_with_aes_256_cbc_sha,
    &s2n_ecdhe_ecdsa_with_aes_128_cbc_sha256,
    &s2n_ecdhe_ecdsa_with_aes_256_cbc_sha384,
    &s2n_ecdhe_rsa_with_aes_128_cbc_sha256,
    &s2n_ecdhe_rsa_with_aes_256_cbc_sha384,
    &s2n_ecdhe_ecdsa_with_aes_128_gcm_sha256,
    &s2n_ecdhe_ecdsa_with_aes_256_gcm_sha384,
    &s2n_ecdhe_rsa_with_aes_128_gcm_sha256,
    &s2n_ecdhe_rsa_with_aes_256_gcm_sha384,
    &s2n_ecdhe_rsa_with_chacha20_poly1305_sha256,
    &s2n_ecdhe_ecdsa_with_chacha20_poly1305_sha256,
    &s2n_dhe_rsa_with_chacha20_poly1305_sha256,
};

/* Preserve every classical signature from the upstream all list. ML-DSA is
 * outside this product; no KEM or PQ policy is inherited from test_all. */
static const struct s2n_signature_scheme *const dotcc_compat_signature_schemes[] = {
    &s2n_rsa_pkcs1_md5_sha1,
    &s2n_rsa_pkcs1_sha1,
    &s2n_rsa_pkcs1_sha224,
    &s2n_rsa_pkcs1_sha256,
    &s2n_rsa_pkcs1_sha384,
    &s2n_rsa_pkcs1_sha512,
    &s2n_ecdsa_sha1,
    &s2n_ecdsa_sha224,
    &s2n_ecdsa_sha256,
    &s2n_ecdsa_sha384,
    &s2n_ecdsa_sha512,
    &s2n_rsa_pss_rsae_sha256,
    &s2n_rsa_pss_rsae_sha384,
    &s2n_rsa_pss_rsae_sha512,
    &s2n_rsa_pss_pss_sha256,
    &s2n_rsa_pss_pss_sha384,
    &s2n_rsa_pss_pss_sha512,
};
static const struct s2n_signature_preferences dotcc_compat_signatures = {
    .count = sizeof(dotcc_compat_signature_schemes) / sizeof(dotcc_compat_signature_schemes[0]),
    .signature_schemes = dotcc_compat_signature_schemes,
};
static const struct s2n_cipher_preferences dotcc_compat_ciphers = {
    .count = sizeof(dotcc_compat_suites) / sizeof(dotcc_compat_suites[0]),
    .suites = dotcc_compat_suites,
};
static const struct s2n_security_policy dotcc_compat_policy = {
    .minimum_protocol_version = S2N_SSLv3,
    .cipher_preferences = &dotcc_compat_ciphers,
    .kem_preferences = &kem_preferences_null,
    .signature_preferences = &dotcc_compat_signatures,
    .ecc_preferences = &s2n_ecc_preferences_20240501,
};
struct dotcc_s2n_policy {
    struct s2n_security_policy policy;
    struct s2n_cipher_preferences ciphers;
    struct s2n_cipher_suite *selected;
    uint8_t maximum_protocol_version;
};

/* Upstream internal setter validates loaded certificates and provider policy. */
int s2n_config_set_security_policy(struct s2n_config *, const struct s2n_security_policy *);

const struct s2n_security_policy *dotcc_s2n_compatibility_policy(void)
{
    return &dotcc_compat_policy;
}
int dotcc_s2n_config_use_compat(struct s2n_config *config)
{
    return s2n_config_set_security_policy(config, &dotcc_compat_policy);
}
struct dotcc_s2n_policy *dotcc_s2n_policy_new(uint8_t minimum, uint8_t maximum, uint16_t cipher)
{
    PTR_ENSURE(minimum >= S2N_SSLv3 && maximum <= S2N_TLS13 && minimum <= maximum, S2N_ERR_INVALID_ARGUMENT);
    struct dotcc_s2n_policy *owner = calloc(1, sizeof(struct dotcc_s2n_policy));
    PTR_ENSURE_REF(owner);
    owner->policy = dotcc_compat_policy;
    owner->policy.minimum_protocol_version = minimum;
    owner->maximum_protocol_version = maximum;
    if (cipher != 0) {
        for (size_t i = 0; i < dotcc_compat_ciphers.count; ++i) {
            struct s2n_cipher_suite *candidate = dotcc_compat_suites[i];
            if (candidate->iana_value[0] * 256u + candidate->iana_value[1] == cipher) {
                owner->selected = candidate;
            }
        }
        if (owner->selected == NULL) {
            free(owner);
            PTR_BAIL(S2N_ERR_CIPHER_NOT_SUPPORTED);
        }
        owner->ciphers.count = 1;
        owner->ciphers.suites = &owner->selected;
        owner->policy.cipher_preferences = &owner->ciphers;
    }
    return owner;
}
int dotcc_s2n_connection_use_policy(struct s2n_connection *conn, struct dotcc_s2n_policy *owner)
{
    POSIX_ENSURE_REF(conn);
    POSIX_ENSURE_REF(owner);
    POSIX_GUARD(s2n_connection_set_security_policy(conn, &owner->policy));
    if (conn->mode == S2N_CLIENT) conn->client_protocol_version = owner->maximum_protocol_version;
    else conn->server_protocol_version = owner->maximum_protocol_version;
    return S2N_SUCCESS;
}
void dotcc_s2n_policy_free(struct dotcc_s2n_policy *owner)
{
    free(owner);
}
