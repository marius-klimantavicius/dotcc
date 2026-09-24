#include <stdio.h>

static unsigned x[3] = {1, 2, 3}, a[3] = {4, 5}, c = 6;
typedef int Row[2];
Row rows[2] = {{7, 8}, {9, 10}}, other = {11};
int before = 12, indexed[4] = {[2] = 13}, after = 14;

int main(void) {
    x[1] += a[0];
    rows[1][0] += other[0];
    printf("%u %u %u %u %zu %zu\n", x[1], a[1], a[2], c, sizeof(x), sizeof(a));
    printf("%d %d %d %d %d %d %zu\n", rows[1][0], other[1], before, indexed[0], indexed[2], after, sizeof(rows));
    return 0;
}
