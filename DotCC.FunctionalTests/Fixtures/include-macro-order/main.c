#include <stdio.h>
#include "late.h"
static int F(int x) { return x + 100; }
int main(void) {
    int value =
#include "tail.h"
        (41);
    printf("%d %d %d %d %d %d\n", alloc(5), (alloc)(5), during(), prior(), token, value);
    return 0;
}
