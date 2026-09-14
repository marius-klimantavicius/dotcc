/* Native oracle for the unchanged upstream ELF loader and initial guest stack.
 * This probe does not execute guest instructions or provide a product fallback.
 */
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
void TerminateSignal(struct Machine *m, int sig, int code) {
  (void)m; (void)sig; (void)code;
  Check(0);
}
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
int main(int argc, char **argv) {
  Check(argc == 2);
  InitMap();
  InitBus();
  /* Same owning initialization required by the upstream frontend. Absolute
   * paths consult this table even when no overlay remapping is requested. */
  Check(!SetOverlays("", false));
  for (int repeat = 0; repeat < 2; ++repeat) {
    struct System *s = NewSystem(XED_MACHINE_MODE_LONG);
    Check(s != 0);
    struct Machine *m = NewMachine(s, 0);
    Check(m != 0);
    g_machine = m;
    s->trapexit = true;
    char arg0[] = "service-probe", arg1[] = "--fixture", arg2[] = "value with spaces";
    char env0[] = "BLINK_FIXTURE=loader", env1[] = "EMPTY=", execfn[] = "fixture-exec-name";
    char *args[] = {arg0, arg1, arg2, 0};
    char *vars[] = {env0, env1, 0};
    LoadProgram(m, execfn, argv[1], args, vars, 0);
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
