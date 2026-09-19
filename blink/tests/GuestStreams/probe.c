/* Normal inherited guest streams. Diagnostic text never uses guest fd1/fd2. */
#ifdef GUEST_STREAMS_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <stdio.h>
#include <string.h>
#include <fcntl.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/fds.h"
#include "blink/machine.h"
#include "blink/map.h"
#include "blink/syscall.h"
#ifndef DISABLE_JIT
#error GuestStreams requires interpreter-only configuration
#endif
#ifndef NOLINEAR
#error GuestStreams requires actual nonlinear guest page tables
#endif
#define CODE 0x400000UL
#define DATA 0x600000UL
#define PAGE 4096UL
#define BUFFER (DATA + PAGE - 8)
#define IOV (DATA + 2 * PAGE - 8)
#define WINSIZE (DATA + 3 * PAGE - 4)
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static struct Machine *machine;
static int used, memory_owner, signal_owner, exit_owner, observed_signal, dispatch_error;
static unsigned completed_cycles, input_bytes, output_bytes, error_bytes;
static unsigned inherited_fds, metadata_cleanups, ioctl_refusals, surviving_fds;
static char report[512];
static unsigned char input[] = {0x00,0x41,0x7f,0x80,0xff,0x0a,0x42,0x0d,0x00,0x5a,0x31,0x32,0x33,0x34,0x35,0x36,0x37};
static unsigned char prefix[] = {0x4f,0x55,0x54,0x3a,0x00,0x3e};
static unsigned char error_output[] = {0x45,0x52,0x52,0x3a,0xff,0x00,0x0a};
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code; observed_signal = 1;
}
static long Call(unsigned number, unsigned long a, unsigned long b, unsigned long c) {
  Write64(machine->ax, number); Write64(machine->di, a);
  Write64(machine->si, b); Write64(machine->dx, c);
  Write64(machine->r10, 0); Write64(machine->r8, 0); Write64(machine->r9, 0);
  machine->ip = CODE;
  int halt = sigsetjmp(machine->onhalt, 1);
  if (!halt) { machine->canhalt = true; ExecuteInstruction(machine); }
  machine->canhalt = false;
  if (halt || observed_signal || machine->ip != CODE + 2 ||
      machine->insyscall || machine->sysdepth || machine->freelist.n)
    dispatch_error = 1;
  return (long)Read64(machine->ax);
}
static int Cycle(unsigned cycle) {
  unsigned char code[2] = {0x0f, 0x05};
  unsigned char bytes[64];
  unsigned char vectors[32];
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  CHECK(s != 0);
#ifdef GUEST_STREAMS_MANAGED
  if (BlinkHostInitializeBoundResourceLimits(s)) { FreeSystem(s); return __LINE__; }
#endif
  machine = NewMachine(s, 0);
  if (!machine) { FreeSystem(s); return __LINE__; }
  g_machine = machine;
  CHECK(!CountFds(&s->fds));
  for (int fd = 0; fd < 3; ++fd) {
    AddStdFd(&s->fds, fd);
    struct Fd *record = GetFd(&s->fds, fd);
    CHECK(record && record->fildes == fd && record->cb == &kFdCbHost);
    CHECK(record->cb->readv && record->cb->writev && record->cb->tcgetwinsize);
    CHECK((record->oflags & 3) == (fd ? 1 : 0) && !record->socktype);
    ++inherited_fds;
  }
  CHECK(CountFds(&s->fds) == 3);
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  s->cr3 = AllocatePageTable(s); CHECK(s->cr3);
  CHECK(ReserveVirtual(s, CODE, PAGE, PAGE_U | PAGE_RW, -1, 0, false, false) == CODE);
  CHECK(ReserveVirtual(s, DATA, 4 * PAGE, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == DATA);
  CHECK(!CopyToUserWrite(machine, CODE, code, sizeof(code)));
  memset(bytes, 0xa5, sizeof(bytes));
  CHECK(!CopyToUserWrite(machine, BUFFER - 16, bytes, sizeof(bytes)));
  unsigned total = 0;
  while (total < sizeof(input)) {
    long count = Call(0, 0, BUFFER + total, sizeof(input) - total);
    CHECK(count > 0 && (unsigned long)count <= sizeof(input) - total);
    total += count; input_bytes += count;
  }
  CHECK(!CopyFromUserRead(machine, bytes, BUFFER - 16, sizeof(bytes)));
  CHECK(!memcmp(bytes + 16, input, sizeof(input)));
  for (unsigned i = 0; i < 16; ++i) CHECK(bytes[i] == 0xa5 && bytes[16 + sizeof(input) + i] == 0xa5);
  if (cycle == 1) CHECK(Call(0, 0, BUFFER, 1) == 0);

  CHECK(!CopyToUserWrite(machine, DATA + 128, prefix, sizeof(prefix)));
  total = 0;
  while (total < sizeof(prefix) + sizeof(input)) {
    unsigned count_vectors = 0;
    if (total < sizeof(prefix)) {
      Write64(vectors, DATA + 128 + total); Write64(vectors + 8, sizeof(prefix) - total);
      Write64(vectors + 16, BUFFER); Write64(vectors + 24, sizeof(input)); count_vectors = 2;
    } else {
      Write64(vectors, BUFFER + total - sizeof(prefix));
      Write64(vectors + 8, sizeof(prefix) + sizeof(input) - total); count_vectors = 1;
    }
    CHECK(!CopyToUserWrite(machine, IOV, vectors, count_vectors * 16));
    long count = Call(20, 1, IOV, count_vectors);
    CHECK(count > 0 && (unsigned long)count <= sizeof(prefix) + sizeof(input) - total);
    total += count; output_bytes += count;
  }
  CHECK(!CopyToUserWrite(machine, DATA + 256, error_output, sizeof(error_output)));
  total = 0;
  while (total < sizeof(error_output)) {
    long count = Call(1, 2, DATA + 256 + total, sizeof(error_output) - total);
    CHECK(count > 0 && (unsigned long)count <= sizeof(error_output) - total);
    total += count; error_bytes += count;
  }
  memset(bytes, 0x6b, 40);
  CHECK(!CopyToUserWrite(machine, WINSIZE - 16, bytes, 40));
  CHECK(Call(16, 1, 0x5413, WINSIZE) == -25); /* TIOCGWINSZ -> ENOTTY */
  CHECK(!CopyFromUserRead(machine, bytes, WINSIZE - 16, 40));
  for (unsigned i = 0; i < 40; ++i) CHECK(bytes[i] == 0x6b);
  ++ioctl_refusals;
  CHECK(!dispatch_error && !observed_signal && !machine->freelist.n && CountFds(&s->fds) == 3);
  CHECK(!FreeVirtual(s, CODE, PAGE) && !FreeVirtual(s, DATA, 4 * PAGE) && !s->vss);
  struct Machine *released = machine; machine = 0;
  /* DestroyFds frees guest metadata. This deliberately does not call guest
   * close(0..2), and does not claim to test closing the underlying streams. */
  FreeMachine(released); g_machine = 0;
  ++metadata_cleanups; ++completed_cycles;
  return 0;
}
void GuestStreamsDestroy(void) {
  if (machine) { struct Machine *released = machine; machine = 0; FreeMachine(released); g_machine = 0; }
}
int GuestStreamsRelease(void) {
#ifdef GUEST_STREAMS_MANAGED
  if (exit_owner) { exit_owner = 0; BlinkHostExitCallbacksEnd(); }
  if (signal_owner) { signal_owner = 0; BlinkHostSignalActionsEnd(); }
  if (memory_owner) { memory_owner = 0; BlinkHostMemoryDisposeWorker(); }
#endif
  return 0;
}
const char *GuestStreamsReport(int result) {
  int length = snprintf(report, sizeof(report),
      "result=%d cycles=%u input_bytes=%u stdout_bytes=%u stderr_bytes=%u inherited_fds=%u metadata_cleanups=%u ioctl_enotty=%u surviving_standard_fds=%u\n",
      result, completed_cycles, input_bytes, output_bytes, error_bytes, inherited_fds, metadata_cleanups, ioctl_refusals, surviving_fds);
  if (length < 0 || (unsigned)length >= sizeof(report)) return 0;
  return report;
}
int GuestStreamsRun(void) {
  CHECK(!used); used = 1;
#ifdef GUEST_STREAMS_MANAGED
  CHECK(!BlinkHostMemoryBegin(64 * 1024 * 1024)); memory_owner = 1;
  CHECK(!BlinkHostSignalActionsBegin()); signal_owner = 1;
  CHECK(!BlinkHostExitCallbacksBegin()); exit_owner = 1;
  CHECK(!BlinkHostMemoryEnablePrivateFiles());
#endif
  InitMap(); InitBus();
#ifdef GUEST_STREAMS_MANAGED
  size_t retained_mappings = 0;
  size_t retained_bytes = 0;
#endif
  for (unsigned cycle = 0; cycle < 2; ++cycle) {
    int result = Cycle(cycle); if (result) return result;
#ifdef GUEST_STREAMS_MANAGED
    size_t mappings = BlinkHostMemoryMappings(); size_t bytes = BlinkHostMemoryBytes();
    CHECK(mappings && bytes && bytes <= BlinkHostMemoryLimit());
    if (!cycle) { retained_mappings = mappings; retained_bytes = bytes; }
    else CHECK(mappings == retained_mappings && bytes == retained_bytes);
#endif
  }
  for (int fd = 0; fd < 3; ++fd) { CHECK(fcntl(fd, F_GETFL) >= 0); ++surviving_fds; }
#ifdef GUEST_STREAMS_MANAGED
  CHECK(!BlinkHostExitCallbacksRun());
#endif
  return 0;
}
#ifndef GUEST_STREAMS_MANAGED
int main(int argc, char **argv) {
  if (argc != 2) return 2;
  int result = GuestStreamsRun();
  const char *text = GuestStreamsReport(result);
  FILE *sidecar = fopen(argv[1], "wb");
  if (!sidecar || !text) return 3;
  size_t length = strlen(text);
  int saved = fwrite(text, 1, length, sidecar) == length;
  if (fclose(sidecar)) saved = 0;
  GuestStreamsDestroy();
  int cleanup = GuestStreamsRelease();
  return result ? result : cleanup ? cleanup : saved ? 0 : 4;
}
#endif
