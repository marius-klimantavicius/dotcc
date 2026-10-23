#ifndef DOTCC_S2N_CRYPTO_PROVIDER_H
#define DOTCC_S2N_CRYPTO_PROVIDER_H
#include <stdint.h>

/* The OpenSSL headers describe the source ABI only. This provider is managed. */
#define DOTCC_CRYPTO_PROVIDER_ABI 1u
#define DOTCC_CRYPTO_CAP_DIGEST  (1ull << 0)
#define DOTCC_CRYPTO_CAP_RC4     (1ull << 1)
#define DOTCC_CRYPTO_CAP_CBC     (1ull << 2)
#define DOTCC_CRYPTO_CAP_AEAD    (1ull << 3)
#define DOTCC_CRYPTO_CAP_RSA     (1ull << 4)
#define DOTCC_CRYPTO_CAP_DH      (1ull << 5)
#define DOTCC_CRYPTO_CAP_EC      (1ull << 6)
#define DOTCC_CRYPTO_CAP_X509    (1ull << 7)
#define DOTCC_CRYPTO_REQUIRED_CAPABILITIES 0xffull

/* Values are checked at runtime by the narrowly adapted s2n_libcrypto.c. */
uint32_t dotcc_crypto_provider_abi(void);
uint64_t dotcc_crypto_provider_capabilities(void);
const char *dotcc_crypto_provider_name(void);
#endif
