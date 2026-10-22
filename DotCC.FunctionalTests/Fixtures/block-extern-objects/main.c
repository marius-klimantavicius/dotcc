#include <stdio.h>

int *storage_address(void);
static int internal = 8;
static int internal_read(void) { extern int internal; return internal; }

static int *block_address(void) { extern int shared; return &shared; }
static void mutate(void) { extern int shared; shared += 5; }
static int shadow(void) {
    int shared = 7;
    int *local = &shared;
    {
        extern int shared;
        shared += 2;
        if (&shared == local) return -1;
    }
    return shared;
}
static int point(void) {
    extern int *selected, counter;
    selected = &counter;
    return *selected + counter;
}
int main(void) {
    mutate();
    int local = shadow();
    int pointers = point();
    if (block_address() != storage_address()) return 1;
    printf("%d %d %d %d\n", *block_address(), local, pointers, internal_read());
    return *block_address() == 17 && local == 7 && pointers == 60 && internal_read() == 8 ? 0 : 2;
}
