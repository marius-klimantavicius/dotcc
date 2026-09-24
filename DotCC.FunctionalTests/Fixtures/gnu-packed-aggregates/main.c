#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>

struct __attribute__((__packed__)) Header {
    uint64_t length;
    uint64_t capacity;
    unsigned char flags;
    char bytes[];
};
typedef struct __attribute__((packed, aligned(8))) Small {
    char tag;
    uint32_t value;
} Small;
union __attribute__((packed)) PackedUnion { char tag; uint64_t value; };
struct Outer { char lead; struct Header header; };

int main(void) {
    struct Header *h = malloc(sizeof(*h) + 4);
    if (!h) return 1;
    h->length = 3; h->capacity = 4; h->flags = 9;
    h->bytes[0] = 'a'; h->bytes[1] = 'b'; h->bytes[2] = 'c'; h->bytes[3] = 0;
    Small s = {'x', 0x12345678};
    union PackedUnion u; u.value = 17;
    printf("%zu %zu %zu %zu %zu %zu %zu %zu\n", sizeof(*h), _Alignof(struct Header),
           offsetof(struct Header, bytes), sizeof(Small), _Alignof(Small),
           offsetof(Small, value), sizeof(u), _Alignof(union PackedUnion));
    printf("%zu %zu %llu %u %s %u %llu\n", offsetof(struct Outer, header), sizeof(struct Outer),
           (unsigned long long)h->length, h->flags, h->bytes, s.value, (unsigned long long)u.value);
    free(h);
    return 0;
}
