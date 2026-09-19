#ifndef BLINK_CPU_CONFORMANCE_CORPUS_H
#define BLINK_CPU_CONFORMANCE_CORPUS_H
#include <stdint.h>
#include <stddef.h>
#define CPU_PAGE 4096
#define CPU_CASES 12
struct CpuCase {
  const char *name;
  unsigned char code[16];
  unsigned length, steps, code_offset, data_offset, data_pages;
  uint64_t ax, cx, dx, flags, flag_mask;
  int fault;
};
/* RFLAGS arithmetic mask=CF|PF|AF|ZF|SF|OF. Shift AF is undefined;
 * multi-bit shift OF is undefined; IDIV flags are undefined. */
static const struct CpuCase cpu_cases[CPU_CASES] = {
 {"add-overflow", {0x48,0x83,0xc0,1},4,1,0,64,2,0x7fffffffffffffffULL,0,0,2,0x8d5,0},
 {"add-carry", {0x48,0x83,0xc0,1},4,1,0,64,2,0xffffffffffffffffULL,0,0,2,0x8d5,0},
 {"sub-borrow", {0x48,0x83,0xe8,1},4,1,0,64,2,0,0,0,2,0x8d5,0},
 {"shift-masked-zero", {0x48,0xd3,0xe0},3,1,0,64,2,0x8123456789abcdefULL,64,0,0x8d7,0x8d5,0},
 {"shift-one", {0x48,0xd3,0xe8},3,1,0,64,2,0x8123456789abcdefULL,1,0,2,0x8c5,0},
 {"shift-large", {0x48,0xd3,0xf8},3,1,0,64,2,0x8123456789abcdefULL,63,0,2,0xc5,0},
 {"idiv-signed", {0x48,0xf7,0xf9},3,1,0,64,2,0xffffffffffffffefULL,5,0xffffffffffffffffULL,2,0,0},
 {"idiv-overflow", {0x48,0xf7,0xf9},3,1,0,64,2,0x8000000000000000ULL,0xffffffffffffffffULL,0xffffffffffffffffULL,2,0,8},
 {"sse2-paddd", {0x66,0x0f,0xfe,0xc1},4,1,0,64,2,0,0,0,0x8d7,0x8d5,0},
 {"decode-page-cross", {0x48,0xb8,0xef,0xcd,0xab,0x89,0x67,0x45,0x23,0x01},10,1,4093,64,2,0,0,0,0x8d7,0x8d5,0},
 {"data-page-cross", {0x48,0x8b,0x03,0x48,0x83,0xc0,1,0x48,0x89,0x03},10,3,0,4092,2,0,0,0,2,0x8d5,0},
 {"data-page-fault", {0x48,0x8b,0x03},3,1,0,4092,1,0x12345678,0,0,2,0,11},
};
static const unsigned char cpu_xmm[32] = {
  0xff,0xff,0xff,0xff, 0xff,0xff,0xff,0x7f, 0,0,0,0x80, 1,2,3,4,
  1,0,0,0, 1,0,0,0, 0,0,0,0x80, 0xff,0xfe,0xfd,0xfc
};
static void CpuData(unsigned char *data, size_t length) {
  for(size_t i=0;i<length;++i)data[i]=(unsigned char)(i*37+11);
}
#endif
