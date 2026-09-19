/* One finite normal TCP exchange through the pinned Linux SYSCALL dispatcher. */
#ifdef GUEST_TCP_MANAGED
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
#error GuestTcp requires interpreter-only configuration
#endif
#ifndef NOLINEAR
#error GuestTcp requires actual nonlinear page tables
#endif
#define CODE 0x400000UL
#define DATA 0x600000UL
#define PAGE 4096UL
#define ADDRESS (DATA + PAGE - 8)
#define ADDRLEN (DATA + 128)
#define OPTION (DATA + 160)
#define OPTLEN (DATA + 192)
#define POLL (DATA + 224)
#define INPUT (DATA + 3 * PAGE - 16)
#define OUTPUT (DATA + 5 * PAGE - 16)
#define CHECK(x) do { if (!(x)) return __LINE__; } while (0)
static struct Machine *machine;
static int used, memory_owner, signal_owner, exit_owner, observed_signal, dispatch_error;
static int listener, accepted, guest_port, peer_port;
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code; observed_signal = 1;
}
static long Call(unsigned number, unsigned long a, unsigned long b,
                 unsigned long c, unsigned long d, unsigned long e, unsigned long f) {
  Write64(machine->ax, number); Write64(machine->di, a);
  Write64(machine->si, b); Write64(machine->dx, c);
  Write64(machine->r10, d); Write64(machine->r8, e); Write64(machine->r9, f);
  machine->ip = CODE;
  int halt = sigsetjmp(machine->onhalt, 1);
  if (!halt) { machine->canhalt = true; ExecuteInstruction(machine); }
  machine->canhalt = false;
  if (halt || observed_signal || machine->ip != CODE + 2 || machine->insyscall ||
      machine->sysdepth || machine->freelist.n) dispatch_error = 1;
  return (long)Read64(machine->ax);
}
static int HostFd(int fd) {
  struct Fd *record = GetFd(&machine->system->fds, fd);
  CHECK(record && record->fildes == fd && record->cb == &kFdCbHost);
  CHECK(record->socktype == 1 && record->cb->poll && record->cb->close);
  return 0;
}
static int Bytes(unsigned long address, const unsigned char *expected, unsigned count) {
  unsigned char actual[512]; CHECK(count <= sizeof(actual));
  CHECK(!CopyFromUserRead(machine, actual, address, count));
  CHECK(!memcmp(actual, expected, count)); return 0;
}
static int AddressBuffer(void) {
  unsigned char bytes[32]; memset(bytes, 0xa5, sizeof(bytes));
  CHECK(!CopyToUserWrite(machine, ADDRESS - 8, bytes, sizeof(bytes)));
  memset(bytes, 0xa5, 12); Write32(bytes + 4, 16);
  CHECK(!CopyToUserWrite(machine, ADDRLEN - 4, bytes, 12)); return 0;
}
static int ReadAddress(int *port) {
  unsigned char bytes[32], length[12];
  CHECK(!CopyFromUserRead(machine, bytes, ADDRESS - 8, sizeof(bytes)));
  CHECK(!CopyFromUserRead(machine, length, ADDRLEN - 4, sizeof(length)));
  for (unsigned i = 0; i < 8; ++i) CHECK(bytes[i] == 0xa5 && bytes[i + 24] == 0xa5);
  for (unsigned i = 0; i < 4; ++i) CHECK(length[i] == 0xa5 && length[i + 8] == 0xa5);
  CHECK(Read32(length + 4) == 16 && Read16(bytes + 8) == 2);
  CHECK(bytes[12] == 127 && !bytes[13] && !bytes[14] && bytes[15] == 1);
  *port = (bytes[10] << 8) | bytes[11]; CHECK(*port > 0); return 0;
}
static int Option(int fd, unsigned level, unsigned name) {
  unsigned char value[12], length[12];
  memset(value, 0xa5, sizeof(value)); Write32(value + 4, 1);
  CHECK(!CopyToUserWrite(machine, OPTION - 4, value, sizeof(value)));
  CHECK(Call(54, fd, level, name, OPTION, 4, 0) == 0);
  Write32(value + 4, 0); CHECK(!CopyToUserWrite(machine, OPTION - 4, value, sizeof(value)));
  memset(length, 0xa5, sizeof(length)); Write32(length + 4, 4);
  CHECK(!CopyToUserWrite(machine, OPTLEN - 4, length, sizeof(length)));
  CHECK(Call(55, fd, level, name, OPTION, OPTLEN, 0) == 0);
  Write32(value + 4, 1);
  CHECK(!Bytes(OPTION - 4, value, sizeof(value)) && !Bytes(OPTLEN - 4, length, sizeof(length)));
  return 0;
}
static int PollReadable(int fd, int ready) {
  unsigned char bytes[16]; memset(bytes, 0xa5, sizeof(bytes));
  Write32(bytes + 4, fd); Write16(bytes + 8, 1); Write16(bytes + 10, 0);
  CHECK(!CopyToUserWrite(machine, POLL - 4, bytes, sizeof(bytes)));
  CHECK(Call(7, POLL, 1, 0, 0, 0, 0) == ready);
  Write16(bytes + 10, ready ? 1 : 0);
  CHECK(!Bytes(POLL - 4, bytes, sizeof(bytes))); return 0;
}
int GuestTcpListener(void) { return listener; }
int GuestTcpPort(void) { return guest_port; }
int GuestTcpPeerPort(void) { return peer_port; }
int GuestTcpSetup(void) {
  CHECK(!used); used = 1;
#ifdef GUEST_TCP_MANAGED
  CHECK(!BlinkHostMemoryBegin(64 * 1024 * 1024)); memory_owner = 1;
  CHECK(!BlinkHostSignalActionsBegin()); signal_owner = 1;
  CHECK(!BlinkHostExitCallbacksBegin()); exit_owner = 1;
#endif
  InitMap(); InitBus();
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG); CHECK(s);
#ifdef GUEST_TCP_MANAGED
  if (BlinkHostInitializeBoundResourceLimits(s)) { FreeSystem(s); return __LINE__; }
#endif
  machine = NewMachine(s, 0); if (!machine) { FreeSystem(s); return __LINE__; }
  g_machine = machine; CHECK(!CountFds(&s->fds));
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG; s->cr3 = AllocatePageTable(s); CHECK(s->cr3);
  CHECK(ReserveVirtual(s, CODE, PAGE, PAGE_U | PAGE_RW, -1, 0, false, false) == CODE);
  CHECK(ReserveVirtual(s, DATA, 8 * PAGE, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, false, false) == DATA);
  unsigned char code[2] = {0x0f, 0x05};
  CHECK(!CopyToUserWrite(machine, CODE, code, sizeof(code)));
  listener = Call(41, 2, 1, 0, 0, 0, 0);
  CHECK(listener >= 0 && !HostFd(listener) && CountFds(&s->fds) == 1);
  CHECK(!Option(listener, 1, 2));
  unsigned char address[16]; memset(address, 0, sizeof(address));
  Write16(address, 2); address[4] = 127; address[7] = 1;
  CHECK(!CopyToUserWrite(machine, ADDRESS, address, sizeof(address)));
  CHECK(Call(49, listener, ADDRESS, 16, 0, 0, 0) == 0);
  CHECK(!AddressBuffer()); CHECK(Call(51, listener, ADDRESS, ADDRLEN, 0, 0, 0) == 0);
  CHECK(!ReadAddress(&guest_port));
  CHECK(Call(50, listener, 1, 0, 0, 0, 0) == 0 && !PollReadable(listener, 0));
  printf("tcp setup ipv4_loopback=1 ephemeral=1 reuseaddr=1 optlen=4 listener_poll=0\n");
  return 0;
}
int GuestTcpExchange(void) {
  struct System *s = machine->system;
  CHECK(!PollReadable(listener, 1) && !AddressBuffer());
  accepted = Call(43, listener, ADDRESS, ADDRLEN, 0, 0, 0);
  CHECK(accepted >= 0 && accepted != listener && !HostFd(accepted) && CountFds(&s->fds) == 2);
  CHECK(!ReadAddress(&peer_port) && !Option(accepted, 6, 1));
  int port; CHECK(!AddressBuffer()); CHECK(Call(51, accepted, ADDRESS, ADDRLEN, 0, 0, 0) == 0);
  CHECK(!ReadAddress(&port) && port == guest_port && !PollReadable(accepted, 1));
  printf("tcp accept count=1 sockaddr_cross_page=1 peer_loopback=1 local_matches=1 nodelay=1 optlen=4\n");
  unsigned char bytes[512]; memset(bytes, 0xa5, sizeof(bytes));
  CHECK(!CopyToUserWrite(machine, INPUT - 8, bytes, 257 + 16));
  unsigned total = 0;
  while (total < 257) {
    long count = Call(45, accepted, INPUT + total, 257 - total, 0, 0, 0);
    CHECK(count > 0 && count <= 257 - total); total += count;
  }
  for (unsigned i = 0; i < 257; ++i) bytes[i + 8] = (unsigned char)(i * 17 + 3);
  CHECK(!Bytes(INPUT - 8, bytes, 257 + 16));
  CHECK(Call(45, accepted, INPUT, 1, 0, 0, 0) == 0);
  CHECK(!Bytes(INPUT - 8, bytes, 257 + 16));
  printf("tcp receive bytes=257 exact=1 cross_page=1 canaries=1 eof=1\n");
  memset(bytes, 0xa5, sizeof(bytes));
  for (unsigned i = 0; i < 263; ++i) bytes[i + 8] = (unsigned char)(i * 29 + 11);
  CHECK(!CopyToUserWrite(machine, OUTPUT - 8, bytes, 263 + 16));
  total = 0;
  while (total < 263) {
    long count = Call(44, accepted, OUTPUT + total, 263 - total, 0x4000, 0, 0);
    CHECK(count > 0 && count <= 263 - total); total += count;
  }
  CHECK(!Bytes(OUTPUT - 8, bytes, 263 + 16));
  CHECK(Call(48, accepted, 1, 0, 0, 0, 0) == 0);
  CHECK(Call(3, accepted, 0, 0, 0, 0, 0) == 0 && CountFds(&s->fds) == 1);
  CHECK(Call(3, listener, 0, 0, 0, 0, 0) == 0 && !CountFds(&s->fds));
  CHECK(!dispatch_error && !observed_signal && !machine->freelist.n);
  CHECK(!FreeVirtual(s, CODE, PAGE) && !FreeVirtual(s, DATA, 8 * PAGE) && !s->vss);
  printf("tcp send bytes=263 cross_page=1 canaries=1 nosignal=1 shutdown_write=1 guest_fds=0 pages=0 syscall_cleanup=1\n");
  struct Machine *released = machine; machine = 0; FreeMachine(released); g_machine = 0;
#ifdef GUEST_TCP_MANAGED
  CHECK(!BlinkHostExitCallbacksRun());
#endif
  return 0;
}
void GuestTcpDestroy(void) {
  if (machine) { struct Machine *released = machine; machine = 0; FreeMachine(released); g_machine = 0; }
}
int GuestTcpRelease(void) {
#ifdef GUEST_TCP_MANAGED
  if (exit_owner) { exit_owner = 0; BlinkHostExitCallbacksEnd(); }
  if (signal_owner) { signal_owner = 0; BlinkHostSignalActionsEnd(); }
  if (memory_owner) { memory_owner = 0; BlinkHostMemoryDisposeWorker(); }
#endif
  return 0;
}
