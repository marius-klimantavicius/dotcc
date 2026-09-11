#include <stdio.h>
struct value { int values[2]; int scalar; void *pointer; };
int main(void) {
    struct value first = {}, second = {.values = {}, .scalar = {}, .pointer = {}};
    struct value third = {.values = {7,8}, .scalar = {9}};
    third = (struct value){.values = {}, .scalar = {}, .pointer = {}};
    printf("%d %d %d %d %d %d\n", first.values[0], second.values[1], second.scalar, second.pointer == NULL, third.values[0], third.scalar);
    return 0;
}
