#define _GNU_SOURCE
#include <errno.h>
#include <linux/futex.h>
#include <pthread.h>
#include <sched.h>
#include <stdatomic.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <sys/syscall.h>
#include <time.h>
#include <unistd.h>

/* Authored R74 regression input, UNBUILT/UNRUN. Parent owns compilation and
 * execution. Mirrors the real A4 WAIT_BITSET_PRIVATE_REALTIME/MOV64 same-page
 * failure. The delay encourages enqueue; parent scheduling instrumentation must
 * confirm enqueue before stores for a deterministic concurrency qualification.
 * This fixture does not claim to cover the separate full-pipe/CAS schedule. */
struct SharedPage {
  _Atomic uint32_t wait;
  unsigned char padding[0x240 - sizeof(uint32_t)];
  volatile uint64_t progress;
};
_Static_assert(offsetof(struct SharedPage, progress) == 0x240, "same-page store");
_Alignas(4096) static struct SharedPage shared;
_Alignas(4096) static _Atomic uint32_t ready;

static void *Waiter(void *unused) {
  struct timespec deadline;
  (void)unused;
  if (clock_gettime(CLOCK_REALTIME, &deadline)) return (void *)(uintptr_t)errno;
  deadline.tv_sec += 10;
  atomic_store_explicit(&ready, 1, memory_order_release);
  int rc = syscall(SYS_futex, &shared.wait,
      FUTEX_WAIT_BITSET | FUTEX_PRIVATE_FLAG | FUTEX_CLOCK_REALTIME, 0,
      &deadline, 0, FUTEX_BITSET_MATCH_ANY);
  return (void *)(uintptr_t)(rc == 0 || errno == EAGAIN ? 0 : errno);
}

__attribute__((noinline)) static void StoreProgress(uint64_t value) {
  shared.progress = value; /* Ordinary native MOV64, not a guest atomic opcode. */
}

int main(void) {
  pthread_t worker;
  struct timespec start, finish, delay = {0, 20000000};
  void *status = 0;
  int error = pthread_create(&worker, 0, Waiter, 0);
  if (error) return 10;
  while (!atomic_load_explicit(&ready, memory_order_acquire)) sched_yield();
  nanosleep(&delay, 0);
  if (clock_gettime(CLOCK_MONOTONIC, &start)) return 11;
  for (uint64_t i = 1; i <= 10000; ++i) StoreProgress(i);
  atomic_store_explicit(&shared.wait, 1, memory_order_release);
  syscall(SYS_futex, &shared.wait, FUTEX_WAKE | FUTEX_PRIVATE_FLAG, 1, 0, 0, 0);
  if (pthread_join(worker, &status)) return 12;
  if (clock_gettime(CLOCK_MONOTONIC, &finish)) return 13;
  double seconds = finish.tv_sec - start.tv_sec + (finish.tv_nsec - start.tv_nsec) / 1e9;
  if (status || shared.progress != 10000 || seconds >= 5.0) {
    fprintf(stderr, "futex-copy-progress failed: status=%lu value=%lu seconds=%.3f\n",
        (unsigned long)(uintptr_t)status, (unsigned long)shared.progress, seconds);
    return 14;
  }
  puts("futex-copy-progress passed");
  return 0;
}
