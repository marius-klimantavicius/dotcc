/* Test-only native oracle for the managed declaration/feature profile.
 * Operations use real pinned OpenSSL; this identity bridge is never delivered
 * with the managed product and never claims native execution is managed TLS. */
#include "dotcc_provider.h"
uint32_t dotcc_crypto_provider_abi(void) { return DOTCC_CRYPTO_PROVIDER_ABI; }
uint64_t dotcc_crypto_provider_capabilities(void) { return DOTCC_CRYPTO_REQUIRED_CAPABILITIES; }
const char *dotcc_crypto_provider_name(void)
{
    return "DotCC managed s2n crypto / native OpenSSL differential-control ABI bridge";
}
