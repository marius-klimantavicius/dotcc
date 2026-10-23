#include "utils/s2n_prelude.h"
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include "crypto/s2n_hash.h"
#include "stuffer/s2n_stuffer.h"
#include "tls/s2n_cipher_suites.h"
#include "tls/s2n_config.h"
#include "tls/s2n_connection.h"
#include "tls/s2n_security_policies.h"

#define LAYOUT(type) printf(#type ":%zu:%zu\n", sizeof(struct type), _Alignof(struct type))
#define FIELD(type, field) printf(#type "." #field ":%zu\n", offsetof(struct type, field))
static void bytes(const char *name, const void *object, size_t size)
{
    const unsigned char *data = object;
    printf("%s:", name);
    for (size_t i = 0; i < size; ++i) printf("%02x", data[i]);
    printf("\n");
}
int layout_probe(void)
{
    LAYOUT(s2n_blob); FIELD(s2n_blob, data); FIELD(s2n_blob, size); FIELD(s2n_blob, allocated);
    LAYOUT(s2n_stuffer); FIELD(s2n_stuffer, blob); FIELD(s2n_stuffer, read_cursor); FIELD(s2n_stuffer, write_cursor); FIELD(s2n_stuffer, high_water_mark);
    LAYOUT(s2n_hash_state); FIELD(s2n_hash_state, hash_impl); FIELD(s2n_hash_state, alg); FIELD(s2n_hash_state, digest);
    LAYOUT(s2n_cipher_suite); FIELD(s2n_cipher_suite, name); FIELD(s2n_cipher_suite, iana_value); FIELD(s2n_cipher_suite, key_exchange_alg);
    FIELD(s2n_cipher_suite, record_alg); FIELD(s2n_cipher_suite, sslv3_record_alg); FIELD(s2n_cipher_suite, minimum_required_tls_version);
    LAYOUT(s2n_security_policy); FIELD(s2n_security_policy, cipher_preferences); FIELD(s2n_security_policy, kem_preferences); FIELD(s2n_security_policy, rules);
    LAYOUT(s2n_config); FIELD(s2n_config, security_policy);
    LAYOUT(s2n_connection); FIELD(s2n_connection, config); FIELD(s2n_connection, security_policy_override); FIELD(s2n_connection, context);
    FIELD(s2n_connection, send); FIELD(s2n_connection, recv); FIELD(s2n_connection, client_protocol_version); FIELD(s2n_connection, actual_protocol_version);
    struct s2n_blob blob; memset(&blob, 0, sizeof(blob)); blob.growable = 1; bytes("blob-bitfields", &blob, sizeof(blob));
    struct s2n_stuffer stuffer; memset(&stuffer, 0, sizeof(stuffer)); stuffer.alloced = 1; stuffer.tainted = 1; bytes("stuffer-bitfields", &stuffer, sizeof(stuffer));
    struct s2n_config config; memset(&config, 0, sizeof(config)); config.use_tickets = 1; config.disable_x509_validation = 1; bytes("config-bitfields", &config, 8);
    return 0;
}
#ifdef NATIVE_LAYOUT_CONTROL
int main(void) { return layout_probe(); }
#endif
