/* Bounded, normal execution of a fixed valid TLS image, not a service worker. */
#ifdef BLINK_TLS_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <stdio.h>
#include <stdlib.h>
#include <sys/mman.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/loader.h"
#include "blink/map.h"
#include "blink/overlays.h"
#include "blink/signal.h"

/* FIXTURE_CONSTANTS */
static int observed_signal;
void TerminateSignal(struct Machine *m, int sig, int code) {
  (void)m; (void)code; observed_signal = sig;
}
static void Check(int good) {
  if (!good) { fputs("normal TLS fixture invariant failed\n", stderr); exit(1); }
}
static unsigned long Word(struct Machine *m, unsigned long address) {
  unsigned char bytes[8];
  Check(!CopyFromUser(m, bytes, address, sizeof(bytes)));
  return Read64(bytes);
}
static int LoadAndRun(char *path) {
  InitMap(); InitBus();
  Check(!SetOverlays("", false));
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  Check(s != 0);
#ifdef BLINK_TLS_MANAGED
  Check(!BlinkHostInitializeBoundResourceLimits(s));
#endif
  struct Machine *m = NewMachine(s, 0);
  Check(m != 0);
  g_machine = m; s->trapexit = true;
  char arg0[] = "tls-fixture";
  char *args[] = {arg0, 0};
  char *vars[] = {0};
  LoadProgram(m, arg0, path, args, vars, 0);
  Check(s->loaded && !s->exited && !s->elf.interpreter && !s->elf.aslr);
  Check(m->ip == FIXTURE_ENTRY);
  Check(s->elf.at_phent == 56 && s->elf.at_phnum == FIXTURE_PHNUM);
  unsigned char tls_header[56];
  Check(!CopyFromUser(m, tls_header, s->elf.at_phdr + FIXTURE_TLS_INDEX*56, sizeof(tls_header)));
  Check(Read32(tls_header) == 7 && Read32(tls_header+4) == 4);
  Check(Read64(tls_header+8) == FIXTURE_TLS_OFFSET && Read64(tls_header+16) == FIXTURE_TEMPLATE);
  Check(Read64(tls_header+24) == FIXTURE_TLS_PADDR);
  Check(Read64(tls_header+32) == 8 && Read64(tls_header+40) == 16 && Read64(tls_header+48) == 8);
  Check(Word(m, FIXTURE_TEMPLATE) == 0x1122334455667788UL);
  Check(!Word(m, FIXTURE_RUNTIME) && !Word(m, FIXTURE_RUNTIME+8));
  Check(IsValidMemory(m, FIXTURE_RUNTIME, 16, PROT_READ|PROT_WRITE));
  Check(!IsValidMemory(m, FIXTURE_RUNTIME, 16, PROT_EXEC));
  observed_signal = 0;
  volatile unsigned completed = 0;
  int halt = sigsetjmp(m->onhalt, 1);
  if (!halt) {
    m->canhalt = true;
    while (completed < 128) { ExecuteInstruction(m); ++completed; }
  }
  m->canhalt = false;
  Check(halt == kMachineExitTrap && !observed_signal && s->exited && !s->exitcode);
  Check(completed == 25 && m->ip == FIXTURE_EXIT && m->fs.base == FIXTURE_RUNTIME);
  unsigned long initial = Word(m, FIXTURE_RUNTIME);
  unsigned long updated = Word(m, FIXTURE_RUNTIME+8);
  Check(initial == 0x1122334455667788UL && updated == 9);
  Check(Read64(m->dx) == 0x1122334455667791UL);
  printf("tls filesz=8 memsz=16 align=8 phdr_preserved=1 runtime_rw_nx=1 fs=%lx initial=%lx updated=%lu sum=%lx steps=%u halt=%d exit=%d\n",
         (unsigned long)m->fs.base, initial, updated, Read64(m->dx), completed, halt, s->exitcode);
  FreeMachine(m);
  return 0;
}
#ifdef BLINK_TLS_MANAGED
static int already_run;
int TlsLoadingRun(void) {
  if (already_run) return 70;
  already_run = 1;
  if (BlinkHostMemoryBegin(64*1024*1024)) return 71;
  if (BlinkHostSignalActionsBegin()) { BlinkHostMemoryDisposeWorker(); return 72; }
  if (BlinkHostExitCallbacksBegin()) { BlinkHostSignalActionsEnd(); BlinkHostMemoryDisposeWorker(); return 73; }
  if (BlinkHostMemoryEnablePrivateFiles()) { BlinkHostExitCallbacksEnd(); BlinkHostSignalActionsEnd(); BlinkHostMemoryDisposeWorker(); return 74; }
  char path[] = "/bin/tls-fixture";
  int result = LoadAndRun(path);
  if (BlinkHostExitCallbacksRun() && !result) result = 75;
  BlinkHostExitCallbacksEnd(); BlinkHostSignalActionsEnd(); BlinkHostMemoryDisposeWorker();
  return result;
}
#else
int main(int argc, char **argv) { Check(argc == 2); return LoadAndRun(argv[1]); }
#endif
