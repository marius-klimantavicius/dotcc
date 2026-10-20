/* Test-only differential calls into unmodified zstd-1.5.7 headers.
 * Expected values below are independent byte/bit loops, never zero-UB builtins. */
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#ifdef DZ_NATIVE_MAIN
#include <sys/mman.h>
#include <unistd.h>
/* The ordinary native core may use compiler builtins. This independent primitive
 * control deliberately takes the same upstream portable fallback bodies as
 * DotCC, including real addressable XXH endian and swap functions. System
 * headers are already included, so their compiler ABI detection is untouched. */
#undef __GNUC__
#undef __clang__
#undef __BYTE_ORDER__
#undef __LITTLE_ENDIAN__
#undef __BIG_ENDIAN__
#undef __has_builtin
#endif
#include "mem.h"
#include "bits.h"
#define XXH_STATIC_LINKING_ONLY
#define XXH_IMPLEMENTATION
#include "xxhash.h"

static unsigned long dz_checks;
static U64 dz_digest;
#define REQUIRE(c) do { ++dz_checks; if (!(c)) return __LINE__; } while (0)
static void dz_observe(U64 v) { dz_digest = (dz_digest ^ v) * 1099511628211ULL; }
static unsigned dz_little(void) { const unsigned one = 1; return *(const unsigned char*)&one; }
static U64 dz_bytes(const unsigned char* p, unsigned n, unsigned little) {
    U64 v=0; unsigned i;
    for (i=0;i<n;++i) v |= (U64)p[i] << (8*(little ? i : n-1-i));
    return v;
}
static unsigned dz_clz(U64 v, unsigned width) { unsigned n=0; while (!(v & ((U64)1 << (width-1)))) { ++n; v <<= 1; } return n; }
static unsigned dz_ctz(U64 v) { unsigned n=0; while (!(v & 1)) { ++n; v >>= 1; } return n; }
static U64 dz_swap(U64 v, unsigned n) { U64 r=0; unsigned i; for (i=0;i<n;++i) { r=(r<<8)|(v&255); v>>=8; } return r; }
static U64 dz_random(U64* state) { *state ^= *state<<13; *state ^= *state>>7; *state ^= *state<<17; return *state; }

#define READ_CASE(fn, type, width, little) do { \
    type (*call)(const void*) = fn; \
    unsigned char* source = buffer+offset; \
    U64 expected=dz_bytes(source,width,little); \
    type a=fn(source++); REQUIRE(source==buffer+offset+1); REQUIRE((U64)a==expected); \
    source=buffer+offset; a=call(source++); REQUIRE(source==buffer+offset+1); REQUIRE((U64)a==expected); \
    buffer[offset]^=0x5a; a=call(buffer+offset); REQUIRE((U64)a==dz_bytes(buffer+offset,width,little)); \
    dz_observe(a); buffer[offset]^=0x5a; \
} while (0)

#define WRITE_CASE(fn, read, type, width, little) do { \
    void (*call)(void*,type)=fn; type values[2]; unsigned index=0, i; \
    unsigned char expected[96]; unsigned char* dest; \
    values[0]=(type)value; values[1]=0; \
    memset(buffer,0xa5,sizeof(buffer)); memset(expected,0xa5,sizeof(expected)); \
    for(i=0;i<width;++i) expected[offset+i]=(unsigned char)(value>>(8*(little ? i : width-1-i))); \
    dest=buffer+offset; fn(dest++,values[index++]); \
    REQUIRE(dest==buffer+offset+1); REQUIRE(index==1); REQUIRE(memcmp(buffer,expected,sizeof(buffer))==0); \
    REQUIRE((U64)read(buffer+offset)==dz_bytes(expected+offset,width,little)); \
    memset(buffer,0xa5,sizeof(buffer)); index=0; dest=buffer+offset; \
    call(dest++,values[index++]); REQUIRE(dest==buffer+offset+1); REQUIRE(index==1); \
    REQUIRE(memcmp(buffer,expected,sizeof(buffer))==0); REQUIRE((U64)read(buffer+offset)==dz_bytes(expected+offset,width,little)); \
    dz_observe(read(buffer+offset)); \
} while (0)

#define VALUE_CASE(fn,type,expected) do { \
    type arg=(type)value; type before=arg; unsigned (*call)(type)=fn; \
    unsigned a=fn(arg++); REQUIRE(arg==(type)(before+1)); REQUIRE(a==(expected)); \
    arg=before; a=call(arg++); REQUIRE(arg==(type)(before+1)); REQUIRE(a==(expected)); dz_observe(a); \
} while (0)
#define SWAP_CASE(fn,type,width) do { \
    type arg=(type)value; type before=arg; type (*call)(type)=fn; \
    type a=fn(arg++); REQUIRE(arg==(type)(before+1)); REQUIRE((U64)a==dz_swap(before,width)); \
    arg=before; a=call(arg++); REQUIRE(arg==(type)(before+1)); REQUIRE((U64)a==dz_swap(before,width)); dz_observe(a); \
} while (0)

int dz_primitives(void) {
    unsigned char buffer[96]; unsigned offset,i,trial;
    U64 state=0x9e3779b97f4a7c15ULL; U64 value;
    unsigned (*mem_endian)(void)=MEM_isLittleEndian;
    int (*xxh_endian)(void)=XXH_isLittleEndian;
    dz_checks=0; dz_digest=1469598103934665603ULL;
    REQUIRE(sizeof(size_t)==8); REQUIRE(sizeof(U64)==8);
    REQUIRE(MEM_isLittleEndian()==dz_little()); REQUIRE(mem_endian()==dz_little());
    REQUIRE(XXH_isLittleEndian()==(int)dz_little()); REQUIRE(xxh_endian()==(int)dz_little());
    for(trial=0;trial<32;++trial) {
        for(offset=0;offset<32;++offset) {
            for(i=0;i<sizeof(buffer);++i) buffer[i]=trial==0 ? 0 : trial==1 ? 255 : trial==2 ? (unsigned char)i : (unsigned char)dz_random(&state);
            READ_CASE(MEM_read16,U16,2,dz_little()); READ_CASE(MEM_read32,U32,4,dz_little());
            READ_CASE(MEM_read64,U64,8,dz_little()); READ_CASE(MEM_readST,size_t,8,dz_little());
            READ_CASE(MEM_readLE16,U16,2,1); READ_CASE(MEM_readLE24,U32,3,1);
            READ_CASE(MEM_readLE32,U32,4,1); READ_CASE(MEM_readLE64,U64,8,1); READ_CASE(MEM_readLEST,size_t,8,1);
            READ_CASE(MEM_readBE32,U32,4,0); READ_CASE(MEM_readBE64,U64,8,0); READ_CASE(MEM_readBEST,size_t,8,0);
            READ_CASE(XXH_read32,xxh_u32,4,dz_little()); READ_CASE(XXH_read64,xxh_u64,8,dz_little());
            READ_CASE(XXH_readLE32,xxh_u32,4,1); READ_CASE(XXH_readLE64,xxh_u64,8,1);
            READ_CASE(XXH_readBE32,xxh_u32,4,0); READ_CASE(XXH_readBE64,xxh_u64,8,0);
            value=trial==0 ? 0 : trial==1 ? ~(U64)0 : trial==2 ? 0x0123456789abcdefULL : dz_random(&state);
            WRITE_CASE(MEM_write16,MEM_read16,U16,2,dz_little());
            WRITE_CASE(MEM_write32,MEM_read32,U32,4,dz_little());
            WRITE_CASE(MEM_write64,MEM_read64,U64,8,dz_little());
            WRITE_CASE(MEM_writeLE16,MEM_readLE16,U16,2,1); WRITE_CASE(MEM_writeLE24,MEM_readLE24,U32,3,1);
            WRITE_CASE(MEM_writeLE32,MEM_readLE32,U32,4,1); WRITE_CASE(MEM_writeLE64,MEM_readLE64,U64,8,1); WRITE_CASE(MEM_writeLEST,MEM_readLEST,size_t,8,1);
            WRITE_CASE(MEM_writeBE32,MEM_readBE32,U32,4,0); WRITE_CASE(MEM_writeBE64,MEM_readBE64,U64,8,0); WRITE_CASE(MEM_writeBEST,MEM_readBEST,size_t,8,0);
        }
    }
    value=0;
    SWAP_CASE(MEM_swap32,U32,4); SWAP_CASE(MEM_swap64,U64,8); SWAP_CASE(MEM_swapST,size_t,8);
    SWAP_CASE(XXH_swap32,xxh_u32,4); SWAP_CASE(XXH_swap64,xxh_u64,8);
    value=~(U64)0;
    SWAP_CASE(MEM_swap32,U32,4); SWAP_CASE(MEM_swap64,U64,8); SWAP_CASE(MEM_swapST,size_t,8);
    SWAP_CASE(XXH_swap32,xxh_u32,4); SWAP_CASE(XXH_swap64,xxh_u64,8);
    /* Every single-bit position plus adjacent-bit masks and deterministic random
       nonzero values cover clz/ctz, highbit and native-endian common bytes. */
    for(i=0;i<1024;++i) {
        value=i<64 ? (U64)1<<i : (i<128 ? ~((U64)1<<(i-64)) : dz_random(&state));
        if(!value) value=1;
        VALUE_CASE(ZSTD_countLeadingZeros64,U64,dz_clz(value,64));
        VALUE_CASE(ZSTD_countTrailingZeros64,U64,dz_ctz(value));
        VALUE_CASE(ZSTD_NbCommonBytes,size_t,(dz_little() ? dz_ctz(value) : dz_clz(value,64))>>3);
        SWAP_CASE(MEM_swap64,U64,8); SWAP_CASE(MEM_swapST,size_t,8); SWAP_CASE(XXH_swap64,xxh_u64,8);
        value=(U32)value; if(!value) value=1;
        VALUE_CASE(ZSTD_countLeadingZeros32,U32,dz_clz(value,32));
        VALUE_CASE(ZSTD_countTrailingZeros32,U32,dz_ctz(value));
        VALUE_CASE(ZSTD_highbit32,U32,31-dz_clz(value,32));
        SWAP_CASE(MEM_swap32,U32,4); SWAP_CASE(XXH_swap32,xxh_u32,4);
    }
    /* Unselected rotations remain checked over their valid upstream domains. */
    for(i=0;i<64;++i) {
        U64 v=dz_random(&state); U32 r=i; U64 (*r64)(U64,U32)=ZSTD_rotateRight_U64;
        REQUIRE(r64(v,r)==((v>>r)|(v<<((0U-r)&63))));
        if(r<32) { U32 x=(U32)v; U32 (*r32)(U32,U32)=ZSTD_rotateRight_U32; REQUIRE(r32(x,r)==((x>>r)|(x<<((0U-r)&31)))); }
        if(r<16) { U16 x=(U16)v; U16 (*r16)(U16,U32)=ZSTD_rotateRight_U16; REQUIRE(r16(x,r)==(U16)((x>>r)|(x<<((0U-r)&15)))); }
    }
    return 0;
}
unsigned long long dz_primitive_checks(void) { return dz_checks; }
unsigned long long dz_primitive_digest(void) { return dz_digest; }

/* edge points at the final three bytes of a readable/writable page. The next
 * byte must actually be inaccessible, supplied by the test-only host harness. */
int dz_primitive_guard3(void* edge) {
    unsigned char* p=(unsigned char*)edge;
    U32 (*read)(const void*)=MEM_readLE24; void (*write)(void*,U32)=MEM_writeLE24;
    p[0]=0x12; p[1]=0x34; p[2]=0x56;
    if(MEM_readLE24(p)!=0x563412 || read(p)!=0x563412) return 1;
    MEM_writeLE24(p,0xffabcdef); if(read(p)!=0xabcdef) return 2;
    write(p,0xaa123456); if(MEM_readLE24(p)!=0x123456) return 3;
    return 0;
}

#ifdef DZ_NATIVE_MAIN
int main(int argc, char** argv) {
    long page=sysconf(_SC_PAGESIZE); int result=dz_primitives(); void* area;
    if(result) { fprintf(stderr,"primitive check failed at line %d\n",result); return 1; }
    area=mmap(0,(size_t)page*2,PROT_READ|PROT_WRITE,MAP_PRIVATE|MAP_ANONYMOUS,-1,0);
    if(area==MAP_FAILED || mprotect((char*)area+page,(size_t)page,PROT_NONE)) return 2;
    if(argc==2 && strcmp(argv[1],"--probe-guard")==0) {
        volatile unsigned char inaccessible=*((unsigned char*)area+page);
        return inaccessible;
    }
    result=dz_primitive_guard3((char*)area+page-3); munmap(area,(size_t)page*2);
    if(result) return 3;
    printf("{\"checks\":%llu,\"digest\":\"%016llx\",\"guard3\":true}\n",dz_primitive_checks(),dz_primitive_digest());
    return 0;
}
#endif
