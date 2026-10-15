/* Genuine application input: all bodies execute as generated C#.
   The provider protocol replaces only the selected host ABI/error boundary. */
#define _GNU_SOURCE 1
#include <errno.h>
#include <fcntl.h>
#include <pthread.h>
#include <stdint.h>
#include <stdlib.h>
#include <unistd.h>

static const char failure_path[] = "/dev/null/roslynobj-provider-error";
static int observations[24];
static int *(*errno_address)(void) = __errno_location;
static int (*process_id)(void) = getpid;
static uintptr_t worker_slot_address;
static int worker_initial, worker_success, worker_error;
static int nested_incoming, nested_success, outer_incoming, outer_after_nested;

int provider_errno_read(void) { return errno; }
void provider_errno_write(int value) { *errno_address() = value; }
int provider_errno_failure(void) { return open(failure_path, O_RDONLY); }
int provider_errno_success(void) {
    int valid = process_id() > 0;
    return valid ? errno : -10000;
}
int provider_errno_identity(void) { return errno_address() == __errno_location(); }

static int inner_compare(const void *left, const void *right) {
    nested_incoming = errno;
    errno = 777;
    nested_success = provider_errno_success();
    errno = 444;
    return (*(const int *)left > *(const int *)right) - (*(const int *)left < *(const int *)right);
}
static int outer_compare(const void *left, const void *right) {
    outer_incoming = errno;
    int nested[2] = {8, 3};
    errno = 777;
    qsort(nested, 2, sizeof(int), inner_compare);
    outer_after_nested = errno;
    errno = 333;
    return (*(const int *)left > *(const int *)right) - (*(const int *)left < *(const int *)right);
}
static void *worker(void *cookie) {
    worker_slot_address = (uintptr_t)errno_address();
    worker_initial = errno;
    errno = 654;
    worker_success = provider_errno_success();
    provider_errno_failure();
    worker_error = errno;
    return cookie;
}

int provider_observation_count(void) { return 24; }
int provider_observe(int selector) { return observations[selector]; }
int fixture(void) {
    int *main_slot = __errno_location();
    provider_errno_write(1201);
    observations[0] = provider_errno_read();
    observations[1] = provider_errno_success();
    observations[2] = provider_errno_failure();
    observations[3] = provider_errno_read();
    observations[4] = provider_errno_identity();
    int values[2] = {9, 2};
    errno = 555;
    qsort(values, 2, sizeof(int), outer_compare);
    observations[5] = outer_incoming;
    observations[6] = nested_incoming;
    observations[7] = nested_success;
    observations[8] = outer_after_nested;
    observations[9] = errno;
    observations[10] = values[0]; observations[11] = values[1];
    pthread_mutexattr_t attributes;
    pthread_mutex_t mutex;
    observations[12] = pthread_mutexattr_init(&attributes);
    observations[13] = pthread_mutexattr_settype(&attributes, PTHREAD_MUTEX_RECURSIVE);
    observations[14] = pthread_mutex_init(&mutex, &attributes);
    int first_lock = pthread_mutex_lock(&mutex);
    int second_lock = pthread_mutex_trylock(&mutex);
    observations[15] = first_lock; observations[16] = second_lock;
    int first_unlock = pthread_mutex_unlock(&mutex);
    int second_unlock = pthread_mutex_unlock(&mutex);
    observations[17] = first_unlock + second_unlock + pthread_mutex_destroy(&mutex) + pthread_mutexattr_destroy(&attributes);
    pthread_t thread;
    int token = 57;
    void *returned = 0;
    errno = 901;
    observations[18] = pthread_create(&thread, 0, worker, &token);
    observations[19] = pthread_join(thread, &returned);
    observations[20] = returned == &token && worker_slot_address != (uintptr_t)main_slot;
    observations[21] = worker_initial == 0 && worker_success == 654 && worker_error == ENOTDIR;
    observations[22] = errno;
    observations[23] = main_slot == __errno_location() && provider_errno_identity();
    int result = 0;
    for (int index = 0; index < 24; ++index) result += observations[index];
    return result;
}
