#define _GNU_SOURCE
#include <signal.h>
#include <stdint.h>
#include <sys/syscall.h>
#include <sys/mman.h>
#include <unistd.h>

/* Guest fixture only: AUTHORED_UNBUILT_UNRUN. No timing establishes order.
   tgkill(self) enters the registered outer handler; that handler synchronously
   tgkill(self,USR2) while its C syscall frame is paused, forcing nested Actor.
   alarm bounds failure. The managed native backend must NOT require host CX16. */
typedef struct { uint64_t lo, hi; } Pair;
_Static_assert(sizeof(Pair) == 16, "pair width");
static volatile Pair cell __attribute__((aligned(16)));
static volatile uint64_t count;
static volatile sig_atomic_t phase;
static volatile unsigned char *crossing;

static void nested(int signal) {
  uint64_t increment = 1;
  if (signal != SIGUSR2 || phase != 1) _exit(71);
  __asm__ volatile("lock xaddq %0,%1" : "+r"(increment), "+m"(count) : : "cc", "memory");
  if (increment != 0 || count != 1) _exit(72);
  uint32_t lo = 0, hi = 0;
  unsigned char success;
  __asm__ volatile("lock cmpxchg8b %1; sete %0"
    : "=qm"(success), "+m"(*(volatile uint64_t *)crossing), "+a"(lo), "+d"(hi)
    : "b"(UINT32_C(0x89abcdef)), "c"(UINT32_C(0xc3d2e1f0)) : "cc", "memory");
  if (!success || lo != 0 || hi != 0) _exit(80);
  phase = 2;
}
static void outer(int signal) {
  uint64_t lo = 0, hi = 0;
  unsigned char success;
  if (signal != SIGUSR1 || phase != 0) _exit(73);
  __asm__ volatile("lock cmpxchg16b %1; sete %0"
    : "=qm"(success), "+m"(cell), "+a"(lo), "+d"(hi)
    : "b"(UINT64_C(0x1122334455667788)), "c"(UINT64_C(0x99aabbccddeeff00))
    : "cc", "memory");
  if (!success || lo != 0 || hi != 0) _exit(74);
  phase = 1;
  if (syscall(SYS_tgkill, getpid(), syscall(SYS_gettid), SIGUSR2) != 0) _exit(75);
  if (phase != 2 || count != 1) _exit(76);
  phase = 3;
}
int main(void) {
  struct sigaction first = {0}, second = {0};
  void *pages = mmap(0, 8192, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  if (pages == MAP_FAILED) return 81;
  crossing = (volatile unsigned char *)pages + 4095;
  first.sa_handler = outer; second.sa_handler = nested;
  first.sa_flags = second.sa_flags = SA_RESTART;
  sigemptyset(&first.sa_mask); sigemptyset(&second.sa_mask);
  if (sigaction(SIGUSR1, &first, 0) || sigaction(SIGUSR2, &second, 0)) return 77;
  alarm(8);
  if (syscall(SYS_tgkill, getpid(), syscall(SYS_gettid), SIGUSR1) != 0) return 78;
  if (phase != 3 || count != 1 || cell.lo != UINT64_C(0x1122334455667788) ||
      cell.hi != UINT64_C(0x99aabbccddeeff00)) return 79;
  uint64_t observed = 0;
  for (int i = 0; i < 8; ++i) observed |= (uint64_t)crossing[i] << (8 * i);
  if (observed != UINT64_C(0xc3d2e1f089abcdef)) return 82;
  if (munmap(pages, 8192)) return 83;
  alarm(0);
  return 0;
}
