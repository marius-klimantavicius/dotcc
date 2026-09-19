#include "HostMemory.h"
#include "blink/map.h"
#include "blink/flag.h"
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>

static void Require(int condition) { if (!condition) abort(); }
static void *Map(size_t length) {
  return Mmap(0, length, PROT_READ | PROT_WRITE,
              MAP_PRIVATE | MAP_ANONYMOUS, -1, 0, "host-memory-test");
}
/* Upstream disables its guest-thread TLS keyword macro with DISABLE_THREADS.
 * This independent host-worker test state requires real C thread storage. */
#ifdef _Thread_local
#undef _Thread_local
#endif
static _Thread_local unsigned char *worker_page;
int MemoryWorkerBegin(void) {
  if (BlinkHostMemoryBegin(1024 * 1024)) return 1;
  worker_page = Map(4096);
  if (worker_page == MAP_FAILED) return 2;
  worker_page[0] = 73;
  worker_page[4095] = 29;
  if (Mprotect(worker_page, 4096, PROT_READ, "worker")) return 5;
  return 0;
}
unsigned long MemoryWorkerAddress(void) { return (unsigned long)(uintptr_t)worker_page; }
int MemoryRejectForeign(unsigned long address) {
  errno = 79;
  if (BlinkHostMemoryProtection((void*)(uintptr_t)address) != -1 || errno != 79) return 1;
  if (blink_host_mprotect((void*)(uintptr_t)address,4096,PROT_WRITE) != -1 || errno != ENOMEM) return 2;
  return BlinkHostMemoryProtection(worker_page) == PROT_READ ? 0 : 3;
}
int MemoryWorkerFinish(void) {
  if (BlinkHostMemoryProtection(worker_page) != PROT_READ || BlinkHostMemoryMappings() != 1 || worker_page[0] != 73 || worker_page[4095] != 29)
    return 3;
  if (Munmap(worker_page, 4096) || BlinkHostMemoryBytes() || BlinkHostMemoryEnd()) return 4;
  worker_page = 0;
  return 0;
}

int MemorySelfTest(void) {
  unsigned char *p, *slab;
  size_t live;
  Require(Map(4096) == MAP_FAILED && errno == ENODEV);
  Require(BlinkHostMemoryBegin(4096) == -1 && errno == EINVAL);
  Require(BlinkHostMemoryBegin(4096+24) == -1 && errno == EINVAL);
  Require(BlinkHostMemoryBegin(4096+24+1) == 0);
  p = Map(4096);
  Require(p != MAP_FAILED && BlinkHostMemoryBytes() == 4096+24+1);
  Require(Map(4096) == MAP_FAILED && errno == ENOMEM);
  Require(!Munmap(p,4096) && !BlinkHostMemoryBytes() && !BlinkHostMemoryEnd());
  Require(BlinkHostMemoryBegin(1024 * 1024) == 0);
  Require(BlinkHostMemoryBegin(1024 * 1024) == -1 && errno == EBUSY);
  InitMap();
  Require(FLAG_pagesize == 4096 && FLAG_vabits == 47 && FLAG_vaspace == UINT64_C(0x7ffffffff000));
  printf("init pagesize=%ld addressbits=%d vaspace=%llx\n", FLAG_pagesize,
         FLAG_vabits, (unsigned long long)FLAG_vaspace);
  p = Map(4097);
  Require(p != MAP_FAILED && !((uintptr_t)p & 4095));
  for (int i = 0; i < 8192; ++i) Require(p[i] == 0);
  p[0] = 19; p[8191] = 31;
  Require(BlinkHostMemoryMappings() == 1);
  live = BlinkHostMemoryBytes();
  Require(live == 8192 + 24 + 2);
  Require(BlinkHostMemoryEnd() == -1 && errno == EBUSY);
  Require(Munmap(p + 4096, 4096) == -1 && errno == EINVAL);
  Require(Munmap(p, 4096) == -1 && errno == EINVAL);
  Require(p[0] == 19 && p[8191] == 31 && BlinkHostMemoryBytes() == live);
  Require(Mprotect(p, 8192, PROT_READ, "test") == 0);
  Require(BlinkHostMemoryProtection(p) == PROT_READ && BlinkHostMemoryProtection(p+8191) == PROT_READ);
  Require(Mprotect(p+4096, 1, PROT_WRITE, "test") == 0);
  Require(BlinkHostMemoryProtection(p) == PROT_READ && BlinkHostMemoryProtection(p+8191) == PROT_WRITE);
  Require(Mprotect(p, 8193, 0, "test") == -1 && errno == ENOMEM);
  Require(Mprotect(p+1, 4096, 0, "test") == -1 && errno == EINVAL);
  Require(Mprotect(p, 0, 0, "test") == -1 && errno == EINVAL);
  Require(Mprotect(p, SIZE_MAX, 0, "test") == -1 && errno == EINVAL);
  Require(Mprotect((void*)(UINTPTR_MAX-4095), 4096, 0, "test") == -1 && errno == EINVAL);
  Require(Mprotect(p, 4096, PROT_EXEC, "test") == -1 && errno == ENOTSUP);
  Require(Mprotect(p, 4096, 8, "test") == -1 && errno == EINVAL);
  Require(BlinkHostMemoryProtection(p) == PROT_READ && BlinkHostMemoryProtection(p+8191) == PROT_WRITE);
  Require(BlinkHostMemoryBytes() == live && p[0] == 19 && p[8191] == 31);
  Require(Mprotect(p, 8192, PROT_NONE, "test") == 0);
  Require(BlinkHostMemoryProtection(p) == PROT_NONE && BlinkHostMemoryProtection(p+8191) == PROT_NONE);
  Require(Mprotect(p, 8192, PROT_READ|PROT_WRITE, "test") == 0);
  Require(Msync(p, 8192, MS_SYNC, "test") == -1 && errno == ENOTSUP);
  Require(Mmap((void *)4096, 4096, 3, MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED,
               -1, 0, "fixed") == MAP_FAILED && errno == ENOTSUP);
  Require(Mmap(0, 4096, 3, MAP_PRIVATE, 0, 0, "file") == MAP_FAILED && errno == ENOTSUP);
  Require(Mmap(0, 4096, 3, MAP_SHARED | MAP_ANONYMOUS, -1, 0, "shared") == MAP_FAILED && errno == ENOTSUP);
  Require(Map(0) == MAP_FAILED && errno == EINVAL);
  Require(Map(SIZE_MAX) == MAP_FAILED && errno == EINVAL);
  Require(Map(1024 * 1024) == MAP_FAILED && errno == ENOMEM);
  Require(BlinkHostMemoryBytes() == live);
  Require(Munmap(p, 4097) == 0);
  Require(Munmap(p, 4097) == -1 && errno == EINVAL);
  Require(BlinkHostMemoryBytes() == 0 && BlinkHostMemoryMappings() == 0);
  Require(BlinkHostMemoryProtection(p) == -1);
  Require(Mprotect(p,4096,PROT_READ,"freed") == -1 && errno == ENOMEM);
  for (int mode=0;mode<4;++mode) {
    p = Mmap(0,4096,mode,MAP_PRIVATE|MAP_ANONYMOUS,-1,0,"modes");
    Require(p != MAP_FAILED && BlinkHostMemoryProtection(p)==mode);
    Require(!Mprotect(p,4096,PROT_READ|PROT_WRITE,"initialize"));
    Require(p[0]==0 && p[4095]==0);
    Require(!Munmap(p,4096));
  }
  puts("ownership aligned zeroed exact-free partial-rejected errors-preserve-state");
  /* The exact slab size requested by unchanged AllocateAnonymousPage. */
  slab = Map(64 * 4096);
  Require(slab != MAP_FAILED);
  for (int i = 0; i < 64; ++i) {
    Require(slab[i * 4096] == 0);
    slab[i * 4096] = (unsigned char)i;
  }
  Require(Munmap(slab, 64 * 4096) == 0);
  Require(BlinkHostMemoryEnd() == 0);
  Require(MemoryWorkerBegin() == 0);
  Require(MemoryWorkerFinish() == 0);
  Require(BlinkHostMemoryBegin(1024 * 1024) == 0);
  Require(Map(4096) != MAP_FAILED && Map(8192) != MAP_FAILED);
  BlinkHostMemoryDisposeWorker();
  Require(BlinkHostMemoryBytes() == 0 && BlinkHostMemoryMappings() == 0);
  Require(Map(4096) == MAP_FAILED && errno == ENODEV);
  Require(BlinkHostMemoryBegin(1024 * 1024) == 0 && BlinkHostMemoryEnd() == 0);
  Require(blink_host_mprotect((void*)4096,4096,PROT_READ)==-1 && errno==ENODEV);
  puts("slab64 bounded dispose-worker reinitialize");
  return 0;
}
#ifndef BLINK_TEST_MANAGED
int main(void) { return MemorySelfTest(); }
#endif
