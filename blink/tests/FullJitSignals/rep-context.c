#define _GNU_SOURCE
#include <signal.h>
#include <stdint.h>
#include <stdio.h>
#include <sys/mman.h>
#include <ucontext.h>
#include <unistd.h>

/* SOURCE-AUTHORED / UNRUN. Linux x64 guest on either host OS/runtime.
 * Parent links rep-context.S and integrates the existing FullJitSignals runner.
 * Real registered PF handler + recursive REP + edited sigreturn context.
 * Native control/Full architectural test. Selected native8 Interpreter keeps
 * last comparison CF on fault (R94-S6); retain that expected gap separately.
 */
struct rep_result { uint64_t count, source, destination, flags; };
extern void rep_context_compare(const void *, const void *, uint64_t, struct rep_result *);
extern void rep_handler_copy(const void *, void *, uint64_t);
extern const unsigned char rep_context_site[];
static volatile sig_atomic_t calls, bad;
static unsigned char *source, *destination;
static unsigned char alternate_source[2] = {7, 5};
static unsigned char alternate_destination[2] = {9, 5};
static unsigned char handler_copy[2];

static void handler(int signal, siginfo_t *info, void *pointer) {
    ucontext_t *context = pointer;
    ++calls;
    if (signal != SIGSEGV || info->si_addr != source + 4096 ||
        context->uc_mcontext.gregs[REG_RIP] != (greg_t)rep_context_site ||
        context->uc_mcontext.gregs[REG_RCX] != 1 ||
        context->uc_mcontext.gregs[REG_RSI] != (greg_t)(source + 4096) ||
        context->uc_mcontext.gregs[REG_RDI] != (greg_t)(destination + 2) ||
        !(context->uc_mcontext.gregs[REG_EFL] & 1) || /* ORIGINAL CF=1, last equal CF=0 */
        !(context->uc_mcontext.gregs[REG_EFL] & 64)) bad = 1;
    if (calls != 1) { bad = 1; _exit(90); }
    /* Nested guest work must receive a new scope state after the old faulted
       instruction's latch/capability have been released. */
    rep_handler_copy(alternate_source, handler_copy, 2);
    context->uc_mcontext.gregs[REG_RSI] = (greg_t)(alternate_source + 1);
    context->uc_mcontext.gregs[REG_RDI] = (greg_t)(alternate_destination + 1);
    context->uc_mcontext.gregs[REG_RCX] = 2;
    context->uc_mcontext.gregs[REG_EFL] &= ~(greg_t)(256 | 65536); /* no TF/RF */
    context->uc_mcontext.gregs[REG_EFL] |= 1024; /* a meaningful handler flag edit: reverse REP */
}

int main(void) {
    alarm(5);
    source = mmap(0, 8192, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    destination = mmap(0, 4096, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    if (source == MAP_FAILED || destination == MAP_FAILED) return 1;
    source[4094] = source[4095] = 5;
    destination[0] = destination[1] = destination[2] = 5;
    if (mprotect(source + 4096, 4096, PROT_NONE)) return 2;
    struct sigaction action = {0}; action.sa_sigaction = handler; action.sa_flags = SA_SIGINFO;
    sigemptyset(&action.sa_mask);
    if (sigaction(SIGSEGV, &action, 0)) return 3;
    struct rep_result result = {0};
    rep_context_compare(source + 4094, destination, 3, &result);
    /* Edited reverse two-element restart compares 5-5 then7-9: CF1/ZF0/DF1.
       A stale one-element latch would instead finish with ZF1/old indices. */
    int failed = bad || calls != 1 || handler_copy[0] != 7 || handler_copy[1] != 5 ||
        result.count != 0 || result.source != (uintptr_t)alternate_source - 1 ||
        result.destination != (uintptr_t)alternate_destination - 1 ||
        !(result.flags & 1) || (result.flags & 64) || !(result.flags & 1024);
    printf("rep-context calls=%d bad=%d count=%llu flags=%llx pass=%d\n",
        calls, bad, (unsigned long long)result.count, (unsigned long long)result.flags, !failed);
    munmap(source, 8192); munmap(destination, 4096);
    return failed ? 4 : 0;
}
