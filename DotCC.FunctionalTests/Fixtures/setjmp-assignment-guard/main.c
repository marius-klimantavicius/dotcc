#include <setjmp.h>
#include <stdio.h>
#include <stdlib.h>

static jmp_buf env;
static int calls, rounds, tail_hits;
static void *Select(void) { ++calls; return env; }
static void *SelectOldValue(int old) { if (old != 42) abort(); ++calls; return env; }
#ifdef DOTCC_TEST_GC
void DotccTestCollect(void);
#endif
static void Deep(int value) {
#ifdef DOTCC_TEST_GC
    DotccTestCollect();
#endif
    longjmp(env, value);
}

int main(void) {
    int rc;
    for (;;) {
        if (!(rc = setjmp(Select()))) {
            Deep(rounds ? 0 : 7);
        } else if (rc == 7) {
            ++rounds;
            continue;
        } else if (rc == 1) {
            break;
        } else abort();
    }
    printf("loop rounds=%d calls=%d rc=%d\n", rounds, calls, rc);
    if (!(rc = setjmp(env))) {
        /* The saved invocation remains active after its if statement. */
    } else if (rc != 9) abort();
    ++tail_hits;
    if (tail_hits != 3) Deep(9);
    printf("tail hits=%d rc=%d\n", tail_hits, rc);
    if ((rc = setjmp(env))) {
        printf("positive rc=%d\n", rc);
    } else Deep(4);
    rc = 42;
    if (!(rc = setjmp(SelectOldValue(rc)))) Deep(6);
    if (rc != 6) abort();
    rc = 42;
    rc = setjmp(SelectOldValue(rc));
    if (!rc) Deep(8);
    if (rc != 8) abort();
    printf("evaluation old=42 calls=%d rc=%d\n", calls, rc);
    return 0;
}
