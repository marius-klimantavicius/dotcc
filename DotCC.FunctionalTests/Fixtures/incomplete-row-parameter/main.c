#include <stdio.h>
struct Rows { char (*values)[4]; };
static void attach(struct Rows *target, char (*)[]);
static void attach(struct Rows *target, char (*values)[]) {
    target->values = values;
}
int main(void) {
    char values[2][4] = {{1,2,3,4},{5,6,7,8}};
    struct Rows rows = {0};
    attach(&rows, values);
    rows.values[1][2] = 19;
    printf("%lu %d %d\n", (unsigned long)sizeof(*rows.values), rows.values[0][3], values[1][2]);
    return 0;
}
