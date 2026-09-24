#include <stdio.h>
#include <stdlib.h>

__attribute__((malloc, alloc_size(1), noinline))
void *allocate_one(size_t size) { return malloc(size); }

__attribute__((__malloc__, __alloc_size__(1, 2), noinline))
void *allocate_many(size_t count, size_t size) { return calloc(count, size); }

int main(void) {
    int *one = allocate_one(sizeof(int));
    int *many = allocate_many(3, sizeof(int));
    if (!one || !many) return 1;
    *one = 7;
    many[2] = 9;
    printf("%d %d %d\n", *one, many[0], many[2]);
    free(one);
    free(many);
    return 0;
}
