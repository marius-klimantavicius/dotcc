/* Normal Linux guest file syscalls through the pinned instruction dispatcher. */
#ifdef GUEST_IO_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <stdio.h>
#include <string.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/fds.h"
#include "blink/machine.h"
#include "blink/map.h"
#ifndef DISABLE_JIT
#error GuestIo requires interpreter-only configuration
#endif
#ifndef NOLINEAR
#error GuestIo requires actual nonlinear guest page tables
#endif
#define CODE 0x400000UL
#define DATA 0x600000UL
#define PAGE 4096UL
#define LENGTH 131072UL
#define INPUT (DATA + 4 * PAGE - 32)
#define OUTPUT (DATA + 40 * PAGE - 32)
#define IOV (DATA + PAGE - 8)
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static struct Machine *machine;
static int used, memory_owner, signal_owner, exit_owner, observed_signal, dispatch_error;
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code; observed_signal = 1;
}
/* Arguments and return values use the Linux x86-64 register ABI. Every call
 * decodes and executes the actual 0f05 instruction; no Sys* handler is called
 * directly and no callback or result is supplied by the fixture. */
static long Call(unsigned number, unsigned long a, unsigned long b, unsigned long c) {
  Write64(machine->ax, number); Write64(machine->di, a);
  Write64(machine->si, b); Write64(machine->dx, c);
  Write64(machine->r10, 0); Write64(machine->r8, 0); Write64(machine->r9, 0);
  machine->ip = CODE;
  int halt = sigsetjmp(machine->onhalt, 1);
  if (!halt) { machine->canhalt = true; ExecuteInstruction(machine); }
  machine->canhalt = false;
  if (halt || observed_signal || machine->ip != CODE + 2 ||
      machine->insyscall || machine->sysdepth || machine->freelist.n) {
    dispatch_error = 1;
  }
  return (long)Read64(machine->ax);
}
static unsigned char Pattern(unsigned long position, unsigned cycle) {
  return (unsigned char)(position * 37 + cycle * 13 + 7);
}
static int CheckBytes(unsigned long address, const unsigned char *expected, unsigned count) {
  unsigned char actual[4096];
  CHECK(count <= sizeof(actual));
  CHECK(!CopyFromUserRead(machine, actual, address, count));
  CHECK(!memcmp(actual, expected, count));
  return 0;
}
static int Vectors(unsigned long first, unsigned first_count, unsigned long second, unsigned second_count) {
  unsigned char bytes[32];
  Write64(bytes, first); Write64(bytes + 8, first_count);
  Write64(bytes + 16, second); Write64(bytes + 24, second_count);
  CHECK(!CopyToUserWrite(machine, IOV, bytes, sizeof(bytes)));
  return 0;
}
static int HostFd(int fd) {
  struct Fd *record = GetFd(&machine->system->fds, fd);
  CHECK(record && record->fildes == fd && record->cb == &kFdCbHost);
  CHECK(record->cb->readv && record->cb->writev && record->cb->poll && record->cb->close);
  return 0;
}
static int Cycle(unsigned cycle) {
  unsigned char code[2] = {0x0f, 0x05};
  unsigned char block[4096];
  unsigned char vector_bytes[96];
  unsigned char pollfd[8];
  char path[] = "guest-io.bin";
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  CHECK(s != 0);
#ifdef GUEST_IO_MANAGED
  if (BlinkHostInitializeBoundResourceLimits(s)) { FreeSystem(s); return __LINE__; }
#endif
  machine = NewMachine(s, 0);
  if (!machine) { FreeSystem(s); return __LINE__; }
  g_machine = machine;
  CHECK(!CountFds(&s->fds));
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  s->cr3 = AllocatePageTable(s);
  CHECK(s->cr3);
  CHECK(ReserveVirtual(s, CODE, PAGE, PAGE_U | PAGE_RW, -1, 0, false, false) == CODE);
  CHECK(ReserveVirtual(s, DATA, 80 * PAGE, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == DATA);
  CHECK(!CopyToUserWrite(machine, CODE, code, sizeof(code)));
  CHECK(!CopyToUserWrite(machine, DATA + 128, path, sizeof(path)));
  for (unsigned long offset = 0; offset < LENGTH; offset += sizeof(block)) {
    for (unsigned i = 0; i < sizeof(block); ++i) block[i] = Pattern(offset + i, cycle);
    CHECK(!CopyToUserWrite(machine, INPUT + offset, block, sizeof(block)));
  }
  /* Linux open: O_RDWR|O_CREAT|O_TRUNC, mode0600. Native Cwd is a dedicated
   * empty directory; managed Cwd is its private root. No host path is embedded. */
  int fd = Call(2, DATA + 128, 2 | 64 | 512, 0600);
  CHECK(fd >= 0 && !HostFd(fd) && CountFds(&s->fds) == 1);
  unsigned long total = 0;
  while (total < LENGTH) {
    long count = Call(1, fd, INPUT + total, LENGTH - total);
    CHECK(count > 0 && (unsigned long)count <= LENGTH - total);
    CHECK(machine->readaddr == INPUT + total && machine->readsize == count);
    total += count;
  }
  CHECK(Call(8, fd, 0, 1) == LENGTH);
  printf("cycle=%u file bytes=%lu written=1 host_callback=1\n", cycle, LENGTH);

  int copy = Call(32, fd, 0, 0);
  CHECK(copy >= 0 && copy != fd && !HostFd(copy) && CountFds(&s->fds) == 2);
  CHECK(Call(8, copy, 0, 0) == 0);
  CHECK(Call(0, fd, OUTPUT, 32) == 32);
  total = 32;
  while (total < LENGTH) {
    long count = Call(0, copy, OUTPUT + total, LENGTH - total);
    CHECK(count > 0 && (unsigned long)count <= LENGTH - total);
    CHECK(machine->writeaddr == OUTPUT + total && machine->writesize == count);
    total += count;
  }
  for (unsigned long offset = 0; offset < LENGTH; offset += sizeof(block)) {
    for (unsigned i = 0; i < sizeof(block); ++i) block[i] = Pattern(offset + i, cycle);
    CHECK(!CheckBytes(OUTPUT + offset, block, sizeof(block)));
  }
  CHECK(Call(8, fd, 0, 1) == LENGTH);
  CHECK(Call(0, copy, OUTPUT, 1) == 0);
  memset(block, 0, 16);
  CHECK(!CheckBytes(OUTPUT - 16, block, 16));
  CHECK(!CheckBytes(OUTPUT + LENGTH, block, 16));
  printf("cycle=%u duplicate shared_cursor=1 exact_read=%lu eof=1\n", cycle, LENGTH);

  CHECK(!CopyFromUserRead(machine, vector_bytes, INPUT, 64));
  CHECK(!CopyFromUserRead(machine, vector_bytes + 64, INPUT + PAGE + 16, 32));
  CHECK(!Vectors(INPUT, 64, INPUT + PAGE + 16, 32));
  CHECK(Call(8, fd, 0, 0) == 0);
  CHECK(Call(20, copy, IOV, 2) == 96);
  CHECK(Call(8, fd, 0, 0) == 0);
  CHECK(!Vectors(OUTPUT, 40, OUTPUT + PAGE + 16, 56));
  CHECK(Call(19, fd, IOV, 2) == 96);
  CHECK(!CheckBytes(OUTPUT, vector_bytes, 40));
  CHECK(!CheckBytes(OUTPUT + PAGE + 16, vector_bytes + 40, 56));
  CHECK(Call(8, copy, 0, 1) == 96);
  printf("cycle=%u vectors bytes=96 table_cross_page=1 payload_cross_page=1 exact=1\n", cycle);

  Write32(pollfd, fd); Write16(pollfd + 4, 5); Write16(pollfd + 6, 0);
  CHECK(!CopyToUserWrite(machine, DATA + 512, pollfd, sizeof(pollfd)));
  CHECK(Call(7, DATA + 512, 1, 0) == 1);
  CHECK(!CopyFromUserRead(machine, pollfd, DATA + 512, sizeof(pollfd)));
  CHECK(Read32(pollfd) == fd && Read16(pollfd + 4) == 5 && Read16(pollfd + 6) == 5);
  CHECK(Call(3, fd, 0, 0) == 0 && CountFds(&s->fds) == 1 && !HostFd(copy));
  CHECK(Call(0, copy, OUTPUT, 16) == 16);
  for (unsigned i = 0; i < 16; ++i) block[i] = Pattern(96 + i, cycle);
  CHECK(!CheckBytes(OUTPUT, block, 16));
  CHECK(Call(3, copy, 0, 0) == 0 && !CountFds(&s->fds));
  printf("cycle=%u poll ready=1 revents=5 close_duplicate_survives=1\n", cycle);

  fd = Call(2, DATA + 128, 0, 0);
  CHECK(fd >= 0 && !HostFd(fd) && CountFds(&s->fds) == 1);
  CHECK(Call(8, fd, 0, 2) == LENGTH && Call(8, fd, 0, 0) == 0);
  CHECK(Call(0, fd, OUTPUT, 96) == 96 && !CheckBytes(OUTPUT, vector_bytes, 96));
  total = 96;
  while (total < LENGTH) {
    unsigned wanted = LENGTH - total < sizeof(block) ? LENGTH - total : sizeof(block);
    long count = Call(0, fd, OUTPUT, wanted);
    CHECK(count > 0 && count <= wanted);
    for (unsigned i = 0; i < (unsigned)count; ++i) block[i] = Pattern(total + i, cycle);
    CHECK(!CheckBytes(OUTPUT, block, count));
    total += count;
  }
  CHECK(Call(3, fd, 0, 0) == 0 && !CountFds(&s->fds));
  CHECK(!dispatch_error && !observed_signal && !machine->freelist.n);
  CHECK(!FreeVirtual(s, CODE, PAGE) && !FreeVirtual(s, DATA, 80 * PAGE) && !s->vss);
  printf("cycle=%u reopen exact=1 bytes=%lu guest_fds=0 pages=0 syscall_cleanup=1\n", cycle, LENGTH);
  struct Machine *released = machine; machine = 0;
  FreeMachine(released); g_machine = 0;
  return 0;
}
void GuestIoDestroy(void) {
  if (machine) { struct Machine *released = machine; machine = 0; FreeMachine(released); g_machine = 0; }
}
int GuestIoRelease(void) {
#ifdef GUEST_IO_MANAGED
  if (exit_owner) { exit_owner = 0; BlinkHostExitCallbacksEnd(); }
  if (signal_owner) { signal_owner = 0; BlinkHostSignalActionsEnd(); }
  if (memory_owner) { memory_owner = 0; BlinkHostMemoryDisposeWorker(); }
#endif
  return 0;
}
int GuestIoRun(void) {
  CHECK(!used); used = 1;
#ifdef GUEST_IO_MANAGED
  CHECK(!BlinkHostMemoryBegin(64 * 1024 * 1024)); memory_owner = 1;
  CHECK(!BlinkHostSignalActionsBegin()); signal_owner = 1;
  CHECK(!BlinkHostExitCallbacksBegin()); exit_owner = 1;
  CHECK(!BlinkHostMemoryEnablePrivateFiles());
#endif
  InitMap(); InitBus();
#ifdef GUEST_IO_MANAGED
  size_t retained_mappings = 0;
  size_t retained_bytes = 0;
#endif
  for (unsigned cycle = 0; cycle < 2; ++cycle) {
    int result = Cycle(cycle);
    if (result) return result;
#ifdef GUEST_IO_MANAGED
    size_t mappings = BlinkHostMemoryMappings();
    size_t bytes = BlinkHostMemoryBytes();
    CHECK(mappings && bytes && bytes <= BlinkHostMemoryLimit());
    if (!cycle) { retained_mappings = mappings; retained_bytes = bytes; }
    else CHECK(mappings == retained_mappings && bytes == retained_bytes);
#endif
  }
#ifdef GUEST_IO_MANAGED
  CHECK(!BlinkHostExitCallbacksRun());
#endif
  return 0;
}
#ifndef GUEST_IO_MANAGED
int main(void) {
  int result = GuestIoRun(); GuestIoDestroy();
  int cleanup = GuestIoRelease();
  return result ? result : cleanup;
}
#endif
