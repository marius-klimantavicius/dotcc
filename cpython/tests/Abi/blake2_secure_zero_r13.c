/* Exercise the same-pin algorithms without changing any upstream input.
 * Expected digests are retained from the exact-pin native _blake2 oracle.
 * Hashing remains in blake2{b,s}-ref.c; this file only asserts its API. */
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "blake2.h"
#include "blake2-impl.h"

static int checks;
static void check(int ok) { if (!ok) abort(); ++checks; }
static void hex(const uint8_t *bytes, size_t n, const char *expected)
{
    const char *digits = "0123456789abcdef";
    for (size_t i = 0; i < n; ++i) {
        if (digits[bytes[i] >> 4] != expected[i * 2] ||
            digits[bytes[i] & 15] != expected[i * 2 + 1]) abort();
    }
    check(expected[n * 2] == 0);
}

#define CHECK_HASH(kind, size, keysize, empty, abc, longmsg, keyed) do { \
    uint8_t output[size], duplicate[size], key[keysize]; \
    kind##_state state, copy; \
    for (size_t i = 0; i < sizeof key; ++i) key[i] = (uint8_t)i; \
    check(kind(output, "", NULL, size, 0, 0) == 0); hex(output, size, empty); \
    check(kind(output, "abc", NULL, size, 3, 0) == 0); hex(output, size, abc); \
    check(kind(output, input, NULL, size, sizeof input, 0) == 0); hex(output, size, longmsg); \
    check(kind(output, input, key, size, sizeof input, sizeof key) == 0); hex(output, size, keyed); \
    check(kind##_init_key(&state, size, key, sizeof key) == 0); \
    check(kind##_update(&state, input, 73) == 0); \
    memcpy(&copy, &state, sizeof state); \
    check(kind##_update(&state, input + 73, sizeof input - 73) == 0); \
    check(kind##_final(&state, output, sizeof output) == 0); hex(output, size, keyed); \
    check(kind##_update(&copy, input + 73, 1) == 0); \
    check(kind##_update(&copy, input + 74, sizeof input - 74) == 0); \
    check(kind##_final(&copy, duplicate, sizeof duplicate) == 0); \
    check(memcmp(output, duplicate, size) == 0); \
    check(kind##_init(&state, size) == 0); \
    check(kind##_update(&state, (const uint8_t *)"abc", 3) == 0); \
    check(kind##_final(&state, output, sizeof output) == 0); hex(output, size, abc); \
    check(kind##_init(&state, 0) == -1); \
    check(kind##_init_key(&state, size, NULL, sizeof key) == -1); \
    check(kind##_init_key(&state, size, key, keysize + 1) == -1); \
    secure_zero_memory(&state, sizeof state); \
    secure_zero_memory(&copy, sizeof copy); \
    const uint8_t *raw = (const uint8_t *)&state; \
    for (size_t i = 0; i < sizeof state; ++i) if (raw[i] != 0) abort(); \
    check(1); \
} while (0)

int main(void)
{
    uint8_t input[200];
    for (size_t i = 0; i < sizeof input; ++i) input[i] = (uint8_t)i;
    CHECK_HASH(blake2b, 64, 64,
        "786a02f742015903c6c6fd852552d272912f4740e15847618a86e217f71f5419d25e1031afee585313896444934eb04b903a685b1448b755d56f701afe9be2ce",
        "ba80a53f981c4d0d6a2797b69f12f6e94c212f14685ac4b74b12bb6fdbffa2d17d87c5392aab792dc252d5de4533cc9518d38aa8dbf1925ab92386edd4009923",
        "fb3c1f0f56a56f8e316fdf5d853c8c872c39635d083634c3904fc3ac07d1b578e85ff0e480e92d44ade33b62e893ee32343e79ddf6ef292e89b582d312502314",
        "3095a349d245708c7cf550118703d7302c27b60af5d4e67fc978f8a4e60953c7a04f92fcf41aee64321ccb707a895851552b1e37b00bc5e6b72fa5bcef9e3fff");
    CHECK_HASH(blake2s, 32, 32,
        "69217a3079908094e11121d042354a7c1f55b6482ca1a51e1b250dfd1ed0eef9",
        "508c5e8c327c14e2e1a72ba34eeb452f37458b209ed63a294d999b4c86675982",
        "6d244e1a06ce4ef578dd0f63aff0936706735119ca9c8d22d86c801414ab9741",
        "13c88480a5d00d6c8c7ad2110d76a82d9b70f4fa6696d4e5dd42a066dcaf9920");
    printf("blake2 actual-source checks=%d variants=2 vectors=8\n", checks);
    return 0;
}
