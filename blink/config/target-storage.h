#ifndef BLINK_CAMPAIGN_TARGET_STORAGE_H
#define BLINK_CAMPAIGN_TARGET_STORAGE_H
/* Qualified Linux x64 managed/native storage profile. These compatibility
 * constants describe byte representation; they do not claim a native CPU,
 * operating system, or GCC implementation. Big-endian execution is unqualified. */
#ifndef __ORDER_LITTLE_ENDIAN__
#define __ORDER_LITTLE_ENDIAN__ 1234
#endif
#ifndef __ORDER_BIG_ENDIAN__
#define __ORDER_BIG_ENDIAN__ 4321
#endif
#ifndef __BYTE_ORDER__
#define __BYTE_ORDER__ __ORDER_LITTLE_ENDIAN__
#endif
#if __BYTE_ORDER__ != __ORDER_LITTLE_ENDIAN__
#error Blink campaign target storage requires qualified little-endian execution
#endif
#endif
