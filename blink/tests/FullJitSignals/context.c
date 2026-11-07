/* Focused Linux x86-64 #UD/#XM/#GP recovery. No libc, allocator, or malformed memory. */
#define _GNU_SOURCE 1
#include <stddef.h>
#include <stdint.h>
#include <signal.h>
#include <ucontext.h>

struct kernel_action {
  void (*handler)(int, siginfo_t *, void *);
  unsigned long flags;
  void (*restorer)(void);
  unsigned long mask;
};
_Static_assert(offsetof(siginfo_t, si_code) == 8, "Linux x64 si_code");
_Static_assert(offsetof(siginfo_t, si_addr) == 16, "Linux x64 si_addr");
_Static_assert(offsetof(ucontext_t, uc_mcontext.gregs[REG_RAX]) == 144, "RAX");
_Static_assert(offsetof(ucontext_t, uc_mcontext.gregs[REG_RIP]) == 168, "RIP");
_Static_assert(offsetof(ucontext_t, uc_mcontext.gregs[REG_TRAPNO]) == 200, "trap");
_Static_assert(offsetof(ucontext_t, uc_sigmask) == 296, "sigmask");

extern long raw_syscall4(long, long, long, long, long);
extern unsigned long try_instruction(void);
extern void restorer(void), fault_site(void), fallback_site(void);
extern unsigned long saved_sp, returned_flags;
extern unsigned int returned_mxcsr;
static volatile unsigned caught;
static volatile unsigned outer_returned;
#if defined(PROBE_GP)
#define PROBE_SIGNAL SIGSEGV
#define PROBE_CODE SI_KERNEL
#define PROBE_TRAP 13
#define SAVED_MXCSR 0x1f80
#elif defined(PROBE_SIMD)
#define PROBE_SIGNAL SIGFPE
#define PROBE_CODE FPE_FLTDIV
#define PROBE_TRAP 19
#define SAVED_MXCSR 0x1d84
#else
#define PROBE_SIGNAL SIGILL
#define PROBE_CODE ILL_ILLOPN
#define PROBE_TRAP 6
#define SAVED_MXCSR 0x1f80
#endif

static __attribute__((noreturn)) void finish(int code) {
  raw_syscall4(231, code, 0, 0, 0);
  for (;;) { }
}
#define CHECK(condition, code) do { if (!(condition)) finish(code); } while (0)

static void handler(int signal, siginfo_t *info, void *pointer) {
  ucontext_t *context = pointer;
  greg_t *g = context->uc_mcontext.gregs;
  unsigned long mask = 0;
  CHECK(signal == PROBE_SIGNAL && info->si_signo == PROBE_SIGNAL, 31);
  CHECK(info->si_errno == 0 && info->si_code == PROBE_CODE, 32);
#ifdef PROBE_GP
  CHECK((uintptr_t)info->si_addr == 0, 33);
#else
  CHECK((uintptr_t)info->si_addr == (uintptr_t)fault_site, 33);
#endif
  CHECK((uintptr_t)g[REG_RIP] == (uintptr_t)fault_site, 34);
  CHECK(g[REG_TRAPNO] == PROBE_TRAP && g[REG_ERR] == 0, 35);
  CHECK((unsigned long)g[REG_RAX] == 0x1122334455667788UL, 36);
  CHECK((unsigned long)g[REG_R12] == 0x8877665544332211UL, 37);
  CHECK((unsigned long)g[REG_RSP] == saved_sp, 38);
  /* XOR set ZF/PF; STC then set CF, without changing parity. */
  CHECK(((unsigned long)g[REG_EFL] & 0x8c5UL) == 0x45UL, 39);
  CHECK(((unsigned long)g[REG_EFL] & 0xffc08028UL) == 0, 40);
  CHECK((g[REG_CSGSFS] & 3) == 3, 41);
  CHECK(context->uc_mcontext.fpregs != 0, 42);
  CHECK(context->uc_mcontext.fpregs->mxcsr == SAVED_MXCSR, 43);
#ifdef PROBE_GP
  CHECK(context->uc_mcontext.fpregs->_xmm[0].element[0] == 0x11223344 &&
        context->uc_mcontext.fpregs->_xmm[0].element[1] == 0x55667788 &&
        context->uc_mcontext.fpregs->_xmm[0].element[2] == 0x99aabbcc &&
        context->uc_mcontext.fpregs->_xmm[0].element[3] == 0xddeeff00, 46);
#elif defined(PROBE_SIMD)
  CHECK(context->uc_mcontext.fpregs->_xmm[0].element[0] == 0x3f800000 &&
        context->uc_mcontext.fpregs->_xmm[1].element[0] == 0, 46);
#endif
  unsigned long probe_mask = 1UL << (PROBE_SIGNAL - 1);
  CHECK(raw_syscall4(14, 2, 0, (long)&mask, 8) == 0 && (mask & probe_mask), 44);
  CHECK((*(unsigned long *)&context->uc_sigmask & probe_mask) == 0, 45);
  /* The fallback is guest code, entered through real rt_sigreturn. */
  g[REG_RIP] = (greg_t)(uintptr_t)fallback_site;
  g[REG_RAX] = 0x13579bdf;
  g[REG_EFL] &= ~(greg_t)0x45;  /* clear CF, PF and ZF in saved context */
  /* Change live handler FP state; sigreturn must restore the saved state. */
  unsigned int handler_mxcsr = 0x3f80;
  __asm__ volatile("ldmxcsr %0" : : "m"(handler_mxcsr));
  ++caught;
}

static void exercise(unsigned long expected_mask) {
  for (unsigned round = 1; round <= 2; ++round) {
    CHECK(try_instruction() == 0x13579bdfUL, 22);
    CHECK(caught == round, 23);
    CHECK((returned_flags & 0x45UL) == 0, 24);
    CHECK(returned_mxcsr == SAVED_MXCSR, 25);
    unsigned long mask = ~0UL;
    CHECK(raw_syscall4(14, 2, 0, (long)&mask, 8) == 0 && mask == expected_mask, 26);
  }
}

#ifdef PROBE_RECURSIVE
static void outer_handler(int signal, siginfo_t *info, void *context) {
  (void)context;
  CHECK(signal == SIGUSR1 && info->si_code == -6, 51);
  exercise(1UL << (SIGUSR1-1));
  outer_returned = 1;
}
#endif

int guest_main(void) {
  struct kernel_action action = {handler, 0x04000004UL, restorer, 0};
  CHECK(raw_syscall4(13, PROBE_SIGNAL, (long)&action, 0, 8) == 0, 21);
#ifdef PROBE_RECURSIVE
  action.handler = outer_handler;
  CHECK(raw_syscall4(13, SIGUSR1, (long)&action, 0, 8) == 0, 52);
  unsigned long mask = 1UL << (SIGUSR1-1);
  CHECK(raw_syscall4(14, 0, (long)&mask, 0, 8) == 0, 53);
  long tid = raw_syscall4(186, 0, 0, 0, 0);
  CHECK(tid > 0 && raw_syscall4(200, tid, SIGUSR1, 0, 0) == 0, 54);
  CHECK(outer_returned == 0, 55);
  /* Deterministically enters Blink's SysSigprocmask -> SignalActor path. */
  CHECK(raw_syscall4(14, 1, (long)&mask, 0, 8) == 0, 56);
  CHECK(outer_returned == 1 && caught == 2, 57);
  CHECK(raw_syscall4(14, 2, 0, (long)&mask, 8) == 0 && mask == 0, 58);
#else
  (void)outer_returned;
  exercise(0);
#endif
#if defined(PROBE_GP) && defined(PROBE_RECURSIVE)
  static const char message[] = "full-jit-signals: recursive=1 caught=2 code=128 trap=13 context=restored\n";
#elif defined(PROBE_GP)
  static const char message[] = "full-jit-signals: caught=2 code=128 trap=13 ip=edited registers=restored flags=restored mxcsr=restored mask=restored\n";
#elif defined(PROBE_SIMD) && defined(PROBE_RECURSIVE)
  static const char message[] = "full-jit-signals: recursive=1 caught=2 code=3 trap=19 context=restored\n";
#elif defined(PROBE_SIMD)
  static const char message[] = "full-jit-signals: caught=2 code=3 trap=19 ip=edited registers=restored flags=restored mxcsr=restored mask=restored\n";
#elif defined(PROBE_RECURSIVE)
  static const char message[] = "full-jit-signals: recursive=1 caught=2 code=2 trap=6 context=restored\n";
#else
  static const char message[] = "full-jit-signals: caught=2 code=2 trap=6 ip=edited registers=restored flags=restored mxcsr=restored mask=restored\n";
#endif
  CHECK(raw_syscall4(1, 1, (long)message, sizeof(message)-1, 0) == (long)sizeof(message)-1, 27);
  return 0;
}
