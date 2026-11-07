#define _GNU_SOURCE 1
#include <stdint.h>
#include <stddef.h>
#include <signal.h>
#include <ucontext.h>

static long sys6(long n, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d;
  register long r8 __asm__("r8") = e;
  register long r9 __asm__("r9") = f;
  __asm__ volatile("syscall" : "+a"(n) : "D"(a), "S"(b), "d"(c), "r"(r10), "r"(r8), "r"(r9) : "rcx", "r11", "memory");
  return n;
}
static __attribute__((noreturn)) void done(int code) { sys6(231, code, 0, 0, 0, 0, 0); for (;;) {} }
#define CHECK(x, c) do { if (!(x)) done(c); } while (0)
struct action { void (*handler)(int, siginfo_t *, void *); unsigned long flags; void (*restorer)(void); unsigned long mask; };
extern void memory_restorer(void), faulting_store(void *), store_fault_site(void), store_resume(void), recovery_ret(void);
static volatile int stage, caught;
static uintptr_t expected_ip, expected_address, resume_ip;

static void handler(int sig, siginfo_t *info, void *p) {
  ucontext_t *uc = p;
  CHECK(sig == SIGSEGV && info->si_signo == sig, 30);
  CHECK(info->si_code == SEGV_ACCERR, 31);
  CHECK((uintptr_t)info->si_addr == expected_address, 32);
  CHECK((uintptr_t)uc->uc_mcontext.gregs[REG_RIP] == expected_ip, 33);
  uc->uc_mcontext.gregs[REG_RIP] = (greg_t)resume_ip;
  ++caught;
}
int guest_main(void) {
  struct action a = {handler, 0x04000004, memory_restorer, 0};
  CHECK(sys6(13, SIGSEGV, (long)&a, 0, 8, 0, 0) == 0, 10);
  unsigned char *memory = (void *)sys6(9, 0, 8192, 3, 0x22, -1, 0);
  CHECK((uintptr_t)memory < (uintptr_t)-4095L, 11);
  for (int i = 0; i < 4096; ++i) memory[i] = 0x5a;
  CHECK(sys6(10, (long)memory + 4096, 4096, 0, 0, 0, 0) == 0, 12);
  stage = 1; expected_ip = (uintptr_t)store_fault_site; expected_address = (uintptr_t)memory + 4096; resume_ip = (uintptr_t)store_resume;
  faulting_store(memory + 4092);
  CHECK(caught == 1, 13);
  for (int i = 0; i < 4096; ++i) CHECK(memory[i] == 0x5a, 14); // no abandoned-stash prefix write
  CHECK(sys6(11, (long)memory, 8192, 0, 0, 0, 0) == 0, 15);

  unsigned char *code = (void *)sys6(9, 0, 8192, 3, 0x22, -1, 0);
  CHECK((uintptr_t)code < (uintptr_t)-4095L, 16);
  code[4095] = 0xc3; // valid RET requires only the first page
  CHECK(sys6(10, (long)code + 4096, 4096, 0, 0, 0, 0) == 0, 17);
  CHECK(sys6(10, (long)code, 4096, 5, 0, 0, 0) == 0, 18);
  ((void (*)(void))(code + 4095))(); CHECK(caught == 1, 19);
  CHECK(sys6(10, (long)code, 4096, 3, 0, 0, 0) == 0, 20);
  code[4095] = 0x0f; // incomplete escape requires inaccessible next page
  CHECK(sys6(10, (long)code, 4096, 5, 0, 0, 0) == 0, 21);
  stage = 2; expected_ip = (uintptr_t)code + 4095; expected_address = (uintptr_t)code + 4096; resume_ip = (uintptr_t)recovery_ret;
  ((void (*)(void))(code + 4095))(); CHECK(caught == 2, 22);

  // Queue while blocked; unmasking runs the first handler instruction through
  // recursive SignalActor inside rt_sigprocmask, carrying an outer SYSCALL.
  a.handler = (void *)(code + 4095);
  CHECK(sys6(13, SIGUSR1, (long)&a, 0, 8, 0, 0) == 0, 23);
  unsigned long mask = 1UL << (SIGUSR1 - 1);
  CHECK(sys6(14, 0, (long)&mask, 0, 8, 0, 0) == 0, 24);
  long tid = sys6(186, 0, 0, 0, 0, 0, 0);
  CHECK(tid > 0 && sys6(200, tid, SIGUSR1, 0, 0, 0, 0) == 0, 25);
  stage = 3;
  CHECK(sys6(14, 1, (long)&mask, 0, 8, 0, 0) == 0, 26);
  CHECK(caught == 3, 27);
  static const char message[] = "memory-recovery: stash, boundary fetch and recursive first fetch ok\n";
  CHECK(sys6(1, 1, (long)message, sizeof(message)-1, 0, 0, 0) == sizeof(message)-1, 28);
  return 0;
}
