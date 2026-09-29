/* Common C arithmetic type must survive arbitrarily nested conditionals,
 * including when both inner arms are comparisons (rendered as CBool). */
#include <stdio.h>
int classify(int x) { return (x > 0 ? (x == 1) : 0) ? 100 : 200; }
static int calls;
int tick(int n) { calls++; return n; }
int nested(int mode, int a, int b) {
    return (mode < 0 ? -7 : (mode ? (a == b) : (a != b))) ? 11 : 22;
}
int main(void) {
    int values[4] = {-3, 0, 1, 2};
    printf("%d %d %d\n", classify(1), classify(5), classify(-1));
    for (int i = 0; i < 4; i++) printf("%s%d,%d", i ? " " : "", nested(values[i],1,1), nested(values[i],1,2));
    printf("\n");
    for (int i = 0; i < 4; i++) {
        int m = values[i];
        int x = m < 0 ? -9 : (m ? (tick(1) == 1) : (tick(0) != 0));
        int y = m < 0 ? (m ? (tick(1) == 1) : (tick(0) != 0)) : -11;
        printf("%d,%d ", x, y);
    }
    printf("calls=%d\n",calls);
    return 0;
}
