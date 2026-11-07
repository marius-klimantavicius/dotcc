#include "HostMemory.h"
#include <assert.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <sys/mman.h>

/* Real C owner lifetime/accounting control. Guest page-pool retirement is a
 * separate managed-owner contract, not implied by these allocation assertions. */
int main(void) {
  uint64_t token = BlinkHostMemoryCreateShared(8 * 1024 * 1024);
  assert(token && !BlinkHostMemoryAttach(token));
  size_t baseline = BlinkHostMemoryBytes();
  unsigned char *page = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE,
                                       MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  assert(page != MAP_FAILED);
  memset(page, 0x5a, 4096);
  size_t live = BlinkHostMemoryBytes();
  assert(live > baseline);
  assert(!BlinkHostMemoryInstructionReadBegin());
  assert(!BlinkHostMemoryInstructionReadBegin());
  assert(!blink_host_munmap(page, 4096));
  assert(!BlinkHostMemoryContains(page, 4096));
  assert(BlinkHostMemoryBytes() == live && BlinkHostMemoryMappings() == 1);
  for (int i = 0; i < 4096; ++i) assert(page[i] == 0x5a);
  assert(BlinkHostMemoryDetach() == -1);
  assert(!BlinkHostMemoryInstructionReadEnd());
  assert(BlinkHostMemoryBytes() == live);
  assert(!BlinkHostMemoryInstructionReadEnd());
  assert(BlinkHostMemoryBytes() == baseline && BlinkHostMemoryMappings() == 0);
  assert(BlinkHostMemoryInstructionReadEnd() == -1);
  assert(!BlinkHostMemoryDetach());
  assert(!BlinkHostMemoryDestroyShared(token));
  puts("PASS instruction read pin: deferred lifetime, nested readers, accounting and teardown");
}
