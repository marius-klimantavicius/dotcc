#include <stdio.h>
#include <stddef.h>
#include <stdlib.h>

struct Bytes { int count; char data[]; };
struct Doubles { char tag; double data[]; };
struct Pair { int left; double right; };
struct Pairs { char tag; struct Pair data[]; };
typedef int (*Callback)(int);
struct Callbacks { char tag; Callback data[]; };
struct Pointers { char tag; void *data[]; };
struct Bits { unsigned a:3; unsigned b:5; double data[]; };
/* GCC's embedding extension lets the native oracle expose tail-header alignment
   as an actual offset, which the emitted sequential wrapper must also preserve. */
struct Wrapper { char lead; struct Doubles header; };
static int plus_one(int value) { return value + 1; }

int main(void) {
    struct Bytes *bytes = (struct Bytes*)malloc(sizeof(struct Bytes) + 3);
    struct Doubles *doubles = (struct Doubles*)malloc(sizeof(struct Doubles) + 2 * sizeof(double));
    struct Pairs *pairs = (struct Pairs*)malloc(sizeof(struct Pairs) + 2 * sizeof(struct Pair));
    struct Callbacks *callbacks = (struct Callbacks*)malloc(sizeof(struct Callbacks) + sizeof(Callback));
    struct Pointers *pointers = (struct Pointers*)malloc(sizeof(struct Pointers) + sizeof(void*));
    struct Bits *bits = (struct Bits*)malloc(sizeof(struct Bits) + sizeof(double));
    struct Wrapper wrapper;
    bytes->count = 3; bytes->data[0] = 10; bytes->data[1] = 20; bytes->data[2] = 30;
    doubles->tag = 7; doubles->data[0] = 1.5; doubles->data[1] = 2.5;
    pairs->data[0].left = 4; pairs->data[1].left = 9; pairs->data[1].right = 3.5;
    callbacks->data[0] = plus_one;
    pointers->data[0] = bytes;
    bits->a = 3; bits->b = 7; bits->data[0] = 4.5;
    printf("%zu %zu %zu %zu %zu %zu\n", sizeof(struct Bytes), sizeof(struct Doubles), sizeof(struct Pairs),
        sizeof(struct Callbacks), sizeof(struct Pointers), sizeof(struct Bits));
    printf("%zu %zu %zu %zu %zu %zu\n", _Alignof(struct Bytes), _Alignof(struct Doubles), _Alignof(struct Pairs),
        _Alignof(struct Callbacks), _Alignof(struct Pointers), _Alignof(struct Bits));
    printf("%zu %zu %zu %zu %zu %zu\n", offsetof(struct Bytes,data), offsetof(struct Doubles,data), offsetof(struct Pairs,data),
        offsetof(struct Callbacks,data), offsetof(struct Pointers,data), offsetof(struct Bits,data));
    printf("%ld %ld %ld %ld %ld %ld\n", (long)((char*)bytes->data-(char*)bytes), (long)((char*)doubles->data-(char*)doubles),
        (long)((char*)pairs->data-(char*)pairs), (long)((char*)callbacks->data-(char*)callbacks),
        (long)((char*)pointers->data-(char*)pointers), (long)((char*)bits->data-(char*)bits));
    printf("%zu %ld\n", sizeof(struct Wrapper), (long)((char*)&wrapper.header - (char*)&wrapper));
    printf("%d %.1f %d %.1f %d %d %u %u %.1f\n", bytes->data[2], doubles->data[1], pairs->data[1].left,
        pairs->data[1].right, callbacks->data[0](41), ((struct Bytes*)pointers->data[0])->count, bits->a, bits->b, bits->data[0]);
    free(bytes); free(doubles); free(pairs); free(callbacks); free(pointers); free(bits);
    return 0;
}
