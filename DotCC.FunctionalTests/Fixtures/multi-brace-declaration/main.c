#include <stdio.h>
typedef struct { int a; int b; } Pair;
typedef struct { int cells[2]; Pair pair; } Nested;
Pair global_first = {1}, global_second = {.b=2};
static Pair static_first = {0}, static_second = {3};
int counter;
int bump(void) { return ++counter; }
int persistent(void) {
    static Pair first = {4}, second = {.b=5};
    first.a++;
    return first.a + second.b;
}
int main(void) {
    Pair first = {bump()}, second = {first.a+bump()}, third = {.b=bump()};
    printf("order %d %d %d %d %d %d %d\n", first.a, first.b, second.a, second.b, third.a, third.b, counter);
    Nested nested = {{7},{8}}, other = {{9},{10,11}};
    printf("nested %d %d %d %d %d %d %d %d\n", nested.cells[0], nested.cells[1], nested.pair.a, nested.pair.b, other.cells[0], other.cells[1], other.pair.a, other.pair.b);
    Pair value = {4}, array[2] = {{value.a+1},{6}}, last = {array[1].a+1};
    Pair array_head[2] = {{1},{2}}, tail = {3};
    printf("arrays %d %d %d %d %d %d %d\n", value.a, array[0].a, array[1].a, last.a, array_head[0].a, array_head[1].a, tail.a);
    Pair *const pointer = {&value}, object = {12}, *other_pointer = {&object};
    typedef Pair *PairPointer;
    PairPointer alias_first = {&value}, alias_second = {&object};
    int scalar = bump(), braced = {bump()}, dependent = {braced+bump()};
    printf("types %d %d %d %d %d scalar %d %d %d %d\n", pointer->a, object.a, other_pointer->a, alias_first->a, alias_second->a, scalar, braced, dependent, counter);
    printf("globals %d %d %d %d %d %d %d %d\n", global_first.a, global_first.b, global_second.a, global_second.b, static_first.a, static_first.b, static_second.a, static_second.b);
    int once = persistent(), twice = persistent();
    printf("static %d %d\n", once, twice);
    return 0;
}
