#include <stdio.h>
#include <threads.h>

int actual = 31;
extern int alternate __attribute__((alias("actual")));
extern int chain __attribute__((alias("alternate")));
__thread int tls_actual = 17;
extern __thread int tls_alternate __attribute__((alias("tls_actual")));
extern __thread int tls_chain __attribute__((alias("tls_alternate")));

_Thread_local int standard_tls = 13;
extern _Thread_local int standard_view __attribute__((alias("standard_tls")));

static int worker(void *argument) {
    (void)argument;
    int *volatile a = &tls_actual;
    int *volatile b = &tls_alternate;
    int *volatile c = &tls_chain;
    if (a != b || b != c || tls_chain != 17) return 1;
    if (standard_view != 13) return 3;
    standard_view = 37;
    if (standard_tls != 37) return 4;
    tls_alternate = 29;
    return tls_actual == 29 && tls_chain == 29 ? 0 : 2;
}

int main(void) {
    int *volatile a = &actual;
    int *volatile b = &alternate;
    int *volatile c = &chain;
    if (a != b || b != c || chain != 31) return 1;
    alternate = 47;
    if (actual != 47 || chain != 47) return 2;
    chain = 59;
    if (actual != 59 || alternate != 59) return 3;
    tls_chain = 71;
    standard_view = 83;
    thrd_t thread;
    if (thrd_create(&thread, worker, 0) != thrd_success) return 4;
    int result;
    if (thrd_join(thread, &result) != thrd_success || result) return 5;
    if (tls_actual != 71 || tls_alternate != 71) return 6;
    if (standard_tls != 83) return 7;
    puts("variable aliases: initialized storage, chains and TLS passed");
    return 0;
}
