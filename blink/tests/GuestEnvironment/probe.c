/* Normal environment/state syscalls; no delivered signal or injected failure. */
#ifdef GUEST_ENVIRONMENT_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <stdio.h>
#include <string.h>
#include <signal.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/fds.h"
#include "blink/machine.h"
#include "blink/map.h"
#ifndef DISABLE_JIT
#error GuestEnvironment requires interpreter-only configuration
#endif
#ifndef NOLINEAR
#error GuestEnvironment requires actual nonlinear guest page tables
#endif
#define CODE 0x400000UL
#define DATA 0x600000UL
#define PAGE 4096UL
#define TIMESPEC (DATA + PAGE - 8)
#define RANDOM (DATA + 2 * PAGE - 16)
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static struct Machine *machine;
static int used, memory_owner, signal_owner, exit_owner, observed_signal, dispatch_error;
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code; observed_signal = 1;
}
static long Call(unsigned number, unsigned long a, unsigned long b, unsigned long c, unsigned long d) {
  Write64(machine->ax, number); Write64(machine->di, a);
  Write64(machine->si, b); Write64(machine->dx, c);
  Write64(machine->r10, d); Write64(machine->r8, 0); Write64(machine->r9, 0);
  machine->ip = CODE;
  int halt = sigsetjmp(machine->onhalt, 1);
  if (!halt) { machine->canhalt = true; ExecuteInstruction(machine); }
  machine->canhalt = false;
  if (halt || observed_signal || machine->ip != CODE + 2 || machine->signals ||
      machine->insyscall || machine->sysdepth || machine->freelist.n)
    dispatch_error = 1;
  return (long)Read64(machine->ax);
}
static int Timestamp(unsigned syscall, unsigned clock, long *seconds, long *nanoseconds) {
  unsigned char bytes[16];
  CHECK(Call(syscall, clock, TIMESPEC, 0, 0) == 0);
  CHECK(!CopyFromUserRead(machine, bytes, TIMESPEC, sizeof(bytes)));
  *seconds = (long)Read64(bytes); *nanoseconds = (long)Read64(bytes + 8);
  CHECK(*seconds >= 0 && *nanoseconds >= 0 && *nanoseconds < 1000000000L);
  return 0;
}
static int Clocks(unsigned cycle) {
  for (unsigned clock = 0; clock < 2; ++clock) {
    long first_sec, first_nsec, second_sec, second_nsec, res_sec, res_nsec;
    CHECK(!Timestamp(228, clock, &first_sec, &first_nsec));
    CHECK(!Timestamp(228, clock, &second_sec, &second_nsec));
    CHECK(!Timestamp(229, clock, &res_sec, &res_nsec));
    CHECK(res_sec || res_nsec);
    if (clock == 1) CHECK(second_sec > first_sec || (second_sec == first_sec && second_nsec >= first_nsec));
    printf("obs cycle=%u clock=%u first_sec=%ld first_nsec=%ld second_sec=%ld second_nsec=%ld res_sec=%ld res_nsec=%ld\n",
           cycle, clock, first_sec, first_nsec, second_sec, second_nsec, res_sec, res_nsec);
  }
  printf("state cycle=%u clocks=2 normalized=1 monotonic_nondecrease=1 resolution_positive=1\n", cycle);
  return 0;
}
static int Entropy(unsigned cycle) {
  unsigned char bytes[64];
  memset(bytes, 0xa5, sizeof(bytes));
  CHECK(!CopyToUserWrite(machine, RANDOM - 16, bytes, sizeof(bytes)));
  CHECK(Call(318, RANDOM, 32, 0, 0) == 32);
  CHECK(machine->writeaddr == RANDOM && machine->writesize == 32);
  CHECK(!CopyFromUserRead(machine, bytes, RANDOM - 16, sizeof(bytes)));
  for (unsigned i = 0; i < 16; ++i) CHECK(bytes[i] == 0xa5 && bytes[i + 48] == 0xa5);
  printf("obs cycle=%u random=", cycle);
  for (unsigned i = 16; i < 48; ++i) printf("%02x", bytes[i]);
  printf("\n");
  printf("state cycle=%u random_return=32 cross_page=1 canaries=1\n", cycle);
  return 0;
}
static int Tid(unsigned cycle) {
  unsigned char bytes[4];
  Write32(bytes, 0x3456789a);
  CHECK(!CopyToUserWrite(machine, DATA + 640, bytes, sizeof(bytes)));
  long tid = Call(218, DATA + 640, 0, 0, 0);
  CHECK(tid > 0 && tid == machine->tid && machine->ctid == DATA + 640);
  CHECK(!CopyFromUserRead(machine, bytes, DATA + 640, sizeof(bytes)));
  CHECK(Read32(bytes) == 0x3456789a);
  printf("obs cycle=%u tid=%ld ctid=%lx\n", cycle, tid, (unsigned long)machine->ctid);
  printf("state cycle=%u tid_matches=1 ctid_stored=1 word_unchanged=1\n", cycle);
  return 0;
}
static int Signals(unsigned cycle) {
  unsigned char action[32]; unsigned char output[32]; unsigned char word[8];
  struct sigaction host;
  unsigned long ignore_flags, default_flags;
  unsigned long initial = machine->sigmask;
  unsigned long bit = 1UL << 9; /* Linux SIGUSR1 */
  CHECK(!(machine->system->blinksigs & bit) && !(initial & bit) && !machine->signals);
  memset(action, 0, sizeof(action)); Write64(action, 1); /* SIG_IGN */
  CHECK(!CopyToUserWrite(machine, DATA + 128, action, sizeof(action)));
  CHECK(Call(13, 10, DATA + 128, DATA + 256, 8) == 0);
  CHECK(!CopyFromUserRead(machine, output, DATA + 256, sizeof(output)));
  for (unsigned i = 0; i < sizeof(output); ++i) CHECK(!output[i]);
  CHECK(!memcmp(&machine->system->hands[9], action, sizeof(action)));
  CHECK(Call(13, 10, 0, DATA + 384, 8) == 0);
  CHECK(!CopyFromUserRead(machine, output, DATA + 384, sizeof(output)));
  CHECK(!memcmp(action, output, sizeof(action)));
  /* This query detects a host registration failure that SysSigaction merely
   * logs. On native it queries libc; in managed it queries the private registry. */
  memset(&host, 0, sizeof(host));
  CHECK(!sigaction(SIGUSR1, 0, &host));
  CHECK(host.sa_handler == SIG_IGN && (host.sa_flags & SA_SIGINFO));
  ignore_flags = (unsigned int)host.sa_flags;
  memset(action, 0, sizeof(action)); /* SIG_DFL */
  CHECK(!CopyToUserWrite(machine, DATA + 128, action, sizeof(action)));
  CHECK(Call(13, 10, DATA + 128, DATA + 256, 8) == 0);
  CHECK(!CopyFromUserRead(machine, output, DATA + 256, sizeof(output)));
  CHECK(Read64(output) == 1);
  for (unsigned i = 8; i < sizeof(output); ++i) CHECK(!output[i]);
  CHECK(Call(13, 10, 0, DATA + 384, 8) == 0);
  CHECK(!CopyFromUserRead(machine, output, DATA + 384, sizeof(output)));
  CHECK(!memcmp(action, output, sizeof(action)) && !memcmp(&machine->system->hands[9], action, sizeof(action)));
  memset(&host, 0, sizeof(host));
  CHECK(!sigaction(SIGUSR1, 0, &host));
  CHECK(host.sa_handler == SIG_DFL && (host.sa_flags & SA_SIGINFO));
  default_flags = (unsigned int)host.sa_flags;

  Write64(word, bit); CHECK(!CopyToUserWrite(machine, DATA + 512, word, sizeof(word)));
  CHECK(Call(14, 0, DATA + 512, DATA + 528, 8) == 0); /* SIG_BLOCK */
  CHECK(!CopyFromUserRead(machine, word, DATA + 528, sizeof(word)) && Read64(word) == initial);
  CHECK(machine->sigmask == (initial | bit));
  CHECK(Call(14, 0, 0, DATA + 528, 8) == 0);
  CHECK(!CopyFromUserRead(machine, word, DATA + 528, sizeof(word)) && Read64(word) == (initial | bit));
  Write64(word, initial); CHECK(!CopyToUserWrite(machine, DATA + 512, word, sizeof(word)));
  CHECK(Call(14, 2, DATA + 512, DATA + 528, 8) == 0); /* SIG_SETMASK */
  CHECK(!CopyFromUserRead(machine, word, DATA + 528, sizeof(word)) && Read64(word) == (initial | bit));
  CHECK(machine->sigmask == initial && !machine->signals && !observed_signal);
  printf("obs cycle=%u initial_mask=%lx host_ignore_flags=%lx host_default_flags=%lx\n", cycle, initial, ignore_flags, default_flags);
  printf("state cycle=%u disposition_restored=1 guest_mask_restored=1 host_registry=1 pending=0 delivered=0\n", cycle);
  return 0;
}
static int Cycle(unsigned cycle) {
  unsigned char code[2] = {0x0f, 0x05};
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  CHECK(s != 0);
#ifdef GUEST_ENVIRONMENT_MANAGED
  if (BlinkHostInitializeBoundResourceLimits(s)) { FreeSystem(s); return __LINE__; }
#endif
  machine = NewMachine(s, 0);
  if (!machine) { FreeSystem(s); return __LINE__; }
  g_machine = machine;
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  s->cr3 = AllocatePageTable(s); CHECK(s->cr3);
  CHECK(ReserveVirtual(s, CODE, PAGE, PAGE_U | PAGE_RW, -1, 0, false, false) == CODE);
  CHECK(ReserveVirtual(s, DATA, 4 * PAGE, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == DATA);
  CHECK(!CopyToUserWrite(machine, CODE, code, sizeof(code)));
  CHECK(!Clocks(cycle) && !Entropy(cycle) && !Tid(cycle) && !Signals(cycle));
  CHECK(!dispatch_error && !machine->freelist.n && !CountFds(&s->fds));
  CHECK(!FreeVirtual(s, CODE, PAGE) && !FreeVirtual(s, DATA, 4 * PAGE) && !s->vss);
  printf("state cycle=%u cleanup pages=0 guest_fds=0 syscall_cleanup=1\n", cycle);
  struct Machine *released = machine; machine = 0;
  FreeMachine(released); g_machine = 0;
  return 0;
}
void GuestEnvironmentDestroy(void) {
  if (machine) { struct Machine *released = machine; machine = 0; FreeMachine(released); g_machine = 0; }
}
int GuestEnvironmentRelease(void) {
#ifdef GUEST_ENVIRONMENT_MANAGED
  if (exit_owner) { exit_owner = 0; BlinkHostExitCallbacksEnd(); }
  if (signal_owner) { signal_owner = 0; BlinkHostSignalActionsEnd(); }
  if (memory_owner) { memory_owner = 0; BlinkHostMemoryDisposeWorker(); }
#endif
  return 0;
}
int GuestEnvironmentRun(void) {
  CHECK(!used); used = 1;
#ifdef GUEST_ENVIRONMENT_MANAGED
  CHECK(!BlinkHostMemoryBegin(64 * 1024 * 1024)); memory_owner = 1;
  CHECK(!BlinkHostSignalActionsBegin()); signal_owner = 1;
  CHECK(!BlinkHostExitCallbacksBegin()); exit_owner = 1;
  CHECK(!BlinkHostMemoryEnablePrivateFiles());
#endif
  InitMap(); InitBus();
#ifdef GUEST_ENVIRONMENT_MANAGED
  size_t retained_mappings = 0;
  size_t retained_bytes = 0;
#endif
  for (unsigned cycle = 0; cycle < 2; ++cycle) {
    int result = Cycle(cycle); if (result) return result;
#ifdef GUEST_ENVIRONMENT_MANAGED
    size_t mappings = BlinkHostMemoryMappings(); size_t bytes = BlinkHostMemoryBytes();
    CHECK(mappings && bytes && bytes <= BlinkHostMemoryLimit());
    if (!cycle) { retained_mappings = mappings; retained_bytes = bytes; }
    else CHECK(mappings == retained_mappings && bytes == retained_bytes);
#endif
  }
#ifdef GUEST_ENVIRONMENT_MANAGED
  CHECK(!BlinkHostExitCallbacksRun());
#endif
  return 0;
}
#ifndef GUEST_ENVIRONMENT_MANAGED
int main(void) {
  int result = GuestEnvironmentRun(); GuestEnvironmentDestroy();
  int cleanup = GuestEnvironmentRelease();
  return result ? result : cleanup;
}
#endif
