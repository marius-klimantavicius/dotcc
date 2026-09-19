/* Valid guest page-table operations through the pinned upstream algorithms. */
#ifdef GUEST_MEMORY_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <stdio.h>
#include <string.h>
#include <sys/mman.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/machine.h"
#include "blink/map.h"
#ifndef DISABLE_JIT
#error GuestMemory requires interpreter-only configuration
#endif
#ifndef NOLINEAR
#error GuestMemory requires actual nonlinear guest page tables
#endif
#define BASE 0x800000UL
#define EXTRA 0xa00000UL
#define PAGE 4096UL
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static struct Machine *machine;
static int used, memory_owner, signal_owner, exit_owner, observed_signal;
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code; observed_signal = 1;
}
static int ZeroPage(unsigned long address) {
  unsigned char bytes[4096];
  CHECK(!CopyFromUserRead(machine, bytes, address, sizeof(bytes)));
  CHECK(machine->readaddr == address && machine->readsize == sizeof(bytes));
  for (unsigned i = 0; i < sizeof(bytes); ++i) CHECK(!bytes[i]);
  return 0;
}
static int Permissions(unsigned long address, unsigned count, int writable) {
  unsigned long mask = PAGE_V | PAGE_U | PAGE_RW | PAGE_XD;
  unsigned long expected = PAGE_V | PAGE_U | PAGE_XD;
  if (writable) expected |= PAGE_RW;
  for (unsigned i = 0; i < count; ++i)
    CHECK((FindPageTableEntry(machine, address + i * PAGE) & mask) == expected);
  return 0;
}
static int Transfer(unsigned long address, unsigned seed) {
  unsigned char input[96]; unsigned char output[96]; unsigned char canary[8];
  for (unsigned i = 0; i < sizeof(input); ++i) input[i] = (unsigned char)(i * 37 + seed);
  CHECK(!CopyToUserWrite(machine, address, input, sizeof(input)));
  CHECK(machine->writeaddr == address && machine->writesize == sizeof(input));
  CHECK(!CopyFromUserRead(machine, output, address, sizeof(output)));
  CHECK(machine->readaddr == address && machine->readsize == sizeof(output));
  CHECK(!memcmp(input, output, sizeof(input)));
  CHECK(!CopyFromUserRead(machine, canary, address - sizeof(canary), sizeof(canary)));
  for (unsigned i = 0; i < sizeof(canary); ++i) CHECK(!canary[i]);
  CHECK(!CopyFromUserRead(machine, canary, address + sizeof(input), sizeof(canary)));
  for (unsigned i = 0; i < sizeof(canary); ++i) CHECK(!canary[i]);
  return 0;
}
static int Cycle(unsigned cycle) {
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  CHECK(s != 0);
#ifdef GUEST_MEMORY_MANAGED
  if (BlinkHostInitializeBoundResourceLimits(s)) { FreeSystem(s); return __LINE__; }
#endif
  machine = NewMachine(s, 0);
  if (!machine) { FreeSystem(s); return __LINE__; }
  g_machine = machine;
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  s->cr3 = AllocatePageTable(s);
  CHECK(s->cr3 && !s->vss);
  CHECK(ReserveVirtual(s, BASE, PAGE * 2, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == BASE);
  CHECK(s->vss == 2 && IsFullyMapped(s, BASE, PAGE * 2));
  CHECK(!ZeroPage(BASE) && !ZeroPage(BASE + PAGE));
  CHECK(!Permissions(BASE, 2, 1));
  printf("cycle=%u initial pages=%ld rss=%ld zero=1 rw_nx=1\n", cycle, (long)s->vss, (long)s->rss);

  CHECK(ReserveVirtual(s, BASE + PAGE * 2, PAGE * 2, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == BASE + PAGE * 2);
  CHECK(ReserveVirtual(s, EXTRA, PAGE, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == EXTRA);
  CHECK(s->vss == 5 && IsFullyMapped(s, BASE, PAGE * 4) && IsFullyMapped(s, EXTRA, PAGE));
  CHECK(!ZeroPage(BASE + PAGE * 2) && !ZeroPage(BASE + PAGE * 3) && !ZeroPage(EXTRA));
  CHECK(!Transfer(BASE + PAGE - 32, cycle + 1));
  CHECK(!Transfer(BASE + PAGE * 2 - 48, cycle + 11));
  CHECK(!Transfer(EXTRA + 128, cycle + 21));
  printf("cycle=%u grown pages=%ld rss=%ld cross_page=2 separate=1 canaries=1\n", cycle, (long)s->vss, (long)s->rss);

  CHECK(!ProtectVirtual(s, BASE, PAGE * 2, PROT_READ, false));
  CHECK(!Permissions(BASE, 2, 0));
  unsigned char bytes[96];
  CHECK(!CopyFromUserRead(machine, bytes, BASE + PAGE - 32, sizeof(bytes)));
  for (unsigned i = 0; i < sizeof(bytes); ++i) CHECK(bytes[i] == (unsigned char)(i * 37 + cycle + 1));
  CHECK(!ProtectVirtual(s, BASE, PAGE * 2, PROT_READ | PROT_WRITE, false));
  CHECK(!Permissions(BASE, 2, 1));
  CHECK(!Transfer(BASE + PAGE - 32, cycle + 1));
  printf("cycle=%u protection read_nx=1 read_preserved=1 restored_rw_nx=1\n", cycle);

  CHECK(!FreeVirtual(s, BASE + PAGE, PAGE));
  CHECK(s->vss == 4 && IsFullyUnmapped(s, BASE + PAGE, PAGE));
  CHECK(ReserveVirtual(s, BASE + PAGE, PAGE, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == BASE + PAGE);
  CHECK(s->vss == 5 && !ZeroPage(BASE + PAGE) && !Permissions(BASE + PAGE, 1, 1));
  CHECK(!CopyFromUserRead(machine, bytes, BASE + PAGE - 32, 32));
  for (unsigned i = 0; i < 32; ++i) CHECK(bytes[i] == (unsigned char)(i * 37 + cycle + 1));
  CHECK(!CopyFromUserRead(machine, bytes, BASE + PAGE * 2, 48));
  for (unsigned i = 0; i < 48; ++i) CHECK(bytes[i] == (unsigned char)((i + 48) * 37 + cycle + 11));
  CHECK(!Transfer(BASE + PAGE + 128, cycle + 31));
  CHECK(!CopyFromUserRead(machine, bytes, EXTRA + 128, sizeof(bytes)));
  for (unsigned i = 0; i < sizeof(bytes); ++i) CHECK(bytes[i] == (unsigned char)(i * 37 + cycle + 21));
  printf("cycle=%u remap pages=%ld rss=%ld zero=1 refill=1 neighbors_preserved=1\n", cycle, (long)s->vss, (long)s->rss);

  CHECK(!FreeVirtual(s, BASE, PAGE * 4) && !FreeVirtual(s, EXTRA, PAGE));
  CHECK(!s->vss && IsFullyUnmapped(s, BASE, PAGE * 4) && IsFullyUnmapped(s, EXTRA, PAGE));
  CHECK(!observed_signal);
  printf("cycle=%u unmapped pages=%ld rss=%ld metadata_empty=1\n", cycle, (long)s->vss, (long)s->rss);
  struct Machine *released = machine; machine = 0;
  FreeMachine(released); g_machine = 0;
  return 0;
}
void GuestMemoryDestroy(void) {
  if (machine) { struct Machine *released = machine; machine = 0; FreeMachine(released); g_machine = 0; }
}
int GuestMemoryRelease(void) {
#ifdef GUEST_MEMORY_MANAGED
  if (exit_owner) { exit_owner = 0; BlinkHostExitCallbacksEnd(); }
  if (signal_owner) { signal_owner = 0; BlinkHostSignalActionsEnd(); }
  if (memory_owner) { memory_owner = 0; BlinkHostMemoryDisposeWorker(); }
#endif
  return 0;
}
int GuestMemoryRun(void) {
  CHECK(!used); used = 1;
#ifdef GUEST_MEMORY_MANAGED
  CHECK(!BlinkHostMemoryBegin(64 * 1024 * 1024)); memory_owner = 1;
  CHECK(!BlinkHostSignalActionsBegin()); signal_owner = 1;
  CHECK(!BlinkHostExitCallbacksBegin()); exit_owner = 1;
  CHECK(!BlinkHostMemoryEnablePrivateFiles());
#endif
  InitMap(); InitBus();
#ifdef GUEST_MEMORY_MANAGED
  size_t retained_mappings = 0;
  size_t retained_bytes = 0;
#endif
  for (unsigned cycle = 0; cycle < 2; ++cycle) {
    int result = Cycle(cycle);
    if (result) return result;
#ifdef GUEST_MEMORY_MANAGED
    size_t mappings = BlinkHostMemoryMappings();
    size_t bytes = BlinkHostMemoryBytes();
    CHECK(mappings && bytes && bytes <= BlinkHostMemoryLimit());
    if (!cycle) { retained_mappings = mappings; retained_bytes = bytes; }
    else CHECK(mappings == retained_mappings && bytes == retained_bytes);
#endif
  }
#ifdef GUEST_MEMORY_MANAGED
  CHECK(!BlinkHostExitCallbacksRun());
#endif
  return 0;
}
#ifndef GUEST_MEMORY_MANAGED
int main(void) {
  int result = GuestMemoryRun(); GuestMemoryDestroy();
  int cleanup = GuestMemoryRelease();
  return result ? result : cleanup;
}
#endif
