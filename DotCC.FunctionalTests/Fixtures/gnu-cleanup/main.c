#include <stdio.h>
static int visits;
static void cleanup_int(int *value) { visits = visits * 10 + *value; }
static int inner(void) {
    __attribute__((cleanup(cleanup_int))) int first = 1;
    __attribute__((cleanup(cleanup_int))) int second = 2;
    if (first) return 7;
    return 0;
}
struct owner { int value; };
static void cleanup_owner(struct owner *owner) { visits = visits * 10 + owner->value; }
static void cleanup_pointer(int **owner) { if (*owner) visits = visits * 10 + **owner; }
static void branches(void) {
    for (int i = 0; i < 3; ++i) {
        __attribute__((cleanup(cleanup_int))) int item = i + 3;
        if (i == 0) continue;
        if (i == 1) break;
    }
    {
        __attribute__((cleanup(cleanup_owner))) struct owner owner = { .value = 5 };
        {
            __attribute__((cleanup(cleanup_int))) int item = 6;
            goto outside;
        }
    }
outside:
    {
        int pointed = 7;
        __attribute__((cleanup(cleanup_pointer))) int *pointer = &pointed;
        pointer = 0; /* upstream's disable-after-ownership-transfer pattern */
    }
}
int main(void) {
    int result = inner();
    printf("%d %d\n", result, visits);
    visits = 0;
    branches();
    printf("%d\n", visits);
    return visits != 3465;
}
