#define _GNU_SOURCE
#include <pthread.h>
#include <sched.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <sys/syscall.h>
#include <unistd.h>

struct output {
  uint32_t before;
  unsigned value;
  uint32_t after;
};

static int check_cpu(void) {
  cpu_set_t allowed;
  if (sched_getaffinity(0, sizeof(allowed), &allowed)) return 10;
  struct output cpu = {0x12345678, ~0u, 0x87654321};
  struct output node = {0x12345678, ~0u, 0x87654321};
  unsigned char cache[128], original[128];
  memset(cache, 0x5a, sizeof(cache));
  memcpy(original, cache, sizeof(cache));
  if (syscall(SYS_getcpu, &cpu.value, &node.value, cache)) return 11;
  if (cpu.value >= CPU_SETSIZE || !CPU_ISSET(cpu.value, &allowed)) return 12;
  if (cpu.before != 0x12345678 || cpu.after != 0x87654321 ||
      node.before != 0x12345678 || node.after != 0x87654321 ||
      memcmp(cache, original, sizeof(cache))) return 13;
  if (sched_getcpu() != (int)cpu.value) return 14;
  cpu.value = node.value = ~0u;
  if (syscall(SYS_getcpu, &cpu.value, NULL, NULL) ||
      cpu.value >= CPU_SETSIZE || !CPU_ISSET(cpu.value, &allowed) ||
      node.value != ~0u) return 15;
  cpu.value = ~0u;
  if (syscall(SYS_getcpu, NULL, &node.value, NULL) ||
      node.value == ~0u || cpu.value != ~0u) return 16;
  if (syscall(SYS_getcpu, NULL, NULL, cache) ||
      memcmp(cache, original, sizeof(cache))) return 17;
  return 0;
}

static void *worker(void *unused) {
  (void)unused;
  return (void *)(intptr_t)check_cpu();
}

int main(void) {
  cpu_set_t allowed, selected;
  if (sched_getaffinity(0, sizeof(allowed), &allowed)) return 20;
  int first = 0;
  while (first < CPU_SETSIZE && !CPU_ISSET(first, &allowed)) ++first;
  if (first == CPU_SETSIZE) return 21;
  CPU_ZERO(&selected);
  CPU_SET(first, &selected);
  if (sched_setaffinity(0, sizeof(selected), &selected)) return 22;
  int result = check_cpu();
  if (result) return result;
  pthread_t thread;
  if (pthread_create(&thread, NULL, worker, NULL)) return 23;
  void *worker_result;
  if (pthread_join(thread, &worker_result)) return 24;
  if (worker_result) return (int)(intptr_t)worker_result;
  puts("guest-getcpu: main/worker affinity, libc, nullable outputs, width, cache: PASS");
  return 0;
}
