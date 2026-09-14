#include <setjmp.h>
#include <stdio.h>
#include <stdlib.h>

static jmp_buf env, outer;
static int selected, normal, recovery, tail, nested;
static void *Select(void) { ++selected; return env; }
#ifdef DOTCC_TEST_GC
void DotccTestCollect(void);
#endif
static void Deep(jmp_buf target, int value) {
#ifdef DOTCC_TEST_GC
    DotccTestCollect();
#endif
    longjmp(target, value);
}
int main(void) {
    if (!setjmp(Select())) {
        ++normal;
        Deep(env, 0);
    } else {
        ++recovery;
        if (recovery != 3) Deep(env, 4);
    }
    if (normal != 1 || recovery != 3 || selected != 1) abort();
    printf("guard normal=%d recovery=%d selected=%d\n", normal, recovery, selected);
    if (!setjmp(outer)) {
        if (!setjmp(env)) Deep(outer, 9);
        else abort();
    } else ++nested;
    if (nested != 1) abort();
    puts("nested outer=1");
    if (!setjmp(env)) ++normal;
    else ++recovery;
    ++tail;
    if (tail == 1) Deep(env, 2);
    if (normal != 2 || recovery != 4 || tail != 2) abort();
    printf("tail normal=%d recovery=%d visits=%d\n", normal, recovery, tail);
    if (!setjmp(env)) Deep(env, 1);
    return 0;
}
