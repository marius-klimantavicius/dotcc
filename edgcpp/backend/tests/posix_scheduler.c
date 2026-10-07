#define _GNU_SOURCE 1
#include <sched.h>
#include <stdio.h>
#include <stdlib.h>

int main(int argc, char **argv) {
    int checks = 0;
    if (argc != 2) return 1;
    ++checks;
    int limit = atoi(argv[1]);
    if (limit <= 0) return 2;
    ++checks;
    for (int i=0; i<1000; ++i) {
        int cpu = sched_getcpu();
        if (cpu < 0 || cpu >= limit) return 3;
        ++checks;
        if (sched_yield() != 0) return 4;
        ++checks;
    }
    printf("scheduler CPU range and yield: %d checks passed\n", checks);
    return checks != 2002;
}
