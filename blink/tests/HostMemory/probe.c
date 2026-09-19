#include "HostMemory.h"
#include "blink/map.h"
#include "blink/flag.h"
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static void Require(int condition) { if (!condition) abort(); }
static void *Map(size_t length) {
  return Mmap(0, length, PROT_READ | PROT_WRITE,
              MAP_PRIVATE | MAP_ANONYMOUS, -1, 0, "host-memory-test");
}
/* The interpreter profile disables guest TLS; these independent host owners
 * deliberately retain real C thread storage. */
#ifdef _Thread_local
#undef _Thread_local
#endif
static _Thread_local unsigned char *worker_page;
int MemoryWorkerBegin(void) {
  if (BlinkHostMemoryBegin(1024 * 1024)) return 1;
  worker_page = Map(4096);
  if (worker_page == MAP_FAILED) return 2;
  worker_page[0] = 73; worker_page[4095] = 29;
  return Mprotect(worker_page, 4096, PROT_READ, "worker") ? 3 : 0;
}
unsigned long MemoryWorkerAddress(void) { return (unsigned long)(uintptr_t)worker_page; }
int MemoryWorkerFinish(void) {
  if (BlinkHostMemoryProtection(worker_page) != PROT_READ ||
      BlinkHostMemoryMappings() != 1 || worker_page[0] != 73 || worker_page[4095] != 29)
    return 1;
  if (Munmap(worker_page, 4096) || BlinkHostMemoryBytes() || BlinkHostMemoryEnd()) return 2;
  worker_page = 0;
  return 0;
}

int MemorySelfTest(void) {
  unsigned char *p, *slab, *growth;
  unsigned char source[64], copy[64];
  Require(BlinkHostMemoryBegin(1024 * 1024) == 0);
  InitMap();
  Require(FLAG_pagesize == 4096 && FLAG_vabits == 47 && FLAG_vaspace == UINT64_C(0x7ffffffff000));
  printf("init pagesize=%ld addressbits=%d vaspace=%llx\n", FLAG_pagesize,
         FLAG_vabits, (unsigned long long)FLAG_vaspace);
  p = Map(4097);
  Require(p != MAP_FAILED && !((uintptr_t)p & 4095));
  for (int i = 0; i < 8192; ++i) Require(p[i] == 0);
  Require(BlinkHostMemoryMappings() == 1 && BlinkHostMemoryBytes() == 8192 + 24 + 2);
  for (int i = 0; i < 64; ++i) source[i] = (unsigned char)(i * 7 + 3);
  memcpy(p + 4096 - 31, source, sizeof(source));
  memcpy(copy, p + 4096 - 31, sizeof(copy));
  Require(!memcmp(copy, source, sizeof(copy)));
  Require(!Mprotect(p, 8192, PROT_READ, "read"));
  Require(BlinkHostMemoryProtection(p) == PROT_READ && BlinkHostMemoryProtection(p+8191) == PROT_READ);
  Require(!memcmp(p + 4096 - 31, source, sizeof(source)));
  Require(!Mprotect(p+4096, 4096, PROT_READ|PROT_WRITE, "second page"));
  Require(BlinkHostMemoryProtection(p) == PROT_READ && BlinkHostMemoryProtection(p+8191) == (PROT_READ|PROT_WRITE));
  p[8191] = 31;
  Require(!Mprotect(p, 8192, PROT_READ|PROT_WRITE, "write"));
  p[0] = 19;
  growth = Map(3 * 4096);
  Require(growth != MAP_FAILED && BlinkHostMemoryMappings() == 2);
  for (int i = 0; i < 3 * 4096; ++i) Require(growth[i] == 0);
  growth[0] = 17; growth[3 * 4096-1] = 71;
  Require(p[0] == 19 && p[8191] == 31);
  Require(!Munmap(growth, 3 * 4096));
  Require(BlinkHostMemoryMappings() == 1 && BlinkHostMemoryBytes() == 8192+24+2);
  Require(!Munmap(p, 4097));
  Require(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  puts("normal aligned zeroed cross-page copy protection growth unmap");
  slab = Map(64 * 4096);
  Require(slab != MAP_FAILED);
  for (int i = 0; i < 64; ++i) {
    Require(slab[i * 4096] == 0);
    slab[i * 4096] = (unsigned char)i;
  }
  for (int i = 0; i < 64; ++i) Require(slab[i * 4096] == (unsigned char)i);
  Require(!Munmap(slab, 64 * 4096));
  Require(!BlinkHostMemoryEnd());
  Require(!MemoryWorkerBegin() && !MemoryWorkerFinish());
  Require(!BlinkHostMemoryBegin(1024 * 1024));
  Require(Map(4096) != MAP_FAILED && Map(8192) != MAP_FAILED);
  BlinkHostMemoryDisposeWorker();
  Require(!BlinkHostMemoryBytes() && !BlinkHostMemoryMappings());
  Require(!BlinkHostMemoryBegin(1024 * 1024) && !BlinkHostMemoryEnd());
  puts("normal slab64 dispose-worker reinitialize");
  return 0;
}
#ifndef BLINK_TEST_MANAGED
int main(void) { return MemorySelfTest(); }
#endif
