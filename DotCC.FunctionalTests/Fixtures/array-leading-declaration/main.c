#include <stdio.h>
typedef struct Page { int value; } Page;
typedef int *IntPtr;
int main(void) {
    Page values[2] = {{20}, {22}};
    Page *bucket[2], *p = &values[0], **pp = &p;
    int matrix[2][2], scalar = 7, *tail = &scalar;
    IntPtr pointerArray[2], other = &scalar;
    bucket[0] = p;
    bucket[1] = &values[1];
    matrix[1][1] = 17;
    pointerArray[0] = tail;
    pointerArray[1] = other;
    printf("%d %d %d %d %d %d\n", bucket[0]->value + bucket[1]->value,
        (*pp)->value, matrix[1][1], *pointerArray[1],
        (int)(sizeof(bucket) / sizeof(bucket[0])),
        (int)(sizeof(pointerArray) / sizeof(pointerArray[0])));
    return 0;
}
