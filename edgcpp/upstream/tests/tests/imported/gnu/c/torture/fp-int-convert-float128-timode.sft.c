//type: rp
//options: 
# 0 "./torture/fp-int-convert-float128-timode.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/fp-int-convert-float128-timode.c"
# 9 "./torture/fp-int-convert-float128-timode.c"
# 1 "./torture/fp-int-convert.h" 1



# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 27 "/usr/include/limits.h" 2 3 4
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 5 "./torture/fp-int-convert.h" 2

# 5 "./torture/fp-int-convert.h"
extern void abort (void);
extern void exit (int);




typedef int TItype __attribute__ ((mode (TI)));
typedef unsigned int UTItype __attribute__ ((mode (TI)));
# 10 "./torture/fp-int-convert-float128-timode.c" 2




int
main (void)
{
  do { do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)0); fv1 = ((TItype)0); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)0) || ((1) && ivout != ivin) || ((1) && ivout != ((TItype)0)) || fv1 != (__float128) ((TItype)0) || fv2 != (__float128) ((TItype)0) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)1); fv1 = ((TItype)1); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)1) || ((1) && ivout != ivin) || ((1) && ivout != ((TItype)1)) || fv1 != (__float128) ((TItype)1) || fv2 != (__float128) ((TItype)1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((UTItype)~(UTItype)0) >> 1)); fv1 = ((TItype)(((UTItype)~(UTItype)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((UTItype)~(UTItype)0) >> 1)) || ((((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ((TItype)(((UTItype)~(UTItype)0) >> 1))) || fv1 != (__float128) ((TItype)(((UTItype)~(UTItype)0) >> 1)) || fv2 != (__float128) ((TItype)(((UTItype)~(UTItype)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(UTItype)~(((UTItype)~(UTItype)0) >> 1)); fv1 = ((TItype)(UTItype)~(((UTItype)~(UTItype)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(UTItype)~(((UTItype)~(UTItype)0) >> 1)) || ((((16384) > sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ivin) || ((((16384) > sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ((TItype)(UTItype)~(((UTItype)~(UTItype)0) >> 1))) || fv1 != (__float128) ((TItype)(UTItype)~(((UTItype)~(UTItype)0) >> 1)) || fv2 != (__float128) ((TItype)(UTItype)~(((UTItype)~(UTItype)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(UTItype)~(UTItype)0); fv1 = ((TItype)(UTItype)~(UTItype)0); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(UTItype)~(UTItype)0) || ((1) && ivout != ivin) || ((1) && ivout != ((TItype)(UTItype)~(UTItype)0)) || fv1 != (__float128) ((TItype)(UTItype)~(UTItype)0) || fv2 != (__float128) ((TItype)(UTItype)~(UTItype)0) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv1 = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))))) || fv1 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv2 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv1 = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1)) || fv1 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv2 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv1 = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1)) || fv1 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv2 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv1 = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))))) || fv1 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv2 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv1 = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1)) || fv1 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv2 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv1 = ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1)) || fv1 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv2 != (__float128) ((TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv1 = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))))) || fv1 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv2 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv1 = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1)) || fv1 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv2 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv1 = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1)) || fv1 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv2 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv1 = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))))) || fv1 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv2 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113))))) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv1 = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1)) || fv1 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv2 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv1 = (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(TItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1)) || fv1 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv2 != (__float128) (-(TItype)(((113) >= sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1) ? (TItype)1 : (((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2)) + ((TItype)3 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 2 - 113)))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)0); fv1 = ((UTItype)0); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)0) || ((1) && ivout != ivin) || ((1) && ivout != ((UTItype)0)) || fv1 != (__float128) ((UTItype)0) || fv2 != (__float128) ((UTItype)0) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)1); fv1 = ((UTItype)1); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)1) || ((1) && ivout != ivin) || ((1) && ivout != ((UTItype)1)) || fv1 != (__float128) ((UTItype)1) || fv2 != (__float128) ((UTItype)1) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((UTItype)~(UTItype)0) >> 1)); fv1 = ((UTItype)(((UTItype)~(UTItype)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((UTItype)~(UTItype)0) >> 1)) || ((((113) >= sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ((UTItype)(((UTItype)~(UTItype)0) >> 1))) || fv1 != (__float128) ((UTItype)(((UTItype)~(UTItype)0) >> 1)) || fv2 != (__float128) ((UTItype)(((UTItype)~(UTItype)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)~(((UTItype)~(UTItype)0) >> 1)); fv1 = ((UTItype)~(((UTItype)~(UTItype)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)~(((UTItype)~(UTItype)0) >> 1)) || ((((16384) > sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ivin) || ((((16384) > sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) && ivout != ((UTItype)~(((UTItype)~(UTItype)0) >> 1))) || fv1 != (__float128) ((UTItype)~(((UTItype)~(UTItype)0) >> 1)) || fv2 != (__float128) ((UTItype)~(((UTItype)~(UTItype)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)~(UTItype)0); fv1 = ((UTItype)~(UTItype)0); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)~(UTItype)0) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)~(UTItype)0)) || fv1 != (__float128) ((UTItype)~(UTItype)0) || fv2 != (__float128) ((UTItype)~(UTItype)0) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))); fv1 = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))))) || fv1 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))) || fv2 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1); fv1 = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1)) || fv1 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1) || fv2 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1); fv1 = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1)) || fv1 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1) || fv2 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))); fv1 = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))))) || fv1 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))) || fv2 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113))))) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1); fv1 = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1)) || fv1 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1) || fv2 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile UTItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1); fv1 = ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ivin) || ((((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 )) && ivout != ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1)) || fv1 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1) || fv2 != (__float128) ((UTItype)(((113) >= sizeof(UTItype) * 8
# 17 "./torture/fp-int-convert-float128-timode.c"
 ) ? (UTItype)1 : (((UTItype)1 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1)) + ((UTItype)3 << (sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 - 1 - 113)))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = ((TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))); fv1 = ((TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != ((TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))) || ((((16384) > sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1)) && ivout != ivin) || ((((16384) > sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1)) && ivout != ((TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1)))) || fv1 != (__float128) ((TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))) || fv2 != (__float128) ((TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); do { static volatile TItype ivin, ivout; static volatile __float128 fv1, fv2; ivin = (-(TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))); fv1 = (-(TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != (-(TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))) || ((((16384) > sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1)) && ivout != ivin) || ((((16384) > sizeof(UTItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1)) && ivout != (-(TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1)))) || fv1 != (__float128) (-(TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))) || fv2 != (__float128) (-(TItype)((TItype)1 << (sizeof(TItype) * 8 
# 17 "./torture/fp-int-convert-float128-timode.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); } while (0);
  exit (0);
}
