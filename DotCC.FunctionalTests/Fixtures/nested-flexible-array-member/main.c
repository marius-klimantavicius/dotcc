#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
struct TaggedStruct { char prefix; struct Entry { int value; } entries[]; };
struct TaggedUnion { char prefix; union Word { long value; int small; } entries[]; };
struct AnonymousStruct { char prefix; struct { int value; } entries[]; };
struct AnonymousUnion { char prefix; union { long value; int small; } entries[]; };
int main(void) {
    struct TaggedStruct *a = malloc(sizeof(struct TaggedStruct) + 2 * sizeof(struct Entry));
    struct TaggedUnion *b = malloc(sizeof(struct TaggedUnion) + 2 * sizeof(union Word));
    struct AnonymousStruct *c = malloc(sizeof(struct AnonymousStruct) + 16);
    struct AnonymousUnion *d = malloc(sizeof(struct AnonymousUnion) + 16);
    if (!a || !b || !c || !d) return 1;
    a->entries[1].value = 42;
    b->entries[1].value = 17;
    c->entries[1].value = 19;
    d->entries[1].value = 23;
    printf("%zu %zu %zu %zu %d %ld %d %ld\n",
        sizeof(struct TaggedStruct), offsetof(struct TaggedStruct, entries),
        sizeof(struct TaggedUnion), offsetof(struct TaggedUnion, entries),
        a->entries[1].value, b->entries[1].value, c->entries[1].value, d->entries[1].value);
    free(a); free(b); free(c); free(d);
    return 0;
}
