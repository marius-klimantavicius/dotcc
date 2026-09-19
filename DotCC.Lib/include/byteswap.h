#ifndef _DOTCC_BYTESWAP_H
#define _DOTCC_BYTESWAP_H
#include <stdint.h>

/* Unsigned operations avoid signed-shift overflow. Functions evaluate each
   operand once, including side effects passed through the public macros. */
static inline uint16_t __dotcc_bswap_16(uint16_t value) {
    return (uint16_t)((value >> 8) | (value << 8));
}
static inline uint32_t __dotcc_bswap_32(uint32_t value) {
    return ((value & 0xffU) << 24) | ((value & 0xff00U) << 8) |
           ((value >> 8) & 0xff00U) | ((value >> 24) & 0xffU);
}
static inline uint64_t __dotcc_bswap_64(uint64_t value) {
    return ((uint64_t)__dotcc_bswap_32((uint32_t)value) << 32) |
           __dotcc_bswap_32((uint32_t)(value >> 32));
}
#define bswap_16(x) __dotcc_bswap_16(x)
#define bswap_32(x) __dotcc_bswap_32(x)
#define bswap_64(x) __dotcc_bswap_64(x)
#define __bswap_16(x) __dotcc_bswap_16(x)
#define __bswap_32(x) __dotcc_bswap_32(x)
#define __bswap_64(x) __dotcc_bswap_64(x)
#endif
