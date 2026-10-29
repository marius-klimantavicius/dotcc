/* Authoritative test-service C ABI; no upstream terminal algorithms. */
#ifndef DOTCC_UPSTREAM_TEST_HOST_H
#define DOTCC_UPSTREAM_TEST_HOST_H
#include <stddef.h>
#include <stdint.h>
void *dotcc_upstream_host_alloc(size_t len, uint8_t alignment);
void dotcc_upstream_host_free(void *ptr, size_t len, uint8_t alignment);
size_t dotcc_upstream_host_cpu_count(void);
void dotcc_upstream_host_report(const uint8_t *name, size_t len, int status,
                               const uint8_t *error, size_t error_len);
void dotcc_upstream_host_log(const uint8_t *bytes, size_t len);
int dotcc_upstream_host_finish(size_t count);
size_t dotcc_upstream_count(void);
int dotcc_upstream_run(size_t index);
const uint8_t *dotcc_upstream_name(size_t index, size_t *len);
#endif
