#include <stdbool.h>
#include <stdalign.h>
#include <stdnoreturn.h>
#include <iso646.h>
#include <stdint.h>
#ifndef __bool_true_false_are_defined
#error missing bool compatibility feature macro
#endif
#ifndef __alignas_is_defined
#error missing alignment compatibility feature macro
#endif
#ifndef __alignof_is_defined
#error missing alignment compatibility feature macro
#endif
#ifdef __cplusplus
[[noreturn]] static void never_returns(void) { for (;;) {} }
extern "C" {
#else
noreturn static void never_returns(void) { for (;;) {} }
#endif
alignas(32) static unsigned char aligned_global[32];
int compatibility_headers(void) {
 bool a=true,b=false;
 if(sizeof(bool)!=1 or not a or b or (a and b))return 1;
 unsigned bits=3;bits and_eq 1;if(bits not_eq 1)return 2;
 bits or_eq 4;bits xor_eq 1;if(bits not_eq 4)return 3;
 if((bits bitand 7) not_eq 4 or (bits bitor 2) not_eq 6 or (bits xor 1) not_eq 5)return 4;
 if((compl bits) not_eq ~4U)return 5;
 if((uintptr_t)aligned_global%32 or alignof(long)!=8)return 6;
 alignas(32) unsigned char aligned_local[32];aligned_local[0]=17;
 if((uintptr_t)aligned_local%32 or aligned_local[0]!=17)return 7;
 void(*pointer)(void)=never_returns;if(!pointer)return 8;
 return 0;
}
#ifdef __cplusplus
}
#endif
