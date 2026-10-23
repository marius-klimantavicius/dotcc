/* Same-pin native s2n endpoint; internal policy access is confined to this test. */
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <signal.h>
#include <sys/socket.h>
#include <unistd.h>
#include "s2n.h"
#include "api/unstable/cleanup.h"
#include "dotcc_s2n_compat.h"
#include "tls/s2n_connection.h"
#include "tls/s2n_config.h"
#include "tls/s2n_cipher_suites.h"
#include "tls/s2n_cipher_preferences.h"
#include "tls/s2n_security_policies.h"
#include "payload.h"

static void fail(const char *what, int line)
{
    fprintf(stderr, "line %d: %s: %s: %s (%s)\n", line, what, s2n_strerror_name(s2n_errno),
            s2n_strerror(s2n_errno, "EN"), s2n_strerror_debug(s2n_errno, "EN"));
    exit(1);
}
#define CHECK(x) do { if (!(x)) fail(#x, __LINE__); } while (0)
static char *read_pem(const char *path)
{
    FILE *file = fopen(path, "rb"); CHECK(file);
    CHECK(fseek(file, 0, SEEK_END) == 0);
    long size = ftell(file); CHECK(size > 0);
    CHECK(fseek(file, 0, SEEK_SET) == 0);
    char *text = calloc((size_t) size + 1, 1); CHECK(text);
    CHECK(fread(text, 1, size, file) == (size_t) size); CHECK(fclose(file) == 0);
    return text;
}
static int version(const char *name)
{
    if (!strcmp(name, "S2N_SSLv3")) return S2N_SSLv3;
    if (!strcmp(name, "S2N_TLS10")) return S2N_TLS10;
    if (!strcmp(name, "S2N_TLS11")) return S2N_TLS11;
    if (!strcmp(name, "S2N_TLS12")) return S2N_TLS12;
    if (!strcmp(name, "S2N_TLS13")) return S2N_TLS13;
    return -1;
}
static void transfer(struct s2n_connection *conn, int sending, unsigned direction)
{
    uint8_t data[PAYLOAD_LENGTH], expected[PAYLOAD_LENGTH];
    payload(expected, direction);
    if (sending) memcpy(data, expected, sizeof data);
    else memset(data, 0, sizeof data);
    size_t count = 0;
    /* Three application writes prove stream/cipher state continues across records. */
    while (count < sizeof data) {
        s2n_blocked_status blocked = S2N_NOT_BLOCKED;
        size_t length = sizeof data - count;
        if (sending && length > 389) length = 389;
        ssize_t result = sending ? s2n_send(conn, data + count, length, &blocked)
                                 : s2n_recv(conn, data + count, length, &blocked);
        CHECK(result > 0); count += result;
    }
    CHECK(memcmp(data, expected, sizeof data) == 0);
}
int main(int argc, char **argv)
{
    CHECK(argc == 9);
    signal(SIGPIPE, SIG_IGN);
    int client = !strcmp(argv[1], "client"), fd = atoi(argv[2]), requested = version(argv[3]);
    CHECK(requested >= S2N_SSLv3);
    unsigned id = (unsigned) strtoul(argv[4], NULL, 16);
    CHECK(s2n_init() == 0);
    struct s2n_cipher_suite *selected = NULL;
    const struct s2n_security_policy *compat = dotcc_s2n_compatibility_policy();
    for (size_t i = 0; i < compat->cipher_preferences->count; ++i) {
        struct s2n_cipher_suite *candidate = compat->cipher_preferences->suites[i];
        if ((candidate->iana_value[0] * 256u + candidate->iana_value[1]) == id) selected = candidate;
    }
    CHECK(selected && selected->available);
    if (requested == S2N_SSLv3) CHECK(selected->sslv3_record_alg && selected->sslv3_record_alg->cipher->is_available());
    struct dotcc_s2n_policy *policy = dotcc_s2n_policy_new(requested, requested, id); CHECK(policy);
    struct s2n_config *config = s2n_config_new(); CHECK(config);
    struct s2n_cert_chain_and_key *chains[2] = { NULL, NULL };
    CHECK(dotcc_s2n_config_use_compat(config) == 0);
    CHECK(s2n_config_set_session_tickets_onoff(config, 0) == 0);
    char rsa_cert[4096], ec_cert[4096], key_path[4096], cert_path[4096];
    snprintf(rsa_cert, sizeof rsa_cert, "%s/rsa.pem", argv[6]);
    snprintf(ec_cert, sizeof ec_cert, "%s/ecdsa.pem", argv[6]);
    if (client) {
        if (!strstr(argv[8], "untrusted")) {
            CHECK(s2n_config_set_verification_ca_location(config, rsa_cert, NULL) == 0);
            CHECK(s2n_config_set_verification_ca_location(config, ec_cert, NULL) == 0);
        }
        if (strstr(argv[8], "bypass")) CHECK(s2n_config_disable_x509_verification(config) == 0);
    } else {
        for (int ec = 0; ec <= 1; ec++) {
            snprintf(cert_path, sizeof cert_path, "%s/%s.pem", argv[6], ec ? "ecdsa" : "rsa");
            snprintf(key_path, sizeof key_path, "%s/%s.key", argv[6], ec ? "ecdsa" : "rsa");
            char *cert = read_pem(cert_path), *key = read_pem(key_path);
            chains[ec] = s2n_cert_chain_and_key_new(); CHECK(chains[ec]);
            CHECK(s2n_cert_chain_and_key_load_pem(chains[ec], cert, key) == 0);
            CHECK(s2n_config_add_cert_chain_and_key_to_store(config, chains[ec]) == 0);
            free(cert); free(key);
        }
        snprintf(cert_path, sizeof cert_path, "%s/tests/pems/dhparams_2048.pem", argv[7]);
        char *dh = read_pem(cert_path); CHECK(s2n_config_add_dhparams(config, dh) == 0); free(dh);
    }
    struct s2n_connection *conn = s2n_connection_new(client ? S2N_CLIENT : S2N_SERVER); CHECK(conn);
    CHECK(s2n_connection_set_config(conn, config) == 0);
    CHECK(dotcc_s2n_connection_use_policy(conn, policy) == 0);
    CHECK(s2n_connection_set_blinding(conn, S2N_SELF_SERVICE_BLINDING) == 0);
    if (client) {
        CHECK(s2n_set_server_name(conn, "localhost") == 0);
    }
    CHECK(s2n_connection_set_fd(conn, fd) == 0);
    s2n_blocked_status blocked = S2N_NOT_BLOCKED;
    CHECK(s2n_negotiate(conn, &blocked) == 0);
    CHECK(s2n_connection_get_actual_protocol_version(conn) == requested);
    uint8_t actual_id[2] = { 0 };
    CHECK(s2n_connection_get_cipher_iana_value(conn, &actual_id[0], &actual_id[1]) == 0);
    CHECK(actual_id[0] * 256u + actual_id[1] == id);
    CHECK(strcmp(s2n_connection_get_cipher(conn), argv[5]) == 0);
    if (client) { transfer(conn, 1, 0); transfer(conn, 0, 1); }
    else { transfer(conn, 0, 0); transfer(conn, 1, 1); }
    CHECK(s2n_shutdown(conn, &blocked) == 0);
    CHECK(s2n_connection_free(conn) == 0);
    dotcc_s2n_policy_free(policy);
    CHECK(s2n_config_free(config) == 0);
    for (int ec = 0; ec <= 1; ++ec) if (chains[ec]) CHECK(s2n_cert_chain_and_key_free(chains[ec]) == 0);
    CHECK(s2n_cleanup() == 0);
    CHECK(s2n_cleanup_final() == 0);
    CHECK(close(fd) == 0);
    printf("{\"engine\":\"s2n\",\"role\":\"%s\",\"version\":\"%s\",\"iana_id\":\"0x%04X\",\"sent\":%d,\"received\":%d,\"shutdown\":true}\n", argv[1], argv[3], id, PAYLOAD_LENGTH, PAYLOAD_LENGTH);
    return 0;
}
