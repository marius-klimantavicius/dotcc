/* Authored native-only qualification fixture. Never linked into the product. */
#ifndef DOTCC_S2N_ORACLE_PAYLOAD_H
#define DOTCC_S2N_ORACLE_PAYLOAD_H
#include <stdint.h>
#include <string.h>
#define PAYLOAD_LENGTH 1027
static void payload(uint8_t *out, unsigned direction)
{
    for (unsigned i = 0; i < PAYLOAD_LENGTH; ++i) {
        out[i] = (uint8_t) ((i * 131u + (i >> 3) + direction * 73u) & 255u);
    }
    out[0] = 0; out[513] = 255; out[PAYLOAD_LENGTH - 1] = 0;
}
#endif
