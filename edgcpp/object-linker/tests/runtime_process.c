#include <stdio.h>
#include <stdlib.h>
#include <errno.h>

extern int __cxa_atexit(void (*)(void *), void *, void *);
#ifdef NATIVE_ORACLE
extern void *__dso_handle;
extern int __cxa_thread_atexit_impl(void (*)(void *), void *, void *);
#define register_thread(f, p) __cxa_thread_atexit_impl(f, p, &__dso_handle)
#else
extern int __cxa_thread_atexit(void (*)(void *), void *, void *);
#define register_thread(f, p) __cxa_thread_atexit(f, p, 0)
#endif

/* Hosted source TLS must reset even when no translated thread API is linked. */
static _Thread_local int host_tls = 17;
static int host_trace;
static void host_destructor(void *unused) {
    (void)unused;
    if (host_tls != 99 || errno != 41) { host_trace = -1; return; }
    host_tls = 77;
    host_trace = 1;
}
int host_tls_setup(void) {
    if (host_tls != 17) return 1;
    host_tls = 99;
    errno = 41;
    return register_thread(host_destructor, 0);
}
int host_tls_after_release(void) { return host_trace != 1 || host_tls != 17 || errno != 0; }

static FILE *output;
static int mode;
static void mark(int value) {
    fputc(value, stderr);
    if (output) fputc(value, output);
}
static void late(void) { mark('C'); }
static void first(void) { mark('A'); }
static void second(void) { mark('B'); if (atexit(late)) _Exit(99); }
static void repeated(void) { mark('R'); }
static void quick_first(void) { mark('x'); }
static void quick_late(void) { mark('z'); }
static void quick_second(void) { mark('y'); if (at_quick_exit(quick_late)) _Exit(98); }
static void destructor(void *argument) { mark(*(const char *)argument); }
static void thread_destructor(void *argument) {
    mark(*(const char *)argument);
    if (mode == 5 && register_thread(destructor, (void *)"U")) _Exit(97);
}

int main(void) {
    mode = getchar() - '0';
    if (mode < 0 || mode > 5) return 90;
    output = fopen("process.bin", "wb");
    if (!output || setvbuf(output, 0, _IOFBF, 8192)) return 91;
    int (*volatile register_normal)(void (*)(void)) = atexit;
    if (register_normal(first) || __cxa_atexit(destructor, (void *)"G", 0) ||
        register_normal(second) || at_quick_exit(quick_first) || at_quick_exit(quick_second)) return 92;
    if (register_thread(destructor, (void *)"X") || register_thread(thread_destructor, (void *)"T")) return 93;
    if (mode == 4) for (int i = 0; i != 40; ++i) if (atexit(repeated)) return 94;
    fputs("body:", output);
    if (mode == 1) { void (*volatile stop)(int) = exit; stop(18); }
    if (mode == 2) { void (*volatile stop)(int) = quick_exit; stop(19); }
    if (mode == 3) { void (*volatile stop)(int) = _Exit; stop(20); }
    return 17;
}
