#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

struct String {
    uint32_t len : 31;
    uint8_t wide : 1;
    uint32_t hash : 30;
    uint8_t type : 2;
    uint32_t next;
    union { uint8_t bytes[0]; uint16_t words[0]; } u;
};
struct Overlap {
    int prefix;
    union { unsigned char bytes[0]; unsigned short words[0]; } u;
    unsigned char marker;
};
int main(void) {
    struct String *s = malloc(sizeof(*s) + 8);
    if (!s) return 1;
    s->len = 2; s->wide = 1; s->hash = 19; s->type = 2; s->next = 31;
    s->u.words[0] = 0x1234; s->u.words[1] = 0x5678;
    if ((unsigned char *)&s->u - (unsigned char *)s != 12) return 2;
    if (s->u.bytes[0] != 0x34 || s->u.bytes[1] != 0x12 || s->u.words[1] != 0x5678) return 3;
    if (s->len != 2 || !s->wide || s->hash != 19 || s->type != 2 || s->next != 31) return 4;
    struct Overlap value = {0};
    value.prefix = 42; value.u.bytes[0] = 91;
    if (value.marker != 91 || value.prefix != 42) return 5;
    printf("header %zu tail %zu union %zu align %zu overlap %zu/%zu values %x %x %u\n",
      sizeof(*s), offsetof(struct String,u), sizeof(s->u), _Alignof(__typeof__(s->u)),
      offsetof(struct Overlap,u), offsetof(struct Overlap,marker), s->u.words[0],s->u.words[1],value.marker);
    free(s);
    return 0;
}
