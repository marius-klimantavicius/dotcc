/* Test services only. Upstream tests and assertions are in the emitted C unit. */
#include "host.h"
#include <stdint.h>
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#if !defined(_WIN32)
#include <unistd.h>
#endif

typedef struct { void *base; size_t len; uint8_t alignment; } Header;
static size_t live, passed, skipped, failed;

void dotcc_upstream_host_log(const uint8_t *bytes, size_t len) {
    fwrite(bytes, 1, len, stderr);
    fputc('\n', stderr);
}

size_t dotcc_upstream_host_cpu_count(void) {
#if defined(_SC_NPROCESSORS_ONLN)
    long count = sysconf(_SC_NPROCESSORS_ONLN);
    return count > 0 ? (size_t)count : 1;
#else
    return 1; /* The runner executes one case at a time on one thread. */
#endif
}

static void require(int condition) {
    if (!condition) { fputs("FAIL upstream host allocation contract\n", stderr); abort(); }
}
void *dotcc_upstream_host_alloc(size_t len, uint8_t alignment) {
    require(alignment < sizeof(size_t) * 8 - 1);
    size_t align = (size_t)1 << alignment;
    if (align < _Alignof(Header)) align = _Alignof(Header);
    require(len <= SIZE_MAX - align - sizeof(Header));
    void *base = malloc(len + align + sizeof(Header));
    if (!base) return NULL;
    uintptr_t address = ((uintptr_t)base + sizeof(Header) + align - 1) & ~(uintptr_t)(align - 1);
    Header *h = (Header *)address - 1;
    h->base = base; h->len = len; h->alignment = alignment; live++;
    return (void *)address;
}
void dotcc_upstream_host_free(void *ptr, size_t len, uint8_t alignment) {
    Header *h = (Header *)ptr - 1;
    require(live > 0 && h->len == len && h->alignment == alignment);
    live--; free(h->base);
}
void dotcc_upstream_host_report(const uint8_t *name, size_t len, int status,
                               const uint8_t *error, size_t error_len) {
    if (status == 0) passed++;
    else if (status == 1) skipped++;
    else failed++;
    printf("%s %.*s", status == 0 ? "PASS" : status == 1 ? "SKIP" : "FAIL", (int)len, name);
    if (error_len) printf(" error=%.*s", (int)error_len, error);
    putchar('\n');
    fflush(stdout);
}
size_t dotcc_upstream_count(void);
int dotcc_upstream_run(size_t index);
const uint8_t *dotcc_upstream_name(size_t index, size_t *len);
int dotcc_upstream_host_finish(size_t count) {
    printf("RESULT upstream-cases=%zu passed=%zu skipped=%zu failed=%zu live=%zu\n",
           count, passed, skipped, failed, live);
    return failed || live || passed + skipped != count ? 1 : 0;
}
#if !defined(DOTCC_UPSTREAM_HOST_NO_MAIN)
int main(void) {
    size_t count = dotcc_upstream_count();
    for (size_t index = 0; index < count; index++) {
        size_t len;
        const uint8_t *name = dotcc_upstream_name(index, &len);
        fprintf(stderr, "RUN %.*s\n", (int)len, name);
        dotcc_upstream_run(index);
    }
    return dotcc_upstream_host_finish(count);
}
#endif
