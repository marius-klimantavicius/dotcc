#include "mbedtls/sha256.h"
#include "mbedtls/platform_util.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

int main() {
    const size_t sizes[] = {0, 1, 55, 56, 63, 64, 65, 127, 128, 129, 4096, 65536};
    for (int pattern = 0; pattern < 3; ++pattern) {
        for (size_t size : sizes) {
            unsigned char *allocation = (unsigned char *)malloc(size + 2);
            if (!allocation) return 1;
            unsigned char *input = allocation + 1;
            unsigned state = 0x12345678U;
            for (size_t i = 0; i < size; ++i) {
                state = 1664525U * state + 1013904223U;
                input[i] = (unsigned char)(pattern == 0 ? state >> 24 : pattern == 1 ? i % 7 : 'a');
            }
            unsigned char one_shot[34], streamed[34], cloned[34];
            memset(one_shot, 0xa5, sizeof(one_shot));
            memset(streamed, 0xa5, sizeof(streamed));
            memset(cloned, 0xa5, sizeof(cloned));
            if (mbedtls_sha256(input, size, one_shot + 1, 0)) return 2;
            mbedtls_sha256_context first, second;
            mbedtls_sha256_init(&first);
            mbedtls_sha256_init(&second);
            if (mbedtls_sha256_starts(&first, 0) || mbedtls_sha256_update(&first, input, size / 2)) return 3;
            mbedtls_sha256_clone(&second, &first);
            for (size_t offset = size / 2; offset < size;) {
                size_t count = size - offset < 23 ? size - offset : 23;
                if (mbedtls_sha256_update(&first, input + offset, count)) return 4;
                offset += count;
            }
            if (mbedtls_sha256_update(&second, input + size / 2, size - size / 2) ||
                mbedtls_sha256_finish(&first, streamed + 1) || mbedtls_sha256_finish(&second, cloned + 1)) return 5;
            if (memcmp(one_shot, streamed, sizeof(one_shot)) || memcmp(one_shot, cloned, sizeof(one_shot)) ||
                one_shot[0] != 0xa5 || one_shot[33] != 0xa5) return 6;
            mbedtls_sha256_free(&first);
            mbedtls_sha256_free(&second);
            for (size_t i = 0; i < sizeof(first); ++i)
                if (((unsigned char *)&first)[i] || ((unsigned char *)&second)[i]) return 7;
            printf("%d %lu ", pattern, (unsigned long)size);
            for (int i = 1; i <= 32; ++i) printf("%02x", (unsigned)one_shot[i]);
            puts("");
            mbedtls_platform_zeroize(input, size);
            for (size_t i = 0; i < size; ++i) if (input[i]) return 8;
            free(allocation);
        }
    }
    mbedtls_sha256_free(NULL);
    mbedtls_platform_zeroize(NULL, 0);
    return 0;
}
