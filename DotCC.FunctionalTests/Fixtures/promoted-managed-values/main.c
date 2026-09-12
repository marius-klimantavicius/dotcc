#include <stdio.h>
#include <stddef.h>
typedef int (*Callback)(int);
struct Example {
    unsigned marker;
    union { unsigned long flags; struct { unsigned long enabled:1; unsigned long mode:3; } bits; };
    struct { union { int number; unsigned unsigned_number; }; Callback callback; const int fixed_value; int *pointer; };
};
static int add(int x) { return x + 5; }
int main(void) {
    struct Example value = {0};
    value.marker = 7;
    value.bits.enabled = 1;
    value.bits.mode = 5;
    value.unsigned_number = ~0u;
    value.callback = add;
    int pointee = 17;
    value.pointer = &pointee;
    printf("%lu %lu %lu %lu\n", (unsigned long)sizeof(value), (unsigned long)offsetof(struct Example, flags), (unsigned long)offsetof(struct Example, number), (unsigned long)offsetof(struct Example, pointer));
    printf("%u %lu %d %d %d %d\n", value.marker, value.flags, value.number, value.callback(4), value.fixed_value, *value.pointer);
    return 0;
}
