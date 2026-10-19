#define _POSIX_C_SOURCE 200809L
#include <unistd.h>
#include <errno.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <stddef.h>
#include <pthread.h>

static int checks, failures;
static int (*hostname_service)(char *, size_t) = gethostname;
static const char *expected;
static void check(int condition) { ++checks; if (!condition) ++failures; }
static void *worker(void *arg)
{
    char out[1028];
    int *result = (int *)arg;
    errno = ERANGE;
    *result = hostname_service(out, sizeof out) == 0 && strcmp(out, expected) == 0 && errno == ERANGE;
    return arg;
}
int main(void)
{
    char out[1028];
    const unsigned char sentinel = 0x5a;
    expected = getenv("DOTCC_CPYTHON_EXPECTED_HOSTNAME");
    if (!expected || !expected[0] || strlen(expected) >= 1023) return 24;
    size_t n = strlen(expected);
    check(sizeof(size_t) == 8 && sizeof(long) == 8);
    check(hostname_service == gethostname);
    for (int cycle = 0; cycle < 2; ++cycle) {
        memset(out, sentinel, sizeof out);
        errno = EDOM;
        check(hostname_service(out, 1023) == 0);
        check(strcmp(out, expected) == 0);
        check(out[n] == 0 && (unsigned char)out[n + 1] == sentinel);
        check(errno == EDOM);
        for (int i = 0; i < 3; ++i) {
            size_t cap = i == 0 ? 0 : i == 1 ? 1 : n;
            memset(out, sentinel, sizeof out);
            errno = EDOM;
            check(hostname_service(out, cap) == -1);
            check(errno == ENAMETOOLONG);
            check(memcmp(out, expected, cap) == 0);
            check((unsigned char)out[cap] == sentinel);
        }
        memset(out, sentinel, sizeof out);
        errno = EDOM;
        check(hostname_service(out, n + 1) == 0);
        check(strcmp(out, expected) == 0 && out[n] == 0);
        check((unsigned char)out[n + 1] == sentinel && errno == EDOM);
        volatile size_t wide = (size_t)2147483713ULL;
        check(hostname_service(out, wide) == 0);
        check(strcmp(out, expected) == 0);
    }
    pthread_t threads[2];
    int result[2] = {0, 0};
    void *returned[2] = {0, 0};
    errno = EDOM;
    for (int i = 0; i < 2; ++i) check(pthread_create(&threads[i], 0, worker, &result[i]) == 0);
    for (int i = 0; i < 2; ++i) {
        check(pthread_join(threads[i], &returned[i]) == 0);
        check(returned[i] == &result[i] && result[i] == 1);
    }
    check(errno == EDOM);
    printf("gethostname checks=%d failures=%d workers=2\n", checks, failures);
    return failures ? 25 : 0;
}
