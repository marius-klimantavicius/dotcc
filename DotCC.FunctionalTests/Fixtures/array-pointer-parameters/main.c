#include <stdio.h>

/* Explicit row pointers must keep row stride, including in prototypes. */
double sum(double (*)[2], int);
double sum(double (*rows)[2], int count) {
    double result = 0;
    for (int i = 0; i < count; i++) result += rows[i][0] + rows[i][1];
    printf("%lu %lu ", (unsigned long)sizeof(rows), (unsigned long)sizeof(*rows));
    return result;
}
int cube(int (*)[2][3]);
int cube(int (*values)[2][3]) {
    values[1][1][2] = 31;
    return values[0][1][2] + values[1][1][2];
}
int main(void) {
    double rows[3][2] = {{1,2},{3,4},{5,6}};
    int values[2][2][3] = {{{1,2,3},{4,5,6}},{{7,8,9},{10,11,12}}};
    double result = sum(rows, 3);
    int total = cube(values);
    printf("%.0f %d %d\n", result, total, values[1][1][2]);
    return 0;
}
