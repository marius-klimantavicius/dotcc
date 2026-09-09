#include <stdio.h>
#include <stddef.h>

struct Cell { long key; void *value; };
typedef struct Outer Outer;
struct Outer {
    char tag;
    union { double scale; struct Cell cell; } payload;
    struct Cell entries[3];
    int (*callback)(int);
    unsigned bits : 3;
    unsigned more : 5;
    int tail;
};
struct Promoted { char tag; union { long a; double b; }; };
enum OffsetConstants { TAIL = offsetof(Outer, tail) };
_Static_assert(offsetof(Outer, entries[2].value) == 64, "indexed offsetof");
int main(void) {
    Outer value;
    struct Promoted promoted;
    char bound[offsetof(Outer, callback)];
    printf("%zu %zu %zu %zu %zu\n", sizeof(Outer), offsetof(Outer, payload.cell.value),
        offsetof(Outer, entries[2].value), offsetof(Outer, callback), sizeof(bound));
    printf("%ld %ld %ld %d\n", (long)((char*)&value.entries[2].value - (char*)&value),
        (long)((char*)&value.callback - (char*)&value), (long)((char*)&value.tail - (char*)&value), TAIL);
    printf("%zu %ld\n", offsetof(struct Promoted, b), (long)((char*)&promoted.b - (char*)&promoted));
    switch (TAIL) { case offsetof(Outer, tail): return 0; default: return 1; }
}
