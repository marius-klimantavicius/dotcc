#ifndef BLINK_CPU_CONFORMANCE_CORPUS_H
#define BLINK_CPU_CONFORMANCE_CORPUS_H
#include <stdint.h>
#include <stddef.h>
#define CPU_PAGE 4096
#define CPU_CODE_BYTES 32
struct CpuCase {
  const char *name;
  unsigned char code[CPU_CODE_BYTES];
  unsigned length, steps, code_offset, data_offset, data_pages;
  uint64_t ax, cx, dx, flags, flag_mask;
  int fault;
  unsigned xmm_preset, mxcsr;
  int profile_reference;
  int mxcsr_present, expected_halt, fault_state;
  unsigned raw_size;
  uint64_t raw_x, raw_y;
};
/* RFLAGS arithmetic mask=CF|PF|AF|ZF|SF|OF. Shift AF is undefined;
 * multi-bit shift OF is undefined; IDIV flags are undefined. */
static const struct CpuCase cpu_cases[] = {
 {"add-overflow", {0x48,0x83,0xc0,1},4,1,0,64,2,0x7fffffffffffffffULL,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"add-carry", {0x48,0x83,0xc0,1},4,1,0,64,2,0xffffffffffffffffULL,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"sub-borrow", {0x48,0x83,0xe8,1},4,1,0,64,2,0,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"shift-masked-zero", {0x48,0xd3,0xe0},3,1,0,64,2,0x8123456789abcdefULL,64,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"shift-one", {0x48,0xd3,0xe8},3,1,0,64,2,0x8123456789abcdefULL,1,0,2,0x8c5,0,0,0,0,0,0,0,0,0,0},
 {"shift-large", {0x48,0xd3,0xf8},3,1,0,64,2,0x8123456789abcdefULL,63,0,2,0xc5,0,0,0,0,0,0,0,0,0,0},
 {"idiv-signed", {0x48,0xf7,0xf9},3,1,0,64,2,0xffffffffffffffefULL,5,0xffffffffffffffffULL,2,0,0,0,0,0,0,0,0,0,0,0},
 {"idiv-overflow", {0x48,0xf7,0xf9},3,1,0,64,2,0x8000000000000000ULL,0xffffffffffffffffULL,0xffffffffffffffffULL,2,0,8,0,0,0,0,0,0,0,0,0},
 {"sse2-paddd", {0x66,0x0f,0xfe,0xc1},4,1,0,64,2,0,0,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"decode-page-cross", {0x48,0xb8,0xef,0xcd,0xab,0x89,0x67,0x45,0x23,0x01},10,1,4093,64,2,0,0,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"data-page-cross", {0x48,0x8b,0x03,0x48,0x83,0xc0,1,0x48,0x89,0x03},10,3,0,4092,2,0,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"data-page-fault", {0x48,0x8b,0x03},3,1,0,4092,1,0x12345678,0,0,2,0,11,0,0,0,0,0,0,0,0,0},
 {"adc-byte-carry", {0x14,0xff},2,1,0,64,2,0x123456781234567fULL,0,0,3,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"sbb-dword-zeroextend", {0x83,0xd8,0},3,1,0,64,2,0x1234567800000000ULL,0,0,3,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"imul-qword-overflow", {0x48,0x0f,0xaf,0xc1},4,1,0,64,2,0x4000000000000000ULL,4,0,2,0x801,0,0,0,0,0,0,0,0,0,0},
 {"cmov-zero", {0x48,0x0f,0x44,0xc1},4,1,0,64,2,11,22,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"sse2-pxor", {0x66,0x0f,0xef,0xc0},4,1,0,64,2,0,0,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"addsd-negative-zero", {0xf2,0x0f,0x58,0xc1},4,1,0,64,2,0,0,0,0x8d7,0x8d5,0,1,0,0,0,0,0,0,0,0},
 {"addps-exact-lanes", {0x0f,0x58,0xc1},3,1,0,64,2,0,0,0,0x8d7,0x8d5,0,2,0,0,0,0,0,0,0,0},
 {"ucomisd-quiet-nan", {0x66,0x0f,0x2e,0xc1},4,1,0,64,2,0,0,0,0x8d7,0x8d5,0,3,0,0,0,0,0,0,0,0},
 {"cvtsd-nearest-even", {0xf2,0x48,0x0f,0x2d,0xc0},5,1,0,64,2,0,0,0,0x8d7,0x8d5,0,4,0,0,0,0,0,0,0,0},
 {"cvtsd-round-up", {0xf2,0x48,0x0f,0x2d,0xc0},5,1,0,64,2,0,0,0,0x8d7,0x8d5,0,4,0x5f80,0,0,0,0,0,0,0},
 {"cvttsd-negative", {0xf2,0x48,0x0f,0x2c,0xc0},5,1,0,64,2,0,0,0,0x8d7,0x8d5,0,5,0x5f80,0,0,0,0,0,0,0},
 {"cvtss-round-up", {0xf3,0x48,0x0f,0x2d,0xc0},5,1,0,64,2,0,0,0,0x8d7,0x8d5,0,6,0x5f80,0,0,0,0,0,0,0},
 {"cpuid-basic", {0x0f,0xa2},2,1,0,64,2,0,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-features", {0x0f,0xa2},2,1,0,64,2,1,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-structured", {0x0f,0xa2},2,1,0,64,2,7,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-extended-max", {0x0f,0xa2},2,1,0,64,2,0x80000000,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-extended-features", {0x0f,0xa2},2,1,0,64,2,0x80000001,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-invariant-tsc", {0x0f,0xa2},2,1,0,64,2,0x80000007,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-hypervisor", {0x0f,0xa2},2,1,0,64,2,0x40000000,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-thermal-power", {0x0f,0xa2},2,1,0,64,2,0x6,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-structured-unknown", {0x0f,0xa2},2,1,0,64,2,0x7,1,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-unknown", {0x0f,0xa2},2,1,0,64,2,0x12345678,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
 {"cpuid-extended-unknown", {0x0f,0xa2},2,1,0,64,2,0x80000008,0,0,0x8d7,0x8d5,0,0,0,1,0,0,0,0,0,0},
#include "fp-cases.h"
 /* Appended normal cases start at stable ID495; earlier IDs/bytes are unchanged. */
 {"inc-qword-preserve-carry", {0x48,0xff,0xc0},3,1,0,64,2,0x7fffffffffffffffULL,0,0,3,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"inc-dword-preserve-clear-carry", {0xff,0xc0},2,1,0,64,2,0x12345678ffffffffULL,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"dec-byte-preserve-clear-carry", {0xfe,0xc8},2,1,0,64,2,0x1234567812345600ULL,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"dec-word-preserve-carry", {0x66,0xff,0xc8},3,1,0,64,2,0x1234567800008000ULL,0,0,3,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"shl-byte-count-zero", {0xd2,0xe0},2,1,0,64,2,0x1234567812345680ULL,0,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"shl-byte-count-one", {0xd2,0xe0},2,1,0,64,2,0x1234567812345680ULL,1,0,2,0x8c5,0,0,0,0,0,0,0,0,0,0},
 {"shr-word-count-one", {0x66,0xd3,0xe8},3,1,0,64,2,0x1234567812348001ULL,1,0,2,0x8c5,0,0,0,0,0,0,0,0,0,0},
 {"sar-dword-count31", {0xd3,0xf8},2,1,0,64,2,0x1234567880000000ULL,31,0,2,0xc5,0,0,0,0,0,0,0,0,0,0},
 {"shl-word-masked-count-zero", {0x66,0xd3,0xe0},3,1,0,64,2,0x1234567812348001ULL,32,0,0x8d7,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"shl-dword-masked-count-one", {0xd3,0xe0},2,1,0,64,2,0x1234567880000001ULL,33,0,2,0x8c5,0,0,0,0,0,0,0,0,0,0},
 {"div-byte-valid", {0xf6,0xf1},2,1,0,64,2,0x12345678123404d2ULL,5,0x12345678ffffffffULL,2,0,0,0,0,0,0,0,0,0,0,0},
 {"div-dword-valid", {0xf7,0xf1},2,1,0,64,2,0x1122334400000000ULL,3,0xaabbccdd00000001ULL,2,0,0,0,0,0,0,0,0,0,0,0},
 {"idiv-word-valid", {0x66,0xf7,0xf9},3,1,0,64,2,0x11223344aabbffefULL,5,0x88776655ccddffffULL,2,0,0,0,0,0,0,0,0,0,0,0},
 {"idiv-dword-valid", {0xf7,0xf9},2,1,0,64,2,0x11223344ffffffefULL,5,0x88776655ffffffffULL,2,0,0,0,0,0,0,0,0,0,0,0},
 /* RDI receives the data address before EBX becomes the fixed replacement.
  * CpuData bytes64..71 are 4b7095badf04294e; only ZF is compared here. */
 {"cmpxchg8b-match", {0x48,0x89,0xdf,0xbb,0x10,0x32,0x54,0x76,0x0f,0xc7,0x0f},11,3,0,64,2,0x11223344ba95704bULL,0xfedcba98ULL,0x556677884e2904dfULL,0x897,0x40,0,0,0,0,0,0,0,0,0,0},
 {"cmpxchg8b-nonmatch", {0x48,0x89,0xdf,0xbb,0x10,0x32,0x54,0x76,0x0f,0xc7,0x0f},11,3,0,64,2,0x11223344ba95704aULL,0xfedcba98ULL,0x556677884e2904dfULL,0x8d7,0x40,0,0,0,0,0,0,0,0,0,0},
 /* FXSAVE; PXOR XMM0/1; LDMXCSR [RBX+28]; FXRSTOR; MOV RDI,RBX;
  * XOR EAX,EAX; MOV ECX,64; REP STOSQ. The saved MXCSR_MASK (zero is valid)
  * temporarily changes MXCSR using only writable bits; no FP arithmetic runs
  * before restoration. Clear all512 saved bytes before comparing memory:
  * reserved/x87/vendor-specific image bytes are not architectural equality. */
 {"fxsave-fxrstor-xmm-mxcsr", {0x0f,0xae,0x03,0x66,0x0f,0xef,0xc0,0x66,0x0f,0xef,0xc9,0x0f,0xae,0x53,0x1c,0x0f,0xae,0x0b,0x48,0x89,0xdf,0x31,0xc0,0xb9,0x40,0,0,0,0xf3,0x48,0xab},31,9,0,64,2,0,0,0,2,0x8c5,0,0,0x5f80,0,0,0,0,0,0,0},
 /* IDs512/513 cover the same repaired auxiliary-carry formula at8/16 bits. */
 {"inc-byte-wrap-preserve-carry", {0xfe,0xc0},2,1,0,64,2,0x12345678123456ffULL,0,0,3,0x8d5,0,0,0,0,0,0,0,0,0,0},
 {"inc-word-overflow-preserve-clear-carry", {0x66,0xff,0xc0},3,1,0,64,2,0x1234567800007fffULL,0,0,2,0x8d5,0,0,0,0,0,0,0,0,0,0},
};
#define CPU_CASES (sizeof(cpu_cases)/sizeof(cpu_cases[0]))
static const unsigned char cpu_xmm[32] = {
  0xff,0xff,0xff,0xff, 0xff,0xff,0xff,0x7f, 0,0,0,0x80, 1,2,3,4,
  1,0,0,0, 1,0,0,0, 0,0,0,0x80, 0xff,0xfe,0xfd,0xfc
};
static void CpuBits(unsigned char *p,uint64_t bits,unsigned size) {
  for(unsigned i=0;i<size;++i)p[i]=(unsigned char)(bits>>(i*8));
}
static unsigned CpuMxcsr(const struct CpuCase *c) { return (c->mxcsr_present || c->mxcsr)?c->mxcsr:0x1f80; }
static void CpuXmm(const struct CpuCase *c,unsigned char *x) {
  for(unsigned i=0;i<32;++i)x[i]=cpu_xmm[i];
  if(c->raw_size){CpuBits(x,c->raw_x,c->raw_size);CpuBits(x+16,c->raw_y,c->raw_size);return;}
  switch(c->xmm_preset) {
    case 1:CpuBits(x,0x8000000000000000ULL,8);CpuBits(x+16,0x8000000000000000ULL,8);break;
    case 2:
      CpuBits(x,0x3fc00000,4);CpuBits(x+4,0xc0000000,4);CpuBits(x+8,0,4);CpuBits(x+12,0x80000000,4);
      CpuBits(x+16,0x40100000,4);CpuBits(x+20,0x3f800000,4);CpuBits(x+24,0,4);CpuBits(x+28,0x80000000,4);break;
    case 3:CpuBits(x,0x7ff8000000000123ULL,8);CpuBits(x+16,0x3ff0000000000000ULL,8);break;
    case 4:CpuBits(x,0x4004000000000000ULL,8);break;
    case 5:CpuBits(x,0xc004000000000000ULL,8);break;
    case 6:CpuBits(x,0x40200000,4);break;
  }
}
static void CpuData(unsigned char *data, size_t length) {
  for(size_t i=0;i<length;++i)data[i]=(unsigned char)(i*37+11);
}
#endif
