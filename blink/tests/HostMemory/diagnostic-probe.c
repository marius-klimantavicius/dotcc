#define _GNU_SOURCE 1
#include "HostMemory.h"
#include "blink/types.h"
#include "blink/bus.h"
#include "blink/x86.h"
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/mman.h>

i64 ReadWordSafely(int, u8 *);
static void Require(int condition) { if (!condition) abort(); }
static i64 Unavailable(int mode) {
  return INT64_C(0x6660666066660666) >> ((8 - (2 << mode)) * 8);
}

int DiagnosticSelfTest(void) {
  unsigned char *p;
  uint32_t byte_order = UINT32_C(0x01020304);
  Require(((unsigned char *)&byte_order)[0] == 4);
#ifdef BLINK_NATIVE_UNTOUCHED
  p = mmap(0, 8192, PROT_NONE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  Require(p != MAP_FAILED && mprotect(p, 4096, PROT_READ | PROT_WRITE) == 0);
#else
  Require(BlinkHostMemoryBegin(1024 * 1024) == 0);
  p = blink_host_mmap(0, 4096, PROT_READ | PROT_WRITE,
                       MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  Require(p != MAP_FAILED);
  errno = E2BIG;
  Require(BlinkHostMemoryContains(p, 4096));
  Require(BlinkHostMemoryContains(p + 4095, 1));
  Require(!BlinkHostMemoryContains(p, 0));
  Require(!BlinkHostMemoryContains(0, 1));
  Require(!BlinkHostMemoryContains(p + 4096, 1));
  Require(!BlinkHostMemoryContains(p + 4095, 2));
  Require(!BlinkHostMemoryContains((void *)((uintptr_t)p - 1), 2));
  Require(!BlinkHostMemoryContains((void *)(UINTPTR_MAX - 3), 8));
  Require(!BlinkHostMemoryContains(p, SIZE_MAX));
  Require(errno == E2BIG);
  unsigned char *foreign = malloc(16);
  Require(foreign != 0);
  for (int i = 0; i < 16; ++i) foreign[i] = 99;
  Require(!BlinkHostMemoryContains(foreign, 8));
  Require(ReadWordSafely(XED_MODE_LONG, foreign) == Unavailable(XED_MODE_LONG));
  free(foreign);
  Require(ReadWordSafely(-1, p) == Unavailable(XED_MODE_LONG));
#endif
  /* Check actual upstream stores against literal bytes, independently of reads. */
  Store16(p, UINT64_C(0x0201));
  Require(p[0] == 1 && p[1] == 2);
  Store16(p + 1, UINT64_C(0x0403));
  Require(p[1] == 3 && p[2] == 4);
  Store32(p, UINT64_C(0x04030201));
  for (int i = 0; i < 4; ++i) Require(p[i] == i + 1);
  Store32(p + 1, UINT64_C(0x08070605));
  for (int i = 0; i < 4; ++i) Require(p[i + 1] == i + 5);
  Store64Unlocked(p, UINT64_C(0x0807060504030201));
  for (int i = 0; i < 8; ++i) Require(p[i] == i + 1);
  Store64Unlocked(p + 1, UINT64_C(0x100f0e0d0c0b0a09));
  for (int i = 0; i < 8; ++i) Require(p[i + 1] == i + 9);
  puts("independent aligned-unaligned store bytes verified");
  for (int i = 0; i < 4096; ++i) p[i] = (unsigned char)(i + 1);
  for (int mode = XED_MODE_REAL; mode <= XED_MODE_LONG; ++mode) {
    int width = 2 << mode;
    i64 first = ReadWordSafely(mode, p);
    i64 unaligned = ReadWordSafely(mode, p + 1);
    i64 last = ReadWordSafely(mode, p + 4096 - width);
    i64 cross = ReadWordSafely(mode, p + 4097 - width);
    i64 invalid = ReadWordSafely(mode, (u8 *)1);
    i64 overflow = ReadWordSafely(mode, (u8 *)(UINTPTR_MAX - 3));
    Require(cross == Unavailable(mode) && invalid == Unavailable(mode) && overflow == Unavailable(mode));
    printf("mode=%d first=%llx unaligned=%llx last=%llx unavailable=%llx\n",
           mode, (unsigned long long)first, (unsigned long long)unaligned,
           (unsigned long long)last, (unsigned long long)invalid);
  }
#ifdef BLINK_NATIVE_UNTOUCHED
  Require(munmap(p, 8192) == 0);
#else
  Require(blink_host_munmap(p, 4096) == 0);
  Require(!BlinkHostMemoryContains(p, 1));
  Require(ReadWordSafely(XED_MODE_LONG, p) == Unavailable(XED_MODE_LONG));
  Require(BlinkHostMemoryEnd() == 0);
#endif
  return 0;
}
#ifndef BLINK_TEST_MANAGED
int main(void) { return DiagnosticSelfTest(); }
#endif
