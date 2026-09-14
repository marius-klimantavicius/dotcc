#include <stdio.h>
#include <stdlib.h>
#include "blink/endian.h"
#include "blink/machine.h"

/* Header inline helpers retain these symbols in translated output. This
 * CPUID-only fixture never invokes mapping helpers or initializes mappings. */
bool FLAG_nolinear = true;
struct HostPages g_hostpages;

/* A strict test link for an unexercised path, not a product fault handler. */
_Noreturn void ThrowSegmentationFault(struct Machine *m, i64 address) {
  (void)m; (void)address; abort();
}
static void Query(struct Machine *m, u32 leaf, u32 subleaf) {
  Put64(m->ax, leaf);
  Put64(m->cx, subleaf);
  Put64(m->bx, -1);
  Put64(m->dx, -1);
  OpCpuid(m, 0, 0, 0);
  if ((Get64(m->ax) | Get64(m->bx) | Get64(m->cx) | Get64(m->dx)) >> 32) abort();
  printf("%08x %08x %08x %08x %08x %08x\n", leaf, subleaf,
         Get32(m->ax), Get32(m->bx), Get32(m->cx), Get32(m->dx));
}
int CpuidProbe(void) {
  struct System *s = calloc(1, sizeof(*s));
  struct Machine *m = calloc(1, sizeof(*m));
  if (!s || !m) abort();
  m->system = s;
  Query(m, 0, 0);
  Query(m, 1, 0);
  Query(m, 2, 0);
  for (unsigned i = 0; i != 5; ++i) Query(m, 4, i);
  Query(m, 6, 0);
  Query(m, 7, 0);
  Query(m, 7, 1);
  Query(m, 0x80000000u, 0);
  Query(m, 0x80000001u, 0);
  Query(m, 0x80000007u, 0);
  Query(m, 0x40000000u, 0);
  Query(m, 0x12345678u, 0);
  free(m);
  free(s);
  return 0;
}
#ifndef BLINK_TEST_MANAGED
int main(void) { return CpuidProbe(); }
#endif
