#include <stdio.h>
int main(void) {
    int phase = 3, rc, values[3] = {7, 9};
    int base = 42;
    int *pointer = &base, *pointers[2] = {&base, 0};
    int matrix[2][2] = {{1, 2}, {3}}, last = 8;
    rc = values[0] + values[1] + values[2];
    printf("%d %d %d %d %d %d %d\n", phase, rc, *pointer, *pointers[0], pointers[1] == NULL, matrix[1][0] + matrix[1][1], last);
    return 0;
}
