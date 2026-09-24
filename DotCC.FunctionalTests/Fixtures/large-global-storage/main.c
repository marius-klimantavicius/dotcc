#include <stdio.h>

struct Big {
    unsigned char bytes[70000];
    int marker;
};

static struct Big global = {.marker = 7};
static _Thread_local struct Big local = {.marker = 11};

int main(void) {
    struct Big *global_address = &global;
    struct Big *local_address = &local;
    if (global.marker != 7 || local.marker != 11 ||
        global.bytes[0] != 0 || local.bytes[69999] != 0) return 1;
    global.bytes[0] = 23;
    global.bytes[69999] = 42;
    local.bytes[0] = 31;
    local.bytes[69999] = 57;
    printf("%d %d %d %d %d %d\n", global.marker, local.marker,
           global_address->bytes[0], global_address->bytes[69999],
           local_address->bytes[0], local_address->bytes[69999]);
    return 0;
}
