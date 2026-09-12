#include <stdio.h>
#include <stdint.h>
#include <stddef.h>
#include <stdalign.h>
#define DECLARE(Name) struct Name { char prefix; int value; };
#pragma pack(push, outer, 1)
#include "packed.h"
DECLARE(MacroPacked)
struct Packet { unsigned char flags; uint32_t version; unsigned char length; };
struct Bits { unsigned a:20; unsigned b:20; unsigned char end; };
struct Wide { unsigned head:3; unsigned long long value:64; unsigned tail:5; unsigned char end; };
struct Zero { unsigned char prefix; unsigned a:5; unsigned :0; unsigned char end; };
struct Nested { unsigned char prefix; struct { uint32_t value; } inner; };
#pragma pack(push, inner, 2)
struct Cap2 { char prefix; uint32_t value; };
#pragma pack(pop, outer)
struct Natural { char prefix; uint32_t value; };
#pragma pack(push, 8)
struct Clamp { char prefix; alignas(16) char value; };
#pragma pack()
struct Reset { char prefix; uint32_t value; };
#pragma pack(pop)
int main(void) {
    struct Packet packet = {0xab, 0x12345678, 4};
    struct Bits bits = {0};
    bits.a = 0xabcde; bits.b = 0x54321; bits.end = 0x99;
    struct Wide wide = {0};
    wide.head = 5; wide.value = 0x123456789abcdef0ULL; wide.tail = 19; wide.end = 0x77;
    struct Nested nested = {0}; nested.inner.value = 42;
    unsigned char *bytes = (unsigned char*)&bits;
    unsigned char *wide_bytes = (unsigned char*)&wide;
    printf("layout %d %d %d %d %d %d %d %d\n", (int)sizeof(struct Packet), (int)alignof(struct Packet),
        (int)offsetof(struct Packet, version), (int)sizeof(struct Header), (int)alignof(struct Header),
        (int)sizeof(struct MacroPacked), (int)sizeof(struct Cap2), (int)sizeof(struct Natural));
    printf("nested %d %d clamp %d %d %d reset %d\n", (int)sizeof(struct Nested),
        (int)offsetof(struct Nested, inner), (int)sizeof(struct Clamp), (int)alignof(struct Clamp),
        (int)offsetof(struct Clamp, value), (int)sizeof(struct Reset));
    printf("bits %d %d %x %x %x", (int)sizeof(struct Bits), (int)offsetof(struct Bits, end), bits.a, bits.b, bits.end);
    for (int i = 0; i < sizeof(struct Bits); ++i) printf(" %02x", bytes[i]);
    printf("\nwide %d %d %x %llx %x %x", (int)sizeof(struct Wide), (int)offsetof(struct Wide, end),
        wide.head, wide.value, wide.tail, wide.end);
    for (int i = 0; i < sizeof(struct Wide); ++i) printf(" %02x", wide_bytes[i]);
    printf("\nzero %d %d values %x %x %d\n", (int)sizeof(struct Zero), (int)offsetof(struct Zero, end),
        packet.flags, packet.version, nested.inner.value);
    return 0;
}
