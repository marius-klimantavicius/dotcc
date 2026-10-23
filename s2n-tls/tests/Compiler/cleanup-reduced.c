#include <stdio.h>
static int visits;
static void cleanup_int(int *value) { visits = visits * 10 + *value; }
static int inner(void) {
    __attribute__((cleanup(cleanup_int))) int first = 1;
    __attribute__((cleanup(cleanup_int))) int second = 2;
    if (first) return 7;
    return 0;
}
int main(void) { int result = inner(); printf("%d %d\n", result, visits); return visits != 21; }
