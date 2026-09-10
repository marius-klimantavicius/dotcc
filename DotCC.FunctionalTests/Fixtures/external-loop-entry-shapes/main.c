#include <stdio.h>
static void unbraced_loop(void) {
    int tests = 0, bodies = 0;
    goto entered;
    while (++tests < 2)
entered:
    { ++bodies; break; }
    printf("unbraced %d %d\n", tests, bodies);
}
static void unbraced_if_loop(void) {
    int tests = 0, bodies = 0;
    goto entered;
    if (0) while (++tests < 2) {
entered:
        ++bodies;
        break;
    }
    printf("if-loop %d %d\n", tests, bodies);
}
static void switch_continue_and_arrays(void) {
    int i = 0, tests = 0, post = 0, total = 0;
    goto entered;
    for (i = 0; ++tests < 5; ++post) {
        int a[2][2] = {{3}, {4, 5}};
entered:
        if (i++ == 0) continue;
        total += a[0][0] + a[0][1] + a[1][0] + a[1][1];
        switch (i) { case 2: continue; default: break; }
        break;
    }
    printf("switch-array %d %d %d %d\n", i, tests, post, total);
}
int main(void) { unbraced_loop(); unbraced_if_loop(); switch_continue_and_arrays(); return 0; }
