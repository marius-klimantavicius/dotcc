#define _GNU_SOURCE 1
#include "HostMemory.h"
#include <stdio.h>
#include <stdlib.h>
#include "blink/bus.h"
#include "blink/machine.h"
#include "blink/map.h"

#define PAGE 4096
#define CHECK(x) do { if (!(x)) { result = __LINE__; goto Cleanup; } } while (0)
void TerminateSignal(struct Machine *m, int signal, int code) {
  (void)m; (void)signal; (void)code;
  abort();
}
int main(void) {
  const i64 boundaries[] = {0x200000000000ll, 0x200040000000ll,
                            0x200000200000ll};
  struct Machine *m = 0;
  struct System *s = 0;
  int result = 0;
  CHECK(!BlinkHostMemoryBegin(1024 * 1024));
  FLAG_nolinear = true;
  InitMap(); InitBus();
  for (unsigned n = 0; n < sizeof(boundaries) / sizeof(*boundaries); ++n) {
    i64 boundary = boundaries[n], hint = boundary - PAGE * 3;
    i64 span = PAGE * 6, selected;
    unsigned char marker = 127 + n, copied = 0;
    s = NewSystem(XED_MACHINE_MODE_LONG); CHECK(s);
    m = NewMachine(s, 0); CHECK(m); g_machine = m;
    s->cr3 = AllocatePageTable(s); CHECK(s->cr3 != (u64)-1);
    CHECK(ReserveVirtual(s, boundary, PAGE, PAGE_U | PAGE_RW | PAGE_XD,
                         -1, 0, false, false) == boundary);
    CHECK(!CopyToUserWrite(m, boundary + 19, &marker, 1));
    /* A normal hint straddles a 512GiB, 1GiB or 2MiB table boundary.
     * Its first subtree is empty, but the next subtree contains our page. */
    CHECK(!IsFullyUnmapped(s, hint, span));
    CHECK(IsFullyUnmapped(s, hint, PAGE * 3));
    selected = FindVirtual(s, hint, span);
    CHECK(selected >= boundary + PAGE);
    CHECK(IsFullyUnmapped(s, selected, span));
    CHECK(ReserveVirtual(s, selected, span, PAGE_U | PAGE_RW | PAGE_XD,
                         -1, 0, false, false) == selected);
    CHECK(!CopyFromUserRead(m, &copied, boundary + 19, 1));
    CHECK(copied == marker);
    CHECK(!FreeVirtual(s, selected, span));
    CHECK(!FreeVirtual(s, boundary, PAGE));
    /* Empty intermediate tables remain owned until FreeMachine. */
    CHECK(!s->vss && s->rss == s->memstat.tables &&
          !s->memstat.reserved && !s->memstat.committed);
    FreeMachine(m); g_machine = 0; m = 0; s = 0;
  }
Cleanup:
  if (m) { FreeMachine(m); g_machine = 0; s = 0; }
  else if (s) FreeSystem(s);
  BlinkHostMemoryDisposeWorker();
  if (BlinkHostMemoryBytes() || BlinkHostMemoryMappings()) result = __LINE__;
  if (result) fprintf(stderr, "mapping hint invariants failed at line %d\n", result);
  else puts("mapping hint invariants passed: boundaries, selection, bytes, cleanup");
  return !!result;
}
