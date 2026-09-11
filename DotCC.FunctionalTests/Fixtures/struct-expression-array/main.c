#include <stdio.h>
typedef struct { int a; int b; } Pair;
typedef union { int value; unsigned int bits; } Value;
int calls;
Pair make(int input) { Pair result = {input, ++calls}; return result; }
int main(void) {
    Pair values[3] = {make(10), make(20)};
    const Pair original = {30,40};
    Pair copies[] = {original, {50}, make(60)};
    copies[0].a = 99;
    Value source = {70};
    Value unions[2] = {source};
    unions[0].value = 80;
    printf("calls %d values %d %d %d %d %d %d\n", calls, values[0].a, values[0].b, values[1].a, values[1].b, values[2].a, values[2].b);
    printf("copies %d %d %d %d %d %d %d\n", original.a, copies[0].a, copies[0].b, copies[1].a, copies[1].b, copies[2].a, copies[2].b);
    printf("unions %d %d %d\n", source.value, unions[0].value, unions[1].value);
    return 0;
}
