#include <stdio.h>
typedef struct Module { int version; int (*callback)(int); void *context; unsigned char flags; } Module;
static int increment(int value) { return value + 1; }
static const Module module = { .flags = 255, .callback = increment, .version = 12 };
Module exported = { .callback = increment, .version = 17 };
int main(void) {
    printf("%d %d %d %d\n", module.version, module.callback(41), module.context == NULL, module.flags);
    printf("%d %d %d\n", exported.version, exported.callback(16), exported.flags);
    return 0;
}
