#ifndef BLINK_CAMPAIGN_DECODER_CONFIG_H
#define BLINK_CAMPAIGN_DECODER_CONFIG_H
/* Decoder-only profile. No host platform/compiler identity is fabricated. */
#define DISABLE_JIT 1
#define DISABLE_X87 1
#define DISABLE_BMI2 1
#define DISABLE_METAL 1
#define DISABLE_THREADS 1
#define DISABLE_FORK 1
#endif
