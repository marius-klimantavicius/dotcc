#include <stdio.h>
#include <stddef.h>
typedef struct Base { int value; } Base;
struct Middle { Base; int extra; };
struct Container { struct Middle; int tail; };
typedef union Value { int integer; unsigned int bits; } Value;
struct Overlay { Value; int tail; };
int main(void) {
    struct Container container = {0};
    struct Container *pointer = &container;
    struct Overlay overlay = {0};
    pointer->value = 41;
    container.extra = 42;
    container.tail = 43;
    overlay.integer = 123;
    printf("%d %d %d %u %d %d\n", container.value, pointer->extra, container.tail,
        overlay.bits, (int)sizeof(container), (int)offsetof(struct Container, extra));
    return 0;
}
