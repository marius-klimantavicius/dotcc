#define _POSIX_C_SOURCE 200809L
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define CHECK(test) do { if (!(test)) return __LINE__; } while (0)

/* Exercise musl's failure branch deterministically without exhausting the host
   or changing production allocation. Normal cases below use the real heap. */
static int fail_next_allocation;
static size_t requested_size;
static void *string_test_malloc(size_t size)
{
    requested_size = size;
    if (fail_next_allocation) {
        fail_next_allocation = 0;
        errno = ENOMEM;
        return NULL;
    }
    return malloc(size);
}
#define malloc string_test_malloc
#define strdup string_test_strdup
#define strndup string_test_strndup
#include "../runtime/musl/string_alloc/strdup.c"
#include "../runtime/musl/string_alloc/strndup.c"
#undef malloc
#undef strdup
#undef strndup

int main(void)
{
    char raw[] = {'A', (char)0x80, (char)0xff, (char)0xc3, (char)0xa9, 0, 'X'};
    char unterminated[] = {'A', (char)0xff, 'C'};
    unsigned checksum = 0;
    char *a = strdup(raw);
    char *b = strdup(raw);
    CHECK(a && b && a != b && a != raw && b != raw);
    CHECK(strlen(a) == 5 && !memcmp(a, raw, 6) && !memcmp(b, raw, 6));
    raw[0] = 'Z';
    a[1] = 'q';
    CHECK(b[0] == 'A' && (unsigned char)b[1] == 0x80 && a[0] == 'A');
    free(a);
    free(b);
    a = strdup("");
    CHECK(a && a[0] == 0);
    free(a);
    for (size_t n = 0; n < 10; ++n) {
        a = strndup(raw, n);
        size_t expected = n < 5 ? n : 5;
        CHECK(a && strlen(a) == expected && a[expected] == 0);
        CHECK(!memcmp(a, raw, expected));
        checksum += (unsigned)expected;
        free(a);
    }
    a = strndup(unterminated, sizeof unterminated);
    CHECK(a && !memcmp(a, unterminated, sizeof unterminated) && a[3] == 0);
    free(a);
    a = strndup(unterminated, 0);
    CHECK(a && a[0] == 0);
    free(a);
    a = strndup("bounded", SIZE_MAX);
    CHECK(a && !strcmp(a, "bounded"));
    free(a);
    for (unsigned i = 0; i < 2048; ++i) {
        a = strdup(raw);
        CHECK(a && !memcmp(a, raw, 6));
        checksum += (unsigned char)a[i % 5];
        free(a);
    }
    fail_next_allocation = 1;
    errno = 0;
    a = string_test_strdup("failure");
    CHECK(!a && errno == ENOMEM && requested_size == 8 && !fail_next_allocation);
    fail_next_allocation = 1;
    errno = 0;
    a = string_test_strndup(unterminated, sizeof unterminated);
    CHECK(!a && errno == ENOMEM && requested_size == 4 && !fail_next_allocation);
    fail_next_allocation = 1;
    errno = 0;
    a = string_test_strndup(unterminated, 0);
    CHECK(!a && errno == ENOMEM && requested_size == 1 && !fail_next_allocation);
    /* The allocation failure does not poison subsequent successful operations. */
    a = string_test_strdup(raw);
    CHECK(a && !memcmp(a, raw, 6));
    free(a);
    printf("strdup/strndup: byte preservation, bounds, ownership, failure; checksum=%u\n", checksum);
    return 0;
}
