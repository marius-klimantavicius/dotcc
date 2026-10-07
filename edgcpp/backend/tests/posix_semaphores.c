#define _POSIX_C_SOURCE 200809L
#include <semaphore.h>
#include <threads.h>
#include <time.h>
#include <errno.h>
#include <limits.h>
#include <stdio.h>
#include <string.h>

#define CHECK(test) do { if (!(test)) { printf("semaphore failure at line %d errno %d\n", __LINE__, errno); return __LINE__; } } while (0)

struct work { sem_t mutex, ready, start; int total; };

static int worker(void *argument) {
    struct work *state = argument;
    if (sem_post(&state->ready) || sem_wait(&state->start)) return 1;
    for (int i=0; i<1000; ++i) {
        if (sem_wait(&state->mutex)) return 2;
        ++state->total;
        if (sem_post(&state->mutex)) return 3;
    }
    return 0;
}

int main(void) {
    sem_t value;
    /* sem_init must accept arbitrary initial bytes, not require a zero token. */
    memset(&value, 0xa5, sizeof(value));
    CHECK(sem_init(&value, 0, 2) == 0);
    int count = -1;
    CHECK(sem_getvalue(&value, &count) == 0 && count == 2);
    CHECK(sem_wait(&value) == 0 && sem_trywait(&value) == 0);
    errno = 0;
    CHECK(sem_trywait(&value) == -1 && errno == EAGAIN);
    struct timespec deadline = {-1, 0};
    errno = 0;
    CHECK(sem_timedwait(&value, &deadline) == -1 && errno == ETIMEDOUT);
    deadline.tv_sec = 0; deadline.tv_nsec = 1000000000;
    errno = 0;
    CHECK(sem_timedwait(&value, &deadline) == -1 && errno == EINVAL);
    CHECK(sem_post(&value) == 0);
    /* An expired valid deadline still succeeds if a token is available. */
    deadline.tv_sec = -1; deadline.tv_nsec = 0;
    CHECK(sem_timedwait(&value, &deadline) == 0);
    struct timespec before, after;
    CHECK(clock_gettime(CLOCK_MONOTONIC, &before) == 0);
    CHECK(timespec_get(&deadline, TIME_UTC) == TIME_UTC);
    deadline.tv_nsec += 30000000;
    if (deadline.tv_nsec >= 1000000000) { ++deadline.tv_sec; deadline.tv_nsec -= 1000000000; }
    errno = 0;
    CHECK(sem_timedwait(&value, &deadline) == -1 && errno == ETIMEDOUT);
    CHECK(clock_gettime(CLOCK_MONOTONIC, &after) == 0);
    CHECK((after.tv_sec-before.tv_sec)*1000000000L + after.tv_nsec-before.tv_nsec >= 20000000L);
    CHECK(sem_destroy(&value) == 0);
    CHECK(sem_init(&value, 0, INT_MAX) == 0);
    errno = 0;
    CHECK(sem_post(&value) == -1 && errno == EOVERFLOW);
    CHECK(sem_destroy(&value) == 0);
    CHECK(sem_init(&value, 0, 0) == 0 && sem_post(&value) == 0 && sem_wait(&value) == 0);
    CHECK(sem_destroy(&value) == 0);

    struct work state;
    state.total = 0;
    CHECK(sem_init(&state.mutex, 0, 1) == 0);
    CHECK(sem_init(&state.ready, 0, 0) == 0);
    CHECK(sem_init(&state.start, 0, 0) == 0);
    thrd_t threads[4];
    for (int i=0; i<4; ++i) CHECK(thrd_create(&threads[i], worker, &state) == thrd_success);
    for (int i=0; i<4; ++i) CHECK(sem_wait(&state.ready) == 0);
    for (int i=0; i<4; ++i) CHECK(sem_post(&state.start) == 0);
    for (int i=0; i<4; ++i) { int result=-1; CHECK(thrd_join(threads[i], &result) == thrd_success && result == 0); }
    CHECK(state.total == 4000);
    CHECK(sem_getvalue(&state.mutex, &count) == 0 && count == 1);
    CHECK(sem_destroy(&state.mutex) == 0);
    CHECK(sem_destroy(&state.ready) == 0);
    CHECK(sem_destroy(&state.start) == 0);
    puts("POSIX semaphore counts, errors, deadlines, threads and reuse passed");
    return 0;
}
