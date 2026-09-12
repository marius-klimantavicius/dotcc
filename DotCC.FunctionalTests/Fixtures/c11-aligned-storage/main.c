#include <stdio.h>
#include <stddef.h>
#include <stdint.h>
#include <stdalign.h>
struct Aligned { alignas(16) unsigned char value; };
struct Nested { unsigned char prefix; struct Aligned values[2]; int tail; };
struct Aligned global;
struct Nested global_nested;
struct Aligned global_array[3];
int argument_alignment(struct Aligned value) { return (int)((uintptr_t)&value & 15); }
int main(void) {
    struct Aligned local = {23};
    struct Aligned array[3] = {{11}, {12}, {13}};
    struct Nested nested = {0};
    alignas(32) int scalar = 9;
    struct Aligned *temporary = &(struct Aligned){71};
    struct Aligned *temporary_array = (struct Aligned[]){{81}, {82}};
    int for_alignment = -1;
    for (struct Aligned index = {0}; index.value < 1; index.value++)
        for_alignment = (int)((uintptr_t)&index & 15);
    nested.values[1].value = 31;
    global.value = 41;
    global_array[2].value = 51;
    global_nested.values[1].value = 61;
    printf("layout %d %d %d %d %d\n", (int)sizeof(struct Aligned),
        (int)alignof(struct Aligned), (int)sizeof(struct Nested),
        (int)offsetof(struct Nested, values), (int)offsetof(struct Nested, tail));
    printf("storage %d %d %d %d %d %d %d %d\n", (int)((uintptr_t)&local & 15),
        (int)((uintptr_t)array & 15), (int)((uintptr_t)&nested & 15),
        (int)((uintptr_t)&global & 15), (int)((uintptr_t)&global_array[2] & 15),
        (int)((uintptr_t)&global_nested & 15), argument_alignment(local), (int)((uintptr_t)&scalar & 31));
    printf("values %d %d %d %d %d %d %d\n", local.value, array[1].value,
        nested.values[1].value, global.value, global_array[2].value,
        global_nested.values[1].value, scalar);
    printf("temporaries %d %d %d %d %d\n", (int)((uintptr_t)temporary & 15),
        (int)((uintptr_t)&temporary_array[1] & 15), for_alignment,
        temporary->value, temporary_array[1].value);
    return 0;
}
