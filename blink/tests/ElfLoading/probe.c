/* Native oracle for the unchanged upstream ELF loader and initial guest stack.
 * This probe does not execute guest instructions or provide a product fallback.
 */
#ifdef BLINK_ELF_MANAGED
#include "host-bindings.h"
#include "HostMemory.h"
#include "HostFileMapping.h"
#include "HostSignalActions.h"
#include "HostExitCallbacks.h"
#include "GuestResources.h"
#endif
#include <sys/mman.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "blink/bus.h"
#include "blink/endian.h"
#include "blink/linux.h"
#include "blink/loader.h"
#include "blink/map.h"
#include "blink/overlays.h"

static void Check(int good) {
  if (!good) { fputs("loader seam invariant failed\n", stderr); exit(1); }
}
#ifndef BLINK_ELF_MANAGED
void TerminateSignal(struct Machine *m, int sig, int code) {
  (void)m; (void)sig; (void)code;
  Check(0);
}
#endif
static unsigned long Word(struct Machine *m, unsigned long address) {
  unsigned char bytes[8];
  Check(!CopyFromUser(m, bytes, address, sizeof(bytes)));
  return Read64(bytes);
}
static void String(struct Machine *m, unsigned long address, const char *expected) {
  unsigned char bytes[256];
  size_t length = strlen(expected) + 1;
  Check(length <= sizeof(bytes));
  Check(!CopyFromUser(m, bytes, address, length));
  Check(!memcmp(bytes, expected, length));
}
struct ExpectedSegment { unsigned long address, file_size, memory_size, digest; int protection; };
/* EXPECTED_SEGMENTS */
static unsigned long Digest(struct Machine *m, unsigned long address, unsigned long length, int zero) {
  unsigned char bytes[4096];
  unsigned long hash = 14695981039346656037UL;
  while (length) {
    size_t count = length < sizeof(bytes) ? length : sizeof(bytes);
    Check(!CopyFromUser(m, bytes, address, count));
    for (size_t i = 0; i < count; ++i) {
      if (zero) Check(bytes[i] == 0);
      hash = (hash ^ bytes[i]) * 1099511628211UL;
    }
    length -= count; address += count;
  }
  return hash;
}
static int LoadValidImage(char *path) {
  InitMap();
  InitBus();
  /* Same owning initialization required by the upstream frontend. Absolute
   * paths consult this table even when no overlay remapping is requested. */
  Check(!SetOverlays("", false));
  for (int repeat = 0; repeat < 2; ++repeat) {
    struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
    Check(s != 0);
#ifdef BLINK_ELF_MANAGED
    Check(!BlinkHostInitializeBoundResourceLimits(s));
#endif
    struct Machine *m = NewMachine(s, 0);
    Check(m != 0);
    g_machine = m;
    s->trapexit = true;
    char arg0[] = "service-probe";
    char arg1[] = "--fixture";
    char arg2[] = "value with spaces";
    char env0[] = "BLINK_FIXTURE=loader";
    char env1[] = "EMPTY=";
    char execfn[] = "fixture-exec-name";
    char *args[] = {arg0, arg1, arg2, 0};
    char *vars[] = {env0, env1, 0};
    LoadProgram(m, execfn, path, args, vars, 0);
    Check(s->loaded && !s->exited && !s->elf.interpreter);
    Check(s->elf.aslr == 0 && s->elf.at_entry == m->ip);
    unsigned long stack = Read64(m->sp), cursor = stack;
    Check(!(stack & 15) && Word(m, cursor) == 3);
    cursor += 8;
    for (int i = 0; i < 3; ++i, cursor += 8) String(m, Word(m, cursor), args[i]);
    Check(Word(m, cursor) == 0); cursor += 8;
    for (int i = 0; i < 2; ++i, cursor += 8) String(m, Word(m, cursor), vars[i]);
    Check(Word(m, cursor) == 0); cursor += 8;
    unsigned seen = 0, count = 0;
    for (; count < 32; ++count, cursor += 16) {
      unsigned long kind = Word(m, cursor), value = Word(m, cursor + 8);
      if (!kind) { Check(!value); break; }
      switch (kind) {
        case AT_ENTRY_LINUX: Check(value == m->ip); seen |= 1; break;
        case AT_PHDR_LINUX: Check(value == s->elf.at_phdr); seen |= 2; break;
        case AT_PHENT_LINUX: Check(value == 56); seen |= 4; break;
        case AT_PHNUM_LINUX: Check(value == s->elf.at_phnum && value > 0); seen |= 8; break;
        case AT_PAGESZ_LINUX: Check(value == 4096); seen |= 16; break;
        case AT_EXECFN_LINUX: String(m, value, "fixture-exec-name"); seen |= 32; break;
        case AT_RANDOM_LINUX: {
          unsigned char random[16];
          Check(!CopyFromUser(m, random, value, sizeof(random)));
          Check(!memcmp(random, s->elf.rng, sizeof(random))); seen |= 64; break;
        }
      }
    }
    Check(count < 32 && seen == 127);
    Check(m->ip == 0x4015c4 && s->elf.at_phdr == 0x400040 && s->elf.at_phnum == 6);
    unsigned long files = 0, bss = 0, permissions = 0;
    for (size_t i = 0; i < sizeof(expected_segments)/sizeof(expected_segments[0]); ++i) {
      const struct ExpectedSegment *segment = expected_segments+i;
      Check(Digest(m, segment->address, segment->file_size, 0) == segment->digest);
      Digest(m, segment->address+segment->file_size, segment->memory_size-segment->file_size, 1);
      files += segment->file_size; bss += segment->memory_size-segment->file_size;
      for (unsigned long offset=0; offset<segment->memory_size;) {
        unsigned long address=segment->address+offset;
        Check(!!IsValidMemory(m,address,1,PROT_READ)==!!(segment->protection&PROT_READ));
        Check(!!IsValidMemory(m,address,1,PROT_WRITE)==!!(segment->protection&PROT_WRITE));
        Check(!!IsValidMemory(m,address,1,PROT_EXEC)==!!(segment->protection&PROT_EXEC));
        permissions+=3; offset+=4096-(address&4095);
      }
    }
    Check(IsValidMemory(m,stack,1,PROT_READ|PROT_WRITE));
    Check(!IsValidMemory(m,stack,1,PROT_EXEC));
    printf("image repeat=%d file_bytes=%lu bss_zero_bytes=%lu permission_checks=%lu stack_rw_nx=1\n",repeat,files,bss,permissions);
    unsigned char entry[16];
    Check(!CopyFromUser(m, entry, m->ip, sizeof(entry)));
    printf("loader repeat=%d entry=%lx phdr=%lx phnum=%lu argc=3 envc=2 auxc=%u stack_aligned=1 random_matches=1 entry_bytes=",
           repeat, (unsigned long)m->ip, (unsigned long)s->elf.at_phdr,
           (unsigned long)s->elf.at_phnum, count);
    for (int i = 0; i < 16; ++i) printf("%02x", entry[i]);
    putchar('\n');
    FreeMachine(m);
  }
  return 0;
}

#ifdef BLINK_ELF_MANAGED
static int loader_has_run;
int ElfLoadingRun(void) {
  if (loader_has_run) return 70;
  loader_has_run=1;
  if (BlinkHostMemoryBegin(64*1024*1024)) return 71;
  if (BlinkHostSignalActionsBegin()) { BlinkHostMemoryDisposeWorker(); return 72; }
  if (BlinkHostExitCallbacksBegin()) { BlinkHostSignalActionsEnd(); BlinkHostMemoryDisposeWorker(); return 73; }
  if (BlinkHostMemoryEnablePrivateFiles()) { BlinkHostExitCallbacksEnd(); BlinkHostSignalActionsEnd(); BlinkHostMemoryDisposeWorker(); return 74; }
  char path[]="/bin/service";
  int result=LoadValidImage(path);
  if (BlinkHostExitCallbacksRun() && !result) result=75;
  BlinkHostExitCallbacksEnd(); BlinkHostSignalActionsEnd();
  BlinkHostMemoryDisposeWorker();
  return result;
}
#else
int main(int argc,char **argv) { Check(argc==2); return LoadValidImage(argv[1]); }
#endif
