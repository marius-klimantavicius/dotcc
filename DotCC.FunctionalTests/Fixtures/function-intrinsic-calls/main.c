#include "operations.h"
#define CHECK(c) do { if(!(c)) return __LINE__; } while(0)
static unsigned other(uint32_t v) { return 77; }
static unsigned side_count;
static void side(void) { ++side_count; }
int check_memory(void) {
    unsigned char b[12]={0xa5,0,0,0,0,0,0,0,0,0x5a,0,0};
    unsigned char *p=b+1; uint64_t wide=0x12345678abcdef01ULL;
    uint32_t (*readers[2])(const void*)={read_be32,&read_be32};
    void (*writer)(void*,uint32_t)=write_be32;
    uint16_t native; unsigned little;
    unsigned one=1; little=*(const unsigned char*)&one;
    write_be32(p++,wide++);
    CHECK(p==b+2 && wide==0x12345678abcdef02ULL);
    CHECK(b[0]==0xa5 && b[1]==0xab && b[2]==0xcd && b[3]==0xef && b[4]==1 && b[5]==0);
    CHECK(read_be32(b+1)==0xabcdef01U && readers[1](b+1)==0xabcdef01U);
    writer(b+1,0); CHECK(read_be32(b+1)==0);
    write_le16(b+1,65537); CHECK(b[1]==1 && b[2]==0);
    write_le16(b+1,-1); CHECK(b[1]==255 && b[2]==255);
    { enum Value { too_wide=65537 }; write_le16(b+1,too_wide); }
    native=read_native16(b+1); CHECK(native==(little ? 1 : 256));
    write_native64(b+1,0x0102030405060708ULL);
    CHECK(read_le64(b+1)==(little ? 0x0102030405060708ULL : 0x0807060504030201ULL));
    CHECK(b[0]==0xa5 && b[9]==0x5a);
    CHECK(readers[0]==readers[1] && readers[0]==&read_be32);
    b[1]=7; CHECK(read_le64(b+1)==(little ? 0x0102030405060707ULL : 0x0807060504030207ULL));
    CHECK(swap32(0x12345678U)==0x78563412U && swap64(0x0102030405060708ULL)==0x0807060504030201ULL);
    return 0;
}
int check_conversions(void) {
    enum Bit { first=1 }; uint64_t wide=0x100000001ULL; unsigned x=1;
    CHECK(clz32(wide++)==31 && wide==0x100000002ULL);
    CHECK(clz64(1U)==63 && clz64(first)==63);
    CHECK(ctz64(0x100000000ULL)==32);
    CHECK(clz32(-1)==0 && clz64(-1)==0);
    CHECK(clz32(1)-32U==0xffffffffU && ctz64(1)-1U==0xffffffffU);
    CHECK(clz32(0)==32 && clz64(0)==64 && ctz32(0)==32 && ctz64(0)==64);
    CHECK((swap32(x++)|1U)==0x01000001U && x==2);
    CHECK(2*highbit(8)+1==7);
    CHECK(2*commonbytes(0x10000ULL)+1==(little_unsigned()?5U:11U));
    CHECK(little_signed()==(int)little_unsigned());
    CHECK(little_unsigned()-2U==(little_signed()?0xffffffffU:0xfffffffeU));
    return 0;
}
int check_contexts(void) {
    unsigned x=1, loops=0; uint64_t y=1;
    clz32(x++); (void)ctz64(y++);
    (clz32(x++),ctz64(y++));
    CHECK(x==3 && y==3);
    for(highbit(x++);loops<2;commonbytes(y++)) { ++loops; }
    CHECK(x==4 && y==5 && loops==2);
    for((void)clz32(x++);loops<4;(void)ctz64(y++)) { ++loops; }
    CHECK(x==5 && y==7);
    for((clz32(x++),highbit(x++));loops<6;(ctz64(y++),commonbytes(y++))) { ++loops; }
    CHECK(x==7 && y==11);
    if(0 && clz32(x++)) return 1;
    if(1 || ctz64(y++)) { }
    CHECK(x==7 && y==11);
    CHECK((1 ? clz32(x++) : ctz32(x++))==29 && x==8);
    CHECK((0 ? commonbytes(y++) : highbit(x++))==3 && y==11 && x==9);
    for(0,clz32(x++),side();loops<8;1,highbit(y++),side()) { ++loops; }
    CHECK(x==10 && y==13 && loops==8 && side_count==3);
    for((0,clz32(x++)),loops=loops;loops<10;(1,ctz64(y++)),loops=loops) { ++loops; }
    CHECK(x==11 && y==15 && loops==10 && side_count==3);
    return 0;
}
int check_shadowing(void) {
    unsigned (*clz32)(uint32_t)=other;
    struct Holder { int clz32; } holder={9};
    return clz32(1)+holder.clz32;
}
int probe(void) {
    int r=check_memory(); if(r) return r;
    r=check_conversions(); if(r) return r;
    r=check_contexts(); if(r) return r;
    CHECK(check_shadowing()==86);
    return 0;
}
#ifndef INTRINSIC_NATIVE_MAIN
#include "selected.h"
#else
#include <stdio.h>
int main(void) { int r=probe(); if(r) fprintf(stderr,"check failed: %d\n",r); else puts("intrinsic-calls: ok"); return r!=0; }
#endif
