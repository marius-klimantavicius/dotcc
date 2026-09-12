#ifndef MSQUIC_ABI_OBSERVE_H
#define MSQUIC_ABI_OBSERVE_H
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#define ABI_LAYOUT(T) printf("layout %s %lu %lu\n", #T, (unsigned long)sizeof(T), (unsigned long)_Alignof(T))
#define ABI_OFFSET(T, F) printf("offset %s.%s %lu\n", #T, #F, (unsigned long)offsetof(T, F))
static void abi_bytes(const char *name, const void *object, unsigned long length) {
    const unsigned char *bytes = (const unsigned char *)object;
    printf("bytes %s ", name);
    for (unsigned long index = 0; index < length; index++) printf("%02x", (unsigned int)bytes[index]);
    printf("\n");
}
#endif
