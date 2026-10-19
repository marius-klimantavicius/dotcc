#include <assert.h>
#include <stdint.h>
#include <stddef.h>
#include <pthread.h>
#include "dotcc_cpython_host.h"

/* ABI/service probe only: real emitted C calls the actual managed static thunk.
 * This is neither CPython nor an alternate interpreter implementation. */
int dcp_probe_call(dcp_int64_callback callback, int64_t token, int64_t argument,
                   int64_t *result, char *error, size_t capacity)
{ return callback(token, argument, result, error, capacity); }

typedef struct {
    dcp_int64_callback callback;
    int64_t token, argument, result;
} dcp_probe_worker;

static void *dcp_probe_worker_entry(void *data)
{
    dcp_probe_worker *worker = data;
    char error[128] = {0};
    assert(worker->callback(worker->token, worker->argument, &worker->result, error, sizeof(error)) == 0);
    assert(error[0] == 0);
    return NULL;
}

int64_t dcp_probe_workers(dcp_int64_callback callback, int64_t token)
{
    pthread_t first, second;
    dcp_probe_worker one = {callback, token, 21, 0};
    dcp_probe_worker two = {callback, token, 22, 0};
    assert(pthread_create(&first, NULL, dcp_probe_worker_entry, &one) == 0);
    assert(pthread_create(&second, NULL, dcp_probe_worker_entry, &two) == 0);
    assert(pthread_join(first, NULL) == 0);
    assert(pthread_join(second, NULL) == 0);
    return one.result + two.result;
}
