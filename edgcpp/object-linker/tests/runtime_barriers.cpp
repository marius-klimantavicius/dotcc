// Native reference for the GNU memory constraints used by yyjson and Mbed TLS.
// This is a detached optimization audit, not an owned-export acceptance test.
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static void *(*const volatile clear_memory)(void *, int, size_t) = memset;

extern "C" __attribute__((noinline)) int probe_reload(int *address) {
    int before = *address;
    __asm__ volatile("" : "=m"(*address));
    return before + *address;
}

extern "C" __attribute__((noinline)) int probe_coalesced(int *address) {
    return *address + *address;
}

extern "C" __attribute__((noinline)) void probe_scrub(void *address, size_t size) {
    if (size) {
        clear_memory(address, 0, size);
        __asm__ volatile("" : : "m"(*(char (*)[size])address));
    }
}

int main() {
    int value = 37;
    if (probe_reload(&value) != 74 || probe_coalesced(&value) != 74) return 1;
    unsigned char *bytes = (unsigned char *)malloc(4098);
    if (!bytes) return 2;
    memset(bytes, 0xa5, 4098);
    probe_scrub(bytes + 1, 4096);
    for (size_t i = 1; i <= 4096; ++i) if (bytes[i]) return 3;
    if (bytes[0] != 0xa5 || bytes[4097] != 0xa5) return 4;
    probe_scrub(bytes, 0);
    if (bytes[0] != 0xa5) return 5;
    free(bytes);
    puts("native reload and zeroization passed");
    return 0;
}
