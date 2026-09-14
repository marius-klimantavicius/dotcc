/* Native oracle for an embedding seam around unchanged upstream execution.
 * No guest instructions, address translation, or fault decoding are authored here.
 */
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/machine.h"
#include "blink/map.h"
#include "blink/signal.h"

static int observed_signal;
static int observed_code;

/* This is the upstream frontend hook (CLI and TUI supply their own versions).
 * Capture the event, then let upstream HaltMachine perform synchronous unwind.
 * This probe executes only fixed byte strings, without guest signal handlers.
 */
void TerminateSignal(struct Machine *m, int sig, int code) {
  (void)m;
  observed_signal = sig;
  observed_code = code;
}

static int RunCase(const char *name, unsigned char *code, size_t length,
                   unsigned budget, int expected_halt, unsigned long expected_ax,
                   unsigned long expected_memory, int expected_signal,
                   unsigned long expected_ip) {
  struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
  if (!s) return 10;
  struct Machine *m = NewMachine(s, 0);
  if (!m) { FreeSystem(s); return 11; }
  g_machine = m;
  s->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  s->cr3 = AllocatePageTable(s);
  if (ReserveVirtual(s, 0x400000, 4096, PAGE_U | PAGE_RW, -1, 0, 0, 0) == -1 ||
      ReserveVirtual(s, 0x600000, 4096, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, 0, 0) == -1 ||
      CopyToUser(m, 0x400000, code, length)) { FreeMachine(m); return 12; }
  m->ip = 0x400000;
  Write64(m->bx, 0x600000);
  observed_signal = 0;
  observed_code = 0;
  volatile unsigned completed = 0;
  int halt = sigsetjmp(m->onhalt, 1);
  if (!halt) {
    m->canhalt = true;
    while (completed < budget) {
      ExecuteInstruction(m);
      ++completed;
    }
  }
  m->canhalt = false;
  unsigned char memory[8];
  if (CopyFromUser(m, memory, 0x600000, sizeof(memory))) { FreeMachine(m); return 13; }
  unsigned long ax = Read64(m->ax), value = Read64(memory);
  printf("%s steps=%u halt=%d signal=%d code=%d ax=%lu memory=%lu ip=%lx\n",
         name, completed, halt, observed_signal, observed_code, ax, value,
         (unsigned long)m->ip);
  int good = halt == expected_halt && ax == expected_ax && value == expected_memory &&
             observed_signal == expected_signal && observed_code == (expected_signal ? 1 : 0) &&
             completed == budget - (expected_halt ? 1 : 0) && m->ip == expected_ip;
  FreeMachine(m);
  return good ? 0 : 1;
}

int main(void) {
  /* mov eax,40; add eax,2; mov [rbx],rax; inc qword [rbx]; ud2 */
  unsigned char arithmetic[] = {0xb8,40,0,0,0, 0x83,0xc0,2, 0x48,0x89,0x03,
                               0x48,0xff,0x03, 0x0f,0x0b};
  /* A real upstream branch loop proves return at the requested step budget. */
  unsigned char spin[] = {0xeb,0xfe};
  /* mov rax,[0] uses upstream memory translation and fault handling. */
  unsigned char fault[] = {0x48,0xa1,0,0,0,0,0,0,0,0};
  InitMap();
  InitBus();
  printf("abi Machine=%zu System=%zu ax=%zu ip=%zu flags=%zu onhalt=%zu\n",
         sizeof(struct Machine), sizeof(struct System), offsetof(struct Machine, ax),
         offsetof(struct Machine, ip), offsetof(struct Machine, flags),
         offsetof(struct Machine, onhalt));
  for (int repeat = 0; repeat < 2; ++repeat) {
    if (RunCase("arithmetic", arithmetic, sizeof(arithmetic), 4, 0, 42, 43, 0, 0x40000e)) return 1;
    if (RunCase("undefined", arithmetic, sizeof(arithmetic), 5, kMachineUndefinedInstruction, 42, 43, 4, 0x40000e)) return 2;
    if (RunCase("budget", spin, sizeof(spin), 7, 0, 0, 0, 0, 0x400000)) return 3;
    if (RunCase("unmapped", fault, sizeof(fault), 1, kMachineSegmentationFault, 0, 0, 11, 0x400000)) return 4;
  }
  return 0;
}
