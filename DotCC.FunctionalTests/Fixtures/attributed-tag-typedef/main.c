#include <stddef.h>
#include <stdio.h>

typedef int Number;
typedef struct payloadHeader payloadHeader;
typedef struct __attribute__((__packed__)) payloadHeader {
    char flag;
    Number Number;
    short tail;
} payloadHeader;

typedef union Bits Bits;
union __attribute__((packed)) Bits { unsigned char byte; unsigned int word; };
struct Outer { char before; payloadHeader header; char after; };
struct Metadata { payloadHeader payloadHeader; payloadHeader *next; };

_Static_assert(sizeof(payloadHeader) == 7 && _Alignof(payloadHeader) == 1, "header layout");
_Static_assert(offsetof(payloadHeader, Number) == 1 && offsetof(payloadHeader, tail) == 5, "header offsets");
_Static_assert(sizeof(struct Outer) == 9 && offsetof(struct Outer, header) == 1
    && offsetof(struct Outer, after) == 8, "nested layout");
_Static_assert(sizeof(Bits) == 4 && _Alignof(Bits) == 1, "union layout");

static Number read_value(payloadHeader *payloadHeader);
static Number read_value(payloadHeader *payloadHeader) { return payloadHeader->Number; }
static void write_value(payloadHeader *header, Number value) { header->Number = value; }

int main(void) {
    struct Outer rows[2] = {{1, {2, 3, 4}, 7}, {8, {9, 10, 11}, 12}};
    unsigned char bytes[16] = {0};
    payloadHeader *unaligned = (payloadHeader *)(bytes + 1);
    payloadHeader *second = &rows[1].header;
    write_value(unaligned, 0x01020304);
    write_value(second, 42);
    unsigned char *raw = (unsigned char *)rows;
    Bits bits;
    bits.word = 0x01020304;
    printf("layout %d %d %d %d %d %d %d\n", (int)sizeof(payloadHeader), (int)_Alignof(payloadHeader),
        (int)offsetof(payloadHeader, Number), (int)offsetof(payloadHeader, tail), (int)sizeof(struct Outer),
        (int)sizeof(Bits), (int)_Alignof(Bits));
    printf("storage %d %d %d %d %d %d %d\n", (int)((char *)&rows[1] - (char *)&rows[0]),
        rows[0].after, (int)read_value(second), raw[11], bytes[2], bytes[5], bits.byte);
    return read_value(second) != 42 || rows[0].after != 7 || raw[11] != 42 || bytes[2] != 4 || bytes[5] != 1;
}
