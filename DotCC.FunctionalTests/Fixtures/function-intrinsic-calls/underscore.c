#include "operations.h"
static int underscore_parameter(unsigned _, unsigned __dotcc_intrinsicDiscard0) {
    unsigned __dotcc_intrinsicDiscard1=17;
    unsigned __dotcc_intrinsicDiscard2[1];
    unsigned loops=0;
    volatile unsigned observed=7;
    clz32(1);
    (void)ctz64(1);
    (highbit(8),commonbytes(1));
    for(clz32(1);loops<1;highbit(8)) { ++loops; }
    for((void)ctz64(1);loops<2;(void)commonbytes(1)) { ++loops; }
    for(0,clz32(1);loops<3;1,highbit(8)) { ++loops; }
    for(observed,clz32(1);loops<4;observed,highbit(8)) { ++loops; }
    little_unsigned(); (void)little_signed();
    for(little_unsigned();loops<5;little_signed()) { ++loops; }
    for(0,little_unsigned(),clz32(1);loops<6;little_signed(),highbit(8),1) { ++loops; }
    return _==99 && __dotcc_intrinsicDiscard0==13 && __dotcc_intrinsicDiscard1==17;
}
int underscore_probe(void) {
    unsigned _=99;
    clz32(1); (void)ctz64(1);
    return _==99 && underscore_parameter(99,13);
}
#ifndef INTRINSIC_NATIVE_MAIN
#include "selected.h"
#else
#include <stdio.h>
int main(void) { if(!underscore_probe()) return 1; puts("underscore: preserved99"); return 0; }
#endif
