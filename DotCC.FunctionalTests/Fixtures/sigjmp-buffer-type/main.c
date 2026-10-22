#define _POSIX_C_SOURCE 200809L
#include <setjmp.h>
#include <stdio.h>

/* Actual PGlite declaration shape: signal-jump storage is global/volatile.
   This tests the storage type with existing lexical setjmp/longjmp only;
   it makes no assertion about saving/restoring a process signal mask. */
volatile sigjmp_buf postgresmain_sigjmp_buf;
typedef unsigned long cells[2];
volatile cells values;
int main(void) {
    int result = setjmp(postgresmain_sigjmp_buf);
    if (result == 0) longjmp(postgresmain_sigjmp_buf, 7);
    printf("caught=%d\n", result);
    values[0] = 19;
    values[1] = 23;
    unsigned long *borrow = (unsigned long *)values;
    printf("array=%lu\n", values[0] + borrow[1]);
    return result == 7 ? 0 : 1;
}
