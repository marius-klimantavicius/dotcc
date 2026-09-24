#include <stdio.h>
static int calls;
static int rows(void) { calls++; return 3; }
static int sum(int (*values)[2], int count) {
    int total=0;
    for (int i=0; i<count; i++) total += values[i][0] + values[i][1];
    return total;
}
int main(void) {
    int count = rows();
    int values[count++][2];
    for (int i=0; i<3; i++) for(int j=0; j<2; j++) values[i][j] = i*2+j+1;
    printf("%d %d %lu %lu %d\n", calls, count, (unsigned long)sizeof(values),
        (unsigned long)sizeof(values[0]), sum(values,3));
    int cube[rows()][2][3];
    cube[2][1][2]=17;
    printf("%d %lu %lu %d\n", calls, (unsigned long)sizeof(cube),
        (unsigned long)sizeof(cube[0]), cube[2][1][2]);
    return 0;
}
