#include <unistd.h>
#include <stdlib.h>
#include <errno.h>
#include <stdio.h>
#include <pthread.h>

struct observation {
    int expected;
    int checks;
    int failures;
    pthread_t owner;
};

static void *observe(void *context)
{
    struct observation *result = context;
    int (*identity)(void) = getpid;
    errno = ERANGE;
    int first = identity();
    result->checks++;
    if (first <= 0) result->failures++;
    result->checks++;
    if (first != result->expected) result->failures++;
    result->checks++;
    if (pthread_equal(pthread_self(), result->owner)) result->failures++;
    for (int i = 0; i < 16; i++) {
        result->checks++;
        if (getpid() != first) result->failures++;
    }
    result->checks++;
    if (errno != ERANGE) result->failures++;
    return context;
}

/* The launcher execs this process with its own shell PID as the independent
   expected observation. Actual PID values differ between native/JIT/AOT runs. */
int main(void)
{
    const char *expected_text = getenv("DOTCC_CPYTHON_EXPECTED_PID");
    int expected = expected_text ? atoi(expected_text) : 0;
    int (*identity)(void) = getpid;
    int failures = 0;
    errno = EDOM;
    int first = getpid();
    if (expected <= 0) failures++;
    if (first != expected) failures++;
    if (identity() != expected) failures++;
    if (getpid() != first) failures++;
    if (errno != EDOM) failures++;
    int checks = 5;
    struct observation observations[2] = { { expected, 0, 0, pthread_self() },
                                         { expected, 0, 0, pthread_self() } };
    pthread_t threads[2];
    for (int i = 0; i < 2; i++) {
        checks++;
        if (pthread_create(&threads[i], NULL, observe, &observations[i])) return 26;
    }
    for (int i = 0; i < 2; i++) {
        void *returned = NULL;
        checks++;
        if (pthread_join(threads[i], &returned)) return 27;
        checks++;
        if (returned != &observations[i]) failures++;
        checks++;
        if (observations[i].checks != 20) failures++;
        checks += observations[i].checks;
        failures += observations[i].failures;
    }
    checks++;
    if (errno != EDOM) failures++;
    printf("getpid checks=%d failures=%d workers=2\n", checks, failures);
    return failures ? 25 : 0;
}
