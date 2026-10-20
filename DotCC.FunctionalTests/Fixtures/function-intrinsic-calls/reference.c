/* Independent defined C reference, including defined zero-count behavior. */
#include <string.h>
#include "operations.h"
uint16_t read_native16(const void* p) { uint16_t v; memcpy(&v,p,2); return v; }
uint32_t read_be32(const void* p) { const unsigned char* b=p; return ((uint32_t)b[0]<<24)|((uint32_t)b[1]<<16)|((uint32_t)b[2]<<8)|b[3]; }
uint64_t read_le64(const void* p) { const unsigned char* b=p; uint64_t r=0; unsigned i; for(i=0;i<8;++i) r|=(uint64_t)b[i]<<(8*i); return r; }
void write_le16(void* p,uint16_t v) { unsigned char* b=p; b[0]=(unsigned char)v; b[1]=(unsigned char)(v>>8); }
void write_be32(void* p,uint32_t v) { unsigned char* b=p; unsigned i; for(i=0;i<4;++i) b[i]=(unsigned char)(v>>(8*(3-i))); }
void write_native64(void* p,uint64_t v) { memcpy(p,&v,8); }
uint32_t swap32(uint32_t v) { return (v>>24)|((v>>8)&0xff00)|((v<<8)&0xff0000)|(v<<24); }
uint64_t swap64(uint64_t v) { return ((uint64_t)swap32((uint32_t)v)<<32)|swap32((uint32_t)(v>>32)); }
unsigned clz32(uint32_t v) { unsigned n=0; if(!v) return 32; while(!(v&0x80000000U)) { ++n; v<<=1; } return n; }
unsigned ctz32(uint32_t v) { unsigned n=0; if(!v) return 32; while(!(v&1)) { ++n; v>>=1; } return n; }
unsigned clz64(uint64_t v) { unsigned n=0; if(!v) return 64; while(!(v&0x8000000000000000ULL)) { ++n; v<<=1; } return n; }
unsigned ctz64(uint64_t v) { unsigned n=0; if(!v) return 64; while(!(v&1)) { ++n; v>>=1; } return n; }
unsigned highbit(uint32_t v) { return 31U-clz32(v); }
unsigned little_unsigned(void) { const unsigned v=1; return *(const unsigned char*)&v; }
int little_signed(void) { return (int)little_unsigned(); }
unsigned commonbytes(uint64_t v) { return (little_unsigned()?ctz64(v):clz64(v))>>3; }
#ifndef INTRINSIC_NATIVE_MAIN
#include "selected.h"
#endif
