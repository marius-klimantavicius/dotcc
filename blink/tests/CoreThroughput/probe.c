/* A fixed hot integer loop through the real interpreter, never host execution. */
#ifdef THROUGHPUT_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/flags.h"
#include "blink/machine.h"
#include "blink/map.h"
#include "blink/signal.h"
#ifndef DISABLE_JIT
#error Throughput reference requires interpreter-only execution
#endif
#ifndef NOLINEAR
#error Throughput reference requires nonlinear guest memory
#endif

#define CODE_ADDRESS 0x400000UL
#define DATA_ADDRESS 0x600000UL
static const unsigned char loop_code[] = {
  0x48,0xff,0xc0,             /* inc rax */
  0x48,0x01,0x03,             /* add qword [rbx], rax */
  0x48,0xff,0xc9,             /* dec rcx */
  0x75,0xf5                   /* jnz start */
};
static struct Machine *machine;
static int used, signaled, memory_owner, signal_owner, exit_owner;
#ifdef THROUGHPUT_MANAGED
extern unsigned long ThroughputNow(void);
extern unsigned long ThroughputFrequency(void);
#else
unsigned long ThroughputNow(void) {
  struct timespec value;
  if (clock_gettime(CLOCK_MONOTONIC, &value)) abort();
  return (unsigned long)value.tv_sec * 1000000000UL + value.tv_nsec;
}
unsigned long ThroughputFrequency(void) { return 1000000000UL; }
#endif
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code; signaled = 1;
}
static unsigned long RegisterValue(unsigned index, unsigned iterations, int final) {
  if (index == 0) return final ? iterations : 0;
  if (index == 1) return final ? 0 : iterations;
  if (index == 3) return DATA_ADDRESS + 8;
  return 0x100UL + index;
}
static void Data(unsigned char *bytes, unsigned iterations) {
  memset(bytes, 0xa5, 64);
  Write64(bytes + 8, (unsigned long)iterations * (iterations + 1UL) / 2);
}
static int Prepare(unsigned iterations) {
  unsigned char data[64]; Data(data, 0);
  if (CopyToUser(machine, DATA_ADDRESS, data, sizeof(data))) return 31;
  for (unsigned i = 0; i < 16; ++i) Write64(machine->weg[i], RegisterValue(i, iterations, 0));
  memset(machine->xmm, 0, sizeof(machine->xmm));
  machine->mxcsr = 0x1f80; machine->ip = CODE_ADDRESS; ImportFlags(machine, 2);
  signaled = 0;
  return 0;
}
static int Verify(unsigned iterations) {
  unsigned char expected[64]; unsigned char actual[64]; unsigned char code[11];
  Data(expected, iterations);
  if (signaled || machine->ip != CODE_ADDRESS + sizeof(loop_code)) return 40;
  if ((ExportFlags(machine->flags) & 0x8d5) != 0x44 || !(ExportFlags(machine->flags) & 2)) return 41;
  for (unsigned i = 0; i < 16; ++i)
    if (Read64(machine->weg[i]) != RegisterValue(i, iterations, 1)) return 42;
  if (CopyFromUser(machine, actual, DATA_ADDRESS, sizeof(actual)) || memcmp(actual, expected, sizeof(actual))) return 43;
  if (CopyFromUser(machine, code, CODE_ADDRESS, sizeof(code)) || memcmp(code, loop_code, sizeof(code))) return 44;
  for (unsigned i = 0; i < 16; ++i)
    for (unsigned j = 0; j < 16; ++j) if (machine->xmm[i][j]) return 45;
  if (machine->mxcsr != 0x1f80) return 46;
  return 0;
}
static int Batch(unsigned iterations, int sample) {
  int result = Prepare(iterations); if (result) return result;
  unsigned long instructions = (unsigned long)iterations * 4;
  unsigned long start = 0, end = 0;
  int halt = sigsetjmp(machine->onhalt, 1);
  if (!halt) {
    machine->canhalt = true;
    start = ThroughputNow();
    for (unsigned long step = 0; step < instructions; ++step) ExecuteInstruction(machine);
    end = ThroughputNow();
  }
  machine->canhalt = false;
  if (halt) return 47;
  result = Verify(iterations); if (result) return result;
  if (end <= start) return 48;
  if (sample >= 0) {
    printf("{\"sample\":%d,\"iterations\":%u,\"instructions\":%lu,\"ticks\":%lu,\"frequency\":%lu,\"ax\":%lu,\"cx\":%lu,\"ip\":%lu,\"sum\":%lu,\"flags\":%u,\"verified\":true}\n",
           sample, iterations, instructions, end - start, ThroughputFrequency(),
           Read64(machine->ax), Read64(machine->cx), machine->ip,
           (unsigned long)iterations * (iterations + 1UL) / 2,
           ExportFlags(machine->flags) & 0x8d5);
  }
  return 0;
}
/* Idempotent release also permits a managed finally to release private storage
 * if a fatal callback escapes normal cleanup. No call may follow final release. */
void ThroughputDestroy(void) {
  if (machine) { struct Machine *value = machine; machine = 0; FreeMachine(value); }
}
void ThroughputRelease(void) {
#ifdef THROUGHPUT_MANAGED
  if (exit_owner) { exit_owner = 0; BlinkHostExitCallbacksEnd(); }
  if (signal_owner) { signal_owner = 0; BlinkHostSignalActionsEnd(); }
  if (memory_owner) { memory_owner = 0; BlinkHostMemoryDisposeWorker(); }
#endif
}
int ThroughputMain(unsigned iterations, unsigned samples) {
  if (used || iterations < 1 || iterations > 1000000 || samples < 1 || samples > 20) return 2;
  used = 1;
#ifdef THROUGHPUT_MANAGED
  if (BlinkHostMemoryBegin(64 * 1024 * 1024)) return 20;
  memory_owner = 1;
  if (BlinkHostSignalActionsBegin()) return 21;
  signal_owner = 1;
  if (BlinkHostExitCallbacksBegin()) return 22;
  exit_owner = 1;
  if (BlinkHostMemoryEnablePrivateFiles()) return 23;
#endif
  InitMap(); InitBus();
  struct System *system = NewSystem(XED_MACHINE_MODE_LONG);
  if (!system) return 24;
#ifdef THROUGHPUT_MANAGED
  if (BlinkHostInitializeBoundResourceLimits(system)) { FreeSystem(system); return 25; }
#endif
  machine = NewMachine(system, 0);
  if (!machine) { FreeSystem(system); return 26; }
  g_machine = machine;
  system->cr0 = CR0_PE | CR0_MP | CR0_ET | CR0_PG;
  system->cr3 = AllocatePageTable(system);
  if (ReserveVirtual(system, CODE_ADDRESS, 4096, PAGE_U | PAGE_RW, -1, 0, 0, 0) == -1 ||
      ReserveVirtual(system, DATA_ADDRESS, 4096, PAGE_U | PAGE_RW | PAGE_XD, -1, 0, 0, 0) == -1 ||
      CopyToUser(machine, CODE_ADDRESS, loop_code, sizeof(loop_code))) return 27;
  int result = Batch(10000, -1); /* Fixed untimed warmup; never tuned to a score. */
  for (unsigned sample = 0; sample < samples && !result; ++sample) result = Batch(iterations, sample);
  struct Machine *value = machine; machine = 0; FreeMachine(value);
#ifdef THROUGHPUT_MANAGED
  if (BlinkHostExitCallbacksRun() && !result) result = 28;
#endif
  return result;
}
#ifndef THROUGHPUT_MANAGED
int main(int argc, char **argv) {
  if (argc != 3) return 2;
  int result = ThroughputMain((unsigned)strtoul(argv[1], 0, 10), (unsigned)strtoul(argv[2], 0, 10));
  ThroughputDestroy(); ThroughputRelease(); return result;
}
#endif
