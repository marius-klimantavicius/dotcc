#include <stdio.h>
#include <string.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/machine.h"
#include "blink/map.h"
#include "blink/signal.h"

static int observed_signal;
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)code; observed_signal = signal;
}
static int Run(const char *name, const unsigned char *code, size_t length,
               int steps, int expected_halt, int check_xmm) {
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  if (!s) return 1;
  struct Machine *m = NewMachine(s, 0);
  if (!m) return 1;
  g_machine = m;
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  s->cr3 = AllocatePageTable(s);
  if (ReserveVirtual(s, 0x400000, 4096, PAGE_U | PAGE_RW, -1, 0, 0, 0) == -1 ||
      ReserveVirtual(s, 0x600000, 4096, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, 0, 0) == -1 ||
      CopyToUser(m, 0x400000, code, length)) return 2;
  m->ip = 0x400000;
  Write64(m->bx, 0x600000);
  memset(m->xmm, 0xa5, sizeof(m->xmm));
  m->mxcsr = 0x1f80;
  observed_signal = 0;
  volatile int completed = 0;
  int halt = sigsetjmp(m->onhalt, 1);
  if (!halt) {
    m->canhalt = true;
    while (completed < steps) { ExecuteInstruction(m); ++completed; }
  }
  m->canhalt = false;
  int good = halt == expected_halt && completed == (expected_halt ? 0 : steps) &&
             observed_signal == (expected_halt ? 4 : 0);
  if (check_xmm) {
    for (int i = 0; i < 16; ++i)
      if (((unsigned char *)m->xmm)[i] != (check_xmm == 1 ? 0 : 0xa5)) good = 0;
    if (m->mxcsr != 0x1f80) good = 0;
  }
  printf("%s completed=%d halt=%d signal=%d checked=%d\n",
         name, completed, halt, observed_signal, good);
  FreeMachine(m);
  return good ? 0 : 3;
}
int main(void) {
  const unsigned char x87[] = {0xd9, 0xe8};
  const unsigned char fwait[] = {0x9b};
  const unsigned char mmx[] = {0x0f, 0xef, 0xc0};
  const unsigned char bmi2[] = {0xc4, 0xe2, 0xfb, 0xf5, 0xc0};
  const unsigned char adx[] = {0x66, 0x48, 0x0f, 0x38, 0xf6, 0xc0};
  const unsigned char sse2[] = {0x66, 0x0f, 0xef, 0xc0};
  const unsigned char fxsave_restore[] = {0x0f, 0xae, 0x03,
      0x66, 0x0f, 0xef, 0xc0, 0x0f, 0xae, 0x0b};
  InitMap(); InitBus();
  if (Run("x87-fld1", x87, sizeof(x87), 1, kMachineUndefinedInstruction, 0) ||
      Run("x87-fwait", fwait, sizeof(fwait), 1, kMachineUndefinedInstruction, 0) ||
      Run("mmx-pxor", mmx, sizeof(mmx), 1, kMachineUndefinedInstruction, 0) ||
      Run("bmi2-pdep", bmi2, sizeof(bmi2), 1, kMachineUndefinedInstruction, 0) ||
      Run("adx-adcx", adx, sizeof(adx), 1, kMachineUndefinedInstruction, 0) ||
      Run("sse2-pxor", sse2, sizeof(sse2), 1, 0, 1) ||
      Run("fxsave-sse-restore", fxsave_restore, sizeof(fxsave_restore), 3, 0, 2)) return 1;
  return 0;
}
