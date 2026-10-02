//type: rp
//options: 
# 0 "./torture/fp-int-convert-float.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/fp-int-convert-float.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./torture/fp-int-convert-float.c" 2
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
# 8 "./torture/fp-int-convert-float.c" 2

int
main (void)
{
  do { do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)0); fv1 = ((signed char)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed char)0)) || fv1 != (float) ((signed char)0) || fv2 != (float) ((signed char)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)1); fv1 = ((signed char)1); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)1) || ((1) && ivout != ivin) || ((1) && ivout != ((signed char)1)) || fv1 != (float) ((signed char)1) || fv2 != (float) ((signed char)1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((unsigned char)~(unsigned char)0) >> 1)); fv1 = ((signed char)(((unsigned char)~(unsigned char)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((unsigned char)~(unsigned char)0) >> 1)) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed char)(((unsigned char)~(unsigned char)0) >> 1))) || fv1 != (float) ((signed char)(((unsigned char)~(unsigned char)0) >> 1)) || fv2 != (float) ((signed char)(((unsigned char)~(unsigned char)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)); fv1 = ((signed char)(unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed char)(unsigned char)~(((unsigned char)~(unsigned char)0) >> 1))) || fv1 != (float) ((signed char)(unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)) || fv2 != (float) ((signed char)(unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(unsigned char)~(unsigned char)0); fv1 = ((signed char)(unsigned char)~(unsigned char)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(unsigned char)~(unsigned char)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed char)(unsigned char)~(unsigned char)0)) || fv1 != (float) ((signed char)(unsigned char)~(unsigned char)0) || fv2 != (float) ((signed char)(unsigned char)~(unsigned char)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1) ? (signed char)1 : (((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed char)3 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)0); fv1 = ((unsigned char)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)0) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned char)0)) || fv1 != (float) ((unsigned char)0) || fv2 != (float) ((unsigned char)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)1); fv1 = ((unsigned char)1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)1) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned char)1)) || fv1 != (float) ((unsigned char)1) || fv2 != (float) ((unsigned char)1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((unsigned char)~(unsigned char)0) >> 1)); fv1 = ((unsigned char)(((unsigned char)~(unsigned char)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((unsigned char)~(unsigned char)0) >> 1)) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned char)(((unsigned char)~(unsigned char)0) >> 1))) || fv1 != (float) ((unsigned char)(((unsigned char)~(unsigned char)0) >> 1)) || fv2 != (float) ((unsigned char)(((unsigned char)~(unsigned char)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)); fv1 = ((unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned char)~(((unsigned char)~(unsigned char)0) >> 1))) || fv1 != (float) ((unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)) || fv2 != (float) ((unsigned char)~(((unsigned char)~(unsigned char)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)~(unsigned char)0); fv1 = ((unsigned char)~(unsigned char)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)~(unsigned char)0) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)~(unsigned char)0)) || fv1 != (float) ((unsigned char)~(unsigned char)0) || fv2 != (float) ((unsigned char)~(unsigned char)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned char ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned char)(((24
# 12 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned char) * 8
# 12 "./torture/fp-int-convert-float.c"
 ) ? (unsigned char)1 : (((unsigned char)1 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned char)3 << (sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 12 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = ((signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = ((signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != ((signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ((signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) ((signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) ((signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); do { static volatile signed char ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = (-(signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 12 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != (-(signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) (-(signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) (-(signed char)((signed char)1 << (sizeof(signed char) * 8 
# 12 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); } while (0);
  do { do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)0); fv1 = ((signed short)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed short)0)) || fv1 != (float) ((signed short)0) || fv2 != (float) ((signed short)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)1); fv1 = ((signed short)1); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)1) || ((1) && ivout != ivin) || ((1) && ivout != ((signed short)1)) || fv1 != (float) ((signed short)1) || fv2 != (float) ((signed short)1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((unsigned short)~(unsigned short)0) >> 1)); fv1 = ((signed short)(((unsigned short)~(unsigned short)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((unsigned short)~(unsigned short)0) >> 1)) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed short)(((unsigned short)~(unsigned short)0) >> 1))) || fv1 != (float) ((signed short)(((unsigned short)~(unsigned short)0) >> 1)) || fv2 != (float) ((signed short)(((unsigned short)~(unsigned short)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)); fv1 = ((signed short)(unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed short)(unsigned short)~(((unsigned short)~(unsigned short)0) >> 1))) || fv1 != (float) ((signed short)(unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)) || fv2 != (float) ((signed short)(unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(unsigned short)~(unsigned short)0); fv1 = ((signed short)(unsigned short)~(unsigned short)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(unsigned short)~(unsigned short)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed short)(unsigned short)~(unsigned short)0)) || fv1 != (float) ((signed short)(unsigned short)~(unsigned short)0) || fv2 != (float) ((signed short)(unsigned short)~(unsigned short)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1) ? (signed short)1 : (((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed short)3 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)0); fv1 = ((unsigned short)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)0) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned short)0)) || fv1 != (float) ((unsigned short)0) || fv2 != (float) ((unsigned short)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)1); fv1 = ((unsigned short)1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)1) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned short)1)) || fv1 != (float) ((unsigned short)1) || fv2 != (float) ((unsigned short)1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((unsigned short)~(unsigned short)0) >> 1)); fv1 = ((unsigned short)(((unsigned short)~(unsigned short)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((unsigned short)~(unsigned short)0) >> 1)) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned short)(((unsigned short)~(unsigned short)0) >> 1))) || fv1 != (float) ((unsigned short)(((unsigned short)~(unsigned short)0) >> 1)) || fv2 != (float) ((unsigned short)(((unsigned short)~(unsigned short)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)); fv1 = ((unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned short)~(((unsigned short)~(unsigned short)0) >> 1))) || fv1 != (float) ((unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)) || fv2 != (float) ((unsigned short)~(((unsigned short)~(unsigned short)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)~(unsigned short)0); fv1 = ((unsigned short)~(unsigned short)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)~(unsigned short)0) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)~(unsigned short)0)) || fv1 != (float) ((unsigned short)~(unsigned short)0) || fv2 != (float) ((unsigned short)~(unsigned short)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned short ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned short)(((24
# 13 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned short) * 8
# 13 "./torture/fp-int-convert-float.c"
 ) ? (unsigned short)1 : (((unsigned short)1 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned short)3 << (sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 13 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = ((signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = ((signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != ((signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ((signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) ((signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) ((signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); do { static volatile signed short ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = (-(signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 13 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != (-(signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) (-(signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) (-(signed short)((signed short)1 << (sizeof(signed short) * 8 
# 13 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); } while (0);
  do { do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)0); fv1 = ((signed int)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed int)0)) || fv1 != (float) ((signed int)0) || fv2 != (float) ((signed int)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)1); fv1 = ((signed int)1); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)1) || ((1) && ivout != ivin) || ((1) && ivout != ((signed int)1)) || fv1 != (float) ((signed int)1) || fv2 != (float) ((signed int)1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((unsigned int)~(unsigned int)0) >> 1)); fv1 = ((signed int)(((unsigned int)~(unsigned int)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((unsigned int)~(unsigned int)0) >> 1)) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed int)(((unsigned int)~(unsigned int)0) >> 1))) || fv1 != (float) ((signed int)(((unsigned int)~(unsigned int)0) >> 1)) || fv2 != (float) ((signed int)(((unsigned int)~(unsigned int)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)); fv1 = ((signed int)(unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed int)(unsigned int)~(((unsigned int)~(unsigned int)0) >> 1))) || fv1 != (float) ((signed int)(unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)) || fv2 != (float) ((signed int)(unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(unsigned int)~(unsigned int)0); fv1 = ((signed int)(unsigned int)~(unsigned int)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(unsigned int)~(unsigned int)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed int)(unsigned int)~(unsigned int)0)) || fv1 != (float) ((signed int)(unsigned int)~(unsigned int)0) || fv2 != (float) ((signed int)(unsigned int)~(unsigned int)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1) ? (signed int)1 : (((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed int)3 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)0); fv1 = ((unsigned int)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)0) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned int)0)) || fv1 != (float) ((unsigned int)0) || fv2 != (float) ((unsigned int)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)1); fv1 = ((unsigned int)1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)1) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned int)1)) || fv1 != (float) ((unsigned int)1) || fv2 != (float) ((unsigned int)1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((unsigned int)~(unsigned int)0) >> 1)); fv1 = ((unsigned int)(((unsigned int)~(unsigned int)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((unsigned int)~(unsigned int)0) >> 1)) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned int)(((unsigned int)~(unsigned int)0) >> 1))) || fv1 != (float) ((unsigned int)(((unsigned int)~(unsigned int)0) >> 1)) || fv2 != (float) ((unsigned int)(((unsigned int)~(unsigned int)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)); fv1 = ((unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned int)~(((unsigned int)~(unsigned int)0) >> 1))) || fv1 != (float) ((unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)) || fv2 != (float) ((unsigned int)~(((unsigned int)~(unsigned int)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)~(unsigned int)0); fv1 = ((unsigned int)~(unsigned int)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)~(unsigned int)0) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)~(unsigned int)0)) || fv1 != (float) ((unsigned int)~(unsigned int)0) || fv2 != (float) ((unsigned int)~(unsigned int)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned int ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned int)(((24
# 14 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned int) * 8
# 14 "./torture/fp-int-convert-float.c"
 ) ? (unsigned int)1 : (((unsigned int)1 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned int)3 << (sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 14 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = ((signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = ((signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != ((signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ((signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) ((signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) ((signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); do { static volatile signed int ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = (-(signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 14 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != (-(signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) (-(signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) (-(signed int)((signed int)1 << (sizeof(signed int) * 8 
# 14 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); } while (0);
  do { do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)0); fv1 = ((signed long)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed long)0)) || fv1 != (float) ((signed long)0) || fv2 != (float) ((signed long)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)1); fv1 = ((signed long)1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)1) || ((1) && ivout != ivin) || ((1) && ivout != ((signed long)1)) || fv1 != (float) ((signed long)1) || fv2 != (float) ((signed long)1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((unsigned long)~(unsigned long)0) >> 1)); fv1 = ((signed long)(((unsigned long)~(unsigned long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((unsigned long)~(unsigned long)0) >> 1)) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed long)(((unsigned long)~(unsigned long)0) >> 1))) || fv1 != (float) ((signed long)(((unsigned long)~(unsigned long)0) >> 1)) || fv2 != (float) ((signed long)(((unsigned long)~(unsigned long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)); fv1 = ((signed long)(unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed long)(unsigned long)~(((unsigned long)~(unsigned long)0) >> 1))) || fv1 != (float) ((signed long)(unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)) || fv2 != (float) ((signed long)(unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(unsigned long)~(unsigned long)0); fv1 = ((signed long)(unsigned long)~(unsigned long)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(unsigned long)~(unsigned long)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed long)(unsigned long)~(unsigned long)0)) || fv1 != (float) ((signed long)(unsigned long)~(unsigned long)0) || fv2 != (float) ((signed long)(unsigned long)~(unsigned long)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long)1 : (((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long)3 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)0); fv1 = ((unsigned long)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)0) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned long)0)) || fv1 != (float) ((unsigned long)0) || fv2 != (float) ((unsigned long)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)1); fv1 = ((unsigned long)1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)1) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned long)1)) || fv1 != (float) ((unsigned long)1) || fv2 != (float) ((unsigned long)1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((unsigned long)~(unsigned long)0) >> 1)); fv1 = ((unsigned long)(((unsigned long)~(unsigned long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((unsigned long)~(unsigned long)0) >> 1)) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned long)(((unsigned long)~(unsigned long)0) >> 1))) || fv1 != (float) ((unsigned long)(((unsigned long)~(unsigned long)0) >> 1)) || fv2 != (float) ((unsigned long)(((unsigned long)~(unsigned long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)); fv1 = ((unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned long)~(((unsigned long)~(unsigned long)0) >> 1))) || fv1 != (float) ((unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)) || fv2 != (float) ((unsigned long)~(((unsigned long)~(unsigned long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)~(unsigned long)0); fv1 = ((unsigned long)~(unsigned long)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)~(unsigned long)0) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)~(unsigned long)0)) || fv1 != (float) ((unsigned long)~(unsigned long)0) || fv2 != (float) ((unsigned long)~(unsigned long)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned long)(((24
# 15 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long) * 8
# 15 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long)1 : (((unsigned long)1 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long)3 << (sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 15 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = ((signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != ((signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ((signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) ((signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) ((signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = (-(signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 15 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != (-(signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) (-(signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) (-(signed long)((signed long)1 << (sizeof(signed long) * 8 
# 15 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); } while (0);
  do { do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)0); fv1 = ((signed long long)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed long long)0)) || fv1 != (float) ((signed long long)0) || fv2 != (float) ((signed long long)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)1); fv1 = ((signed long long)1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)1) || ((1) && ivout != ivin) || ((1) && ivout != ((signed long long)1)) || fv1 != (float) ((signed long long)1) || fv2 != (float) ((signed long long)1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((unsigned long long)~(unsigned long long)0) >> 1)); fv1 = ((signed long long)(((unsigned long long)~(unsigned long long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((unsigned long long)~(unsigned long long)0) >> 1)) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed long long)(((unsigned long long)~(unsigned long long)0) >> 1))) || fv1 != (float) ((signed long long)(((unsigned long long)~(unsigned long long)0) >> 1)) || fv2 != (float) ((signed long long)(((unsigned long long)~(unsigned long long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)); fv1 = ((signed long long)(unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((signed long long)(unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1))) || fv1 != (float) ((signed long long)(unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)) || fv2 != (float) ((signed long long)(unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(unsigned long long)~(unsigned long long)0); fv1 = ((signed long long)(unsigned long long)~(unsigned long long)0); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(unsigned long long)~(unsigned long long)0) || ((1) && ivout != ivin) || ((1) && ivout != ((signed long long)(unsigned long long)~(unsigned long long)0)) || fv1 != (float) ((signed long long)(unsigned long long)~(unsigned long long)0) || fv2 != (float) ((signed long long)(unsigned long long)~(unsigned long long)0) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv1 = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) (-(signed long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1) ? (signed long long)1 : (((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2)) + ((signed long long)3 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 2 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)0); fv1 = ((unsigned long long)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)0) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned long long)0)) || fv1 != (float) ((unsigned long long)0) || fv2 != (float) ((unsigned long long)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)1); fv1 = ((unsigned long long)1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)1) || ((1) && ivout != ivin) || ((1) && ivout != ((unsigned long long)1)) || fv1 != (float) ((unsigned long long)1) || fv2 != (float) ((unsigned long long)1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((unsigned long long)~(unsigned long long)0) >> 1)); fv1 = ((unsigned long long)(((unsigned long long)~(unsigned long long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((unsigned long long)~(unsigned long long)0) >> 1)) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned long long)(((unsigned long long)~(unsigned long long)0) >> 1))) || fv1 != (float) ((unsigned long long)(((unsigned long long)~(unsigned long long)0) >> 1)) || fv2 != (float) ((unsigned long long)(((unsigned long long)~(unsigned long long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)); fv1 = ((unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ivin) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) && ivout != ((unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1))) || fv1 != (float) ((unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)) || fv2 != (float) ((unsigned long long)~(((unsigned long long)~(unsigned long long)0) >> 1)) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)~(unsigned long long)0); fv1 = ((unsigned long long)~(unsigned long long)0); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)~(unsigned long long)0) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)~(unsigned long long)0)) || fv1 != (float) ((unsigned long long)~(unsigned long long)0) || fv2 != (float) ((unsigned long long)~(unsigned long long)0) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv1 = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))))) || fv1 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv2 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 ))))) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv1 = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1)) || fv1 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv2 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) + 1) || fv1 != fv2) abort (); } while (0); do { static volatile unsigned long long ivin, ivout; static volatile float fv1, fv2; ivin = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv1 = ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1); fv2 = ivin; ivout = fv2; if (ivin != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ivin) || ((((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 )) && ivout != ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1)) || fv1 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv2 != (float) ((unsigned long long)(((24
# 16 "./torture/fp-int-convert-float.c"
 ) >= sizeof(unsigned long long) * 8
# 16 "./torture/fp-int-convert-float.c"
 ) ? (unsigned long long)1 : (((unsigned long long)1 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1)) + ((unsigned long long)3 << (sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 - 1 - 24
# 16 "./torture/fp-int-convert-float.c"
 )))) - 1) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = ((signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = ((signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != ((signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ((signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) ((signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) ((signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); do { static volatile signed long long ivin, ivout; static volatile float fv1, fv2; ivin = (-(signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv1 = (-(signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))); fv2 = ivin; ivout = fv2; if (ivin != (-(signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != ivin) || ((((128
# 16 "./torture/fp-int-convert-float.c"
 ) > sizeof(unsigned long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1)) && ivout != (-(signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1)))) || fv1 != (float) (-(signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv2 != (float) (-(signed long long)((signed long long)1 << (sizeof(signed long long) * 8 
# 16 "./torture/fp-int-convert-float.c"
 / 2 - 1))) || fv1 != fv2) abort (); } while (0); } while (0);
  exit (0);
}
