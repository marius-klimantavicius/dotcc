#include <stdio.h>
#include "shared.h"
static int get_time(void) { return 17; }
Clock shared_clock = {get_time};
int main(void) {
    printf("%d %d %d\n", shared_clock.cb(), shared_clock.omitted, read_clock());
    return 0;
}
