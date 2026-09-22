/* Ordinary valid buffers only; use the unchanged pinned upstream helpers. */
#include <stdio.h>
#include "blink/endian.h"

#define CHECK(condition) do { if (!(condition)) return __LINE__; } while (0)
static int pointer_calls, value_calls;
static u8 *NextPointer(u8 *pointer) { ++pointer_calls; return pointer; }
static u64 NextValue(u64 value) { ++value_calls; return value; }

static u16 (*read16)(const u8 *) = Get16;
static u32 (*read32)(const u8 *) = Get32;
static u64 (*read64)(const u8 *) = Get64;
static void (*write16)(u8 *, u16) = Put16;
static void (*write32)(u8 *, u32) = Put32;
static void (*write64)(u8 *, u64) = Put64;

static void Write(int width, int indirect, u8 *pointer, u64 value) {
  /* Conversion to the actual parameter type occurs at the C call boundary. */
  if (width == 2) {
    if (indirect) write16(NextPointer(pointer), NextValue(value));
    else Put16(NextPointer(pointer), NextValue(value));
  } else if (width == 4) {
    if (indirect) write32(NextPointer(pointer), NextValue(value));
    else Put32(NextPointer(pointer), NextValue(value));
  } else {
    if (indirect) write64(NextPointer(pointer), NextValue(value));
    else Put64(NextPointer(pointer), NextValue(value));
  }
}
static u64 Read(int width, int indirect, u8 *pointer) {
  if (width == 2) return indirect ? read16(NextPointer(pointer)) : Get16(NextPointer(pointer));
  if (width == 4) return indirect ? read32(NextPointer(pointer)) : Get32(NextPointer(pointer));
  return indirect ? read64(NextPointer(pointer)) : Get64(NextPointer(pointer));
}

int EndianProbe(void) {
  const u64 values[] = {
    0, 1, 0x7fff, 0x8000, 0xffff, 0x10000,
    0x7fffffff, 0x80000000, 0xffffffff, 0x100000000ULL,
    0x7fffffffffffffffULL, 0x8000000000000000ULL,
    0xffffffffffffffffULL, 0x0123456789abcdefULL
  };
  const int offsets[] = {0, 1, 3, 7};
  int rows = 0;
  for (int width = 2; width <= 8; width *= 2)
    for (int indirect = 0; indirect < 2; ++indirect)
      for (int alignment = 0; alignment < 4; ++alignment)
        for (int index = 0; index < 14; ++index) {
          union { u64 align; u8 bytes[32]; } storage;
          for (int i = 0; i < 32; ++i) storage.bytes[i] = 0xa5;
          int offset = 8 + offsets[alignment];
          u8 *pointer = storage.bytes + offset;
          u64 mask = width == 8 ? ~(u64)0 : ((u64)1 << (width * 8)) - 1;
          u64 expected = values[index] & mask;
          pointer_calls = value_calls = 0;
          Write(width, indirect, pointer, values[index]);
          CHECK(pointer_calls == 1 && value_calls == 1);
          for (int i = 0; i < 32; ++i) {
            u8 byte = i >= offset && i < offset + width
                ? expected >> ((i - offset) * 8) : 0xa5;
            CHECK(storage.bytes[i] == byte);
          }
          CHECK(Read(width, indirect, pointer) == expected);
          CHECK(pointer_calls == 2 && value_calls == 1);
          pointer[0] ^= 0x5a;
          u64 changed = Read(width, indirect, pointer);
          CHECK(changed == (expected ^ 0x5a));
          CHECK(pointer_calls == 3 && value_calls == 1);
          printf("%d %d %d %016llx %016llx ", width, indirect,
                 offsets[alignment], (unsigned long long)values[index],
                 (unsigned long long)changed);
          for (int i = 0; i < 32; ++i) printf("%02x", (unsigned)storage.bytes[i]);
          printf("\n");
          ++rows;
        }
  CHECK(rows == 336);
  printf("endian intrinsics: 336 normal rows: PASS\n");
  return 0;
}

#ifndef BLINK_MANAGED_ENDIAN
int main(void) { return EndianProbe(); }
#endif
