/* Test CLI adaptation only; all PSK and TLS operations remain in pinned libssl.
 * A sole configured TLS 1.3 suite specifies the external PSK session's digest.
 * Preserve the upstream SHA256 default when no unique suite was selected.
 */
#ifndef DOTCC_TEST_PEER_PSK_CIPHER_H
#define DOTCC_TEST_PEER_PSK_CIPHER_H
#include <string.h>
#include <openssl/ssl.h>

static const SSL_CIPHER *dotcc_test_psk_cipher(SSL *ssl,
        const unsigned char *default_id)
{
    STACK_OF(SSL_CIPHER) *configured = SSL_get_ciphers(ssl);
    const SSL_CIPHER *selected = NULL;
    int count = 0;
    int i;
    for (i = 0; i < sk_SSL_CIPHER_num(configured); i++) {
        const SSL_CIPHER *candidate = sk_SSL_CIPHER_value(configured, i);
        if (strcmp(SSL_CIPHER_get_version(candidate), "TLSv1.3") == 0) {
            selected = candidate;
            count++;
        }
    }
    return count == 1 ? selected : SSL_CIPHER_find(ssl, default_id);
}
#endif
