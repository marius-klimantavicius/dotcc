//type: rp
//options: --c11
# 0 "./atomic/stdatomic-lockfree.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/stdatomic-lockfree.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 1 3 4
# 29 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4

# 29 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4
typedef enum
  {
    memory_order_relaxed = 0,
    memory_order_consume = 1,
    memory_order_acquire = 2,
    memory_order_release = 3,
    memory_order_acq_rel = 4,
    memory_order_seq_cst = 5
  } memory_order;


typedef _Atomic _Bool atomic_bool;
typedef _Atomic char atomic_char;
typedef _Atomic signed char atomic_schar;
typedef _Atomic unsigned char atomic_uchar;
typedef _Atomic short atomic_short;
typedef _Atomic unsigned short atomic_ushort;
typedef _Atomic int atomic_int;
typedef _Atomic unsigned int atomic_uint;
typedef _Atomic long atomic_long;
typedef _Atomic unsigned long atomic_ulong;
typedef _Atomic long long atomic_llong;
typedef _Atomic unsigned long long atomic_ullong;



typedef _Atomic short unsigned int atomic_char16_t;
typedef _Atomic unsigned int atomic_char32_t;
typedef _Atomic int atomic_wchar_t;
typedef _Atomic signed char atomic_int_least8_t;
typedef _Atomic unsigned char atomic_uint_least8_t;
typedef _Atomic short int atomic_int_least16_t;
typedef _Atomic short unsigned int atomic_uint_least16_t;
typedef _Atomic int atomic_int_least32_t;
typedef _Atomic unsigned int atomic_uint_least32_t;
typedef _Atomic long int atomic_int_least64_t;
typedef _Atomic long unsigned int atomic_uint_least64_t;
typedef _Atomic signed char atomic_int_fast8_t;
typedef _Atomic unsigned char atomic_uint_fast8_t;
typedef _Atomic long int atomic_int_fast16_t;
typedef _Atomic long unsigned int atomic_uint_fast16_t;
typedef _Atomic long int atomic_int_fast32_t;
typedef _Atomic long unsigned int atomic_uint_fast32_t;
typedef _Atomic long int atomic_int_fast64_t;
typedef _Atomic long unsigned int atomic_uint_fast64_t;
typedef _Atomic long int atomic_intptr_t;
typedef _Atomic long unsigned int atomic_uintptr_t;
typedef _Atomic long unsigned int atomic_size_t;
typedef _Atomic long int atomic_ptrdiff_t;
typedef _Atomic long int atomic_intmax_t;
typedef _Atomic long unsigned int atomic_uintmax_t;
# 97 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4
extern void atomic_thread_fence (memory_order);

extern void atomic_signal_fence (memory_order);
# 226 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdatomic.h" 3 4
typedef _Atomic struct
{

  _Bool __val;



} atomic_flag;




extern _Bool atomic_flag_test_and_set (volatile atomic_flag *);


extern _Bool atomic_flag_test_and_set_explicit (volatile atomic_flag *,
      memory_order);



extern void atomic_flag_clear (volatile atomic_flag *);

extern void atomic_flag_clear_explicit (volatile atomic_flag *, memory_order);
# 6 "./atomic/stdatomic-lockfree.c" 2
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
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
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 7 "./atomic/stdatomic-lockfree.c" 2


# 8 "./atomic/stdatomic-lockfree.c"
extern void abort (void);

_Atomic _Bool aba;
atomic_bool abt;
_Atomic char aca;
atomic_char act;
_Atomic short unsigned int ac16a;
atomic_char16_t ac16t;
_Atomic unsigned int ac32a;
atomic_char32_t ac32t;
_Atomic int awca;
atomic_wchar_t awct;
_Atomic short asa;
atomic_short ast;
_Atomic int aia;
atomic_int ait;
_Atomic long ala;
atomic_long alt;
_Atomic long long alla;
atomic_llong allt;
void *_Atomic apa;
# 53 "./atomic/stdatomic-lockfree.c"
int
main ()
{
  do { int r1 = 2
# 56 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 56 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 56 "./atomic/stdatomic-lockfree.c"
 &aba
# 56 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 56 "./atomic/stdatomic-lockfree.c"
 &aba
# 56 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 56 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 56 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 56 "./atomic/stdatomic-lockfree.c"
 &abt
# 56 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 56 "./atomic/stdatomic-lockfree.c"
 &abt
# 56 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 56 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 57 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 57 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 57 "./atomic/stdatomic-lockfree.c"
 &aca
# 57 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 57 "./atomic/stdatomic-lockfree.c"
 &aca
# 57 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 57 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 57 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 57 "./atomic/stdatomic-lockfree.c"
 &act
# 57 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 57 "./atomic/stdatomic-lockfree.c"
 &act
# 57 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 57 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 58 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 58 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 58 "./atomic/stdatomic-lockfree.c"
 &ac16a
# 58 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 58 "./atomic/stdatomic-lockfree.c"
 &ac16a
# 58 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 58 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 58 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 58 "./atomic/stdatomic-lockfree.c"
 &ac16t
# 58 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 58 "./atomic/stdatomic-lockfree.c"
 &ac16t
# 58 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 58 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 59 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 59 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 59 "./atomic/stdatomic-lockfree.c"
 &ac32a
# 59 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 59 "./atomic/stdatomic-lockfree.c"
 &ac32a
# 59 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 59 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 59 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 59 "./atomic/stdatomic-lockfree.c"
 &ac32t
# 59 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 59 "./atomic/stdatomic-lockfree.c"
 &ac32t
# 59 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 59 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 60 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 60 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 60 "./atomic/stdatomic-lockfree.c"
 &awca
# 60 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 60 "./atomic/stdatomic-lockfree.c"
 &awca
# 60 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 60 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 60 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 60 "./atomic/stdatomic-lockfree.c"
 &awct
# 60 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 60 "./atomic/stdatomic-lockfree.c"
 &awct
# 60 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 60 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 61 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 61 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 61 "./atomic/stdatomic-lockfree.c"
 &asa
# 61 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 61 "./atomic/stdatomic-lockfree.c"
 &asa
# 61 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 61 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 61 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 61 "./atomic/stdatomic-lockfree.c"
 &ast
# 61 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 61 "./atomic/stdatomic-lockfree.c"
 &ast
# 61 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 61 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 62 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 62 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 62 "./atomic/stdatomic-lockfree.c"
 &aia
# 62 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 62 "./atomic/stdatomic-lockfree.c"
 &aia
# 62 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 62 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 62 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 62 "./atomic/stdatomic-lockfree.c"
 &ait
# 62 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 62 "./atomic/stdatomic-lockfree.c"
 &ait
# 62 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 62 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 63 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 63 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 63 "./atomic/stdatomic-lockfree.c"
 &ala
# 63 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 63 "./atomic/stdatomic-lockfree.c"
 &ala
# 63 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 63 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 63 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 63 "./atomic/stdatomic-lockfree.c"
 &alt
# 63 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 63 "./atomic/stdatomic-lockfree.c"
 &alt
# 63 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 63 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 64 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 64 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 64 "./atomic/stdatomic-lockfree.c"
 &alla
# 64 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 64 "./atomic/stdatomic-lockfree.c"
 &alla
# 64 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 64 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 64 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 64 "./atomic/stdatomic-lockfree.c"
 &allt
# 64 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 64 "./atomic/stdatomic-lockfree.c"
 &allt
# 64 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 64 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);
  do { int r1 = 2
# 65 "./atomic/stdatomic-lockfree.c"
 ; int r2 = 
# 65 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 65 "./atomic/stdatomic-lockfree.c"
 &apa
# 65 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 65 "./atomic/stdatomic-lockfree.c"
 &apa
# 65 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 65 "./atomic/stdatomic-lockfree.c"
 ; int r3 = 
# 65 "./atomic/stdatomic-lockfree.c" 3 4
 __atomic_is_lock_free (sizeof (*(
# 65 "./atomic/stdatomic-lockfree.c"
 &apa
# 65 "./atomic/stdatomic-lockfree.c" 3 4
 )), (
# 65 "./atomic/stdatomic-lockfree.c"
 &apa
# 65 "./atomic/stdatomic-lockfree.c" 3 4
 ))
# 65 "./atomic/stdatomic-lockfree.c"
 ; if (r1 != 0 && r1 != 1 && r1 != 2) abort (); if (r2 != 0 && r2 != 1) abort (); if (r3 != 0 && r3 != 1) abort (); if (r1 == 2 && r2 != 1) abort (); if (r1 == 2 && r3 != 1) abort (); if (r1 == 0 && r2 != 0) abort (); if (r1 == 0 && r3 != 0) abort (); } while (0);

  return 0;
}
