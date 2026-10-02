//type: fp
//options: --c11
# 0 "./c11-stdatomic-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-stdatomic-1.c"




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
# 6 "./c11-stdatomic-1.c" 2
# 47 "./c11-stdatomic-1.c"

# 47 "./c11-stdatomic-1.c"
memory_order m0 = memory_order_relaxed;
memory_order m1 = memory_order_consume;
memory_order m2 = memory_order_acquire;
memory_order m3 = memory_order_release;
memory_order m4 = memory_order_acq_rel;
memory_order m5 = memory_order_seq_cst;

atomic_flag af = 
# 54 "./c11-stdatomic-1.c" 3 4
                { 0 }
# 54 "./c11-stdatomic-1.c"
                                ;

struct s { int i[100]; } sv;
void
f (void)
{
  _Atomic struct s sva = 
# 60 "./c11-stdatomic-1.c" 3 4
                        (
# 60 "./c11-stdatomic-1.c"
                        sv
# 60 "./c11-stdatomic-1.c" 3 4
                        )
# 60 "./c11-stdatomic-1.c"
                                            ;
}
# 76 "./c11-stdatomic-1.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 77 "./c11-stdatomic-1.c" 2
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
# 78 "./c11-stdatomic-1.c" 2


# 79 "./c11-stdatomic-1.c"
void
check_typedefs (void)
{
  do { atomic_bool v; char array1[sizeof (atomic_bool) == sizeof (_Atomic _Bool) ? 1 : -1]; char array2[_Alignof (atomic_bool) == _Alignof (_Atomic _Bool) ? 1 : -1]; } while (0);
  do { atomic_char v; char array1[sizeof (atomic_char) == sizeof (_Atomic char) ? 1 : -1]; char array2[_Alignof (atomic_char) == _Alignof (_Atomic char) ? 1 : -1]; } while (0);
  do { atomic_schar v; char array1[sizeof (atomic_schar) == sizeof (_Atomic signed char) ? 1 : -1]; char array2[_Alignof (atomic_schar) == _Alignof (_Atomic signed char) ? 1 : -1]; } while (0);
  do { atomic_uchar v; char array1[sizeof (atomic_uchar) == sizeof (_Atomic unsigned char) ? 1 : -1]; char array2[_Alignof (atomic_uchar) == _Alignof (_Atomic unsigned char) ? 1 : -1]; } while (0);
  do { atomic_short v; char array1[sizeof (atomic_short) == sizeof (_Atomic short) ? 1 : -1]; char array2[_Alignof (atomic_short) == _Alignof (_Atomic short) ? 1 : -1]; } while (0);
  do { atomic_ushort v; char array1[sizeof (atomic_ushort) == sizeof (_Atomic unsigned short) ? 1 : -1]; char array2[_Alignof (atomic_ushort) == _Alignof (_Atomic unsigned short) ? 1 : -1]; } while (0);
  do { atomic_int v; char array1[sizeof (atomic_int) == sizeof (_Atomic int) ? 1 : -1]; char array2[_Alignof (atomic_int) == _Alignof (_Atomic int) ? 1 : -1]; } while (0);
  do { atomic_uint v; char array1[sizeof (atomic_uint) == sizeof (_Atomic unsigned int) ? 1 : -1]; char array2[_Alignof (atomic_uint) == _Alignof (_Atomic unsigned int) ? 1 : -1]; } while (0);
  do { atomic_long v; char array1[sizeof (atomic_long) == sizeof (_Atomic long) ? 1 : -1]; char array2[_Alignof (atomic_long) == _Alignof (_Atomic long) ? 1 : -1]; } while (0);
  do { atomic_ulong v; char array1[sizeof (atomic_ulong) == sizeof (_Atomic unsigned long) ? 1 : -1]; char array2[_Alignof (atomic_ulong) == _Alignof (_Atomic unsigned long) ? 1 : -1]; } while (0);
  do { atomic_llong v; char array1[sizeof (atomic_llong) == sizeof (_Atomic long long) ? 1 : -1]; char array2[_Alignof (atomic_llong) == _Alignof (_Atomic long long) ? 1 : -1]; } while (0);
  do { atomic_ullong v; char array1[sizeof (atomic_ullong) == sizeof (_Atomic unsigned long long) ? 1 : -1]; char array2[_Alignof (atomic_ullong) == _Alignof (_Atomic unsigned long long) ? 1 : -1]; } while (0);
  do { atomic_char16_t v; char array1[sizeof (atomic_char16_t) == sizeof (_Atomic short unsigned int) ? 1 : -1]; char array2[_Alignof (atomic_char16_t) == _Alignof (_Atomic short unsigned int) ? 1 : -1]; } while (0);
  do { atomic_char32_t v; char array1[sizeof (atomic_char32_t) == sizeof (_Atomic unsigned int) ? 1 : -1]; char array2[_Alignof (atomic_char32_t) == _Alignof (_Atomic unsigned int) ? 1 : -1]; } while (0);
  do { atomic_wchar_t v; char array1[sizeof (atomic_wchar_t) == sizeof (_Atomic wchar_t) ? 1 : -1]; char array2[_Alignof (atomic_wchar_t) == _Alignof (_Atomic wchar_t) ? 1 : -1]; } while (0);
  do { atomic_int_least8_t v; char array1[sizeof (atomic_int_least8_t) == sizeof (_Atomic int_least8_t) ? 1 : -1]; char array2[_Alignof (atomic_int_least8_t) == _Alignof (_Atomic int_least8_t) ? 1 : -1]; } while (0);
  do { atomic_uint_least8_t v; char array1[sizeof (atomic_uint_least8_t) == sizeof (_Atomic uint_least8_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_least8_t) == _Alignof (_Atomic uint_least8_t) ? 1 : -1]; } while (0);
  do { atomic_int_least16_t v; char array1[sizeof (atomic_int_least16_t) == sizeof (_Atomic int_least16_t) ? 1 : -1]; char array2[_Alignof (atomic_int_least16_t) == _Alignof (_Atomic int_least16_t) ? 1 : -1]; } while (0);
  do { atomic_uint_least16_t v; char array1[sizeof (atomic_uint_least16_t) == sizeof (_Atomic uint_least16_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_least16_t) == _Alignof (_Atomic uint_least16_t) ? 1 : -1]; } while (0);
  do { atomic_int_least32_t v; char array1[sizeof (atomic_int_least32_t) == sizeof (_Atomic int_least32_t) ? 1 : -1]; char array2[_Alignof (atomic_int_least32_t) == _Alignof (_Atomic int_least32_t) ? 1 : -1]; } while (0);
  do { atomic_uint_least32_t v; char array1[sizeof (atomic_uint_least32_t) == sizeof (_Atomic uint_least32_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_least32_t) == _Alignof (_Atomic uint_least32_t) ? 1 : -1]; } while (0);
  do { atomic_int_least64_t v; char array1[sizeof (atomic_int_least64_t) == sizeof (_Atomic int_least64_t) ? 1 : -1]; char array2[_Alignof (atomic_int_least64_t) == _Alignof (_Atomic int_least64_t) ? 1 : -1]; } while (0);
  do { atomic_uint_least64_t v; char array1[sizeof (atomic_uint_least64_t) == sizeof (_Atomic uint_least64_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_least64_t) == _Alignof (_Atomic uint_least64_t) ? 1 : -1]; } while (0);
  do { atomic_int_fast8_t v; char array1[sizeof (atomic_int_fast8_t) == sizeof (_Atomic int_fast8_t) ? 1 : -1]; char array2[_Alignof (atomic_int_fast8_t) == _Alignof (_Atomic int_fast8_t) ? 1 : -1]; } while (0);
  do { atomic_uint_fast8_t v; char array1[sizeof (atomic_uint_fast8_t) == sizeof (_Atomic uint_fast8_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_fast8_t) == _Alignof (_Atomic uint_fast8_t) ? 1 : -1]; } while (0);
  do { atomic_int_fast16_t v; char array1[sizeof (atomic_int_fast16_t) == sizeof (_Atomic int_fast16_t) ? 1 : -1]; char array2[_Alignof (atomic_int_fast16_t) == _Alignof (_Atomic int_fast16_t) ? 1 : -1]; } while (0);
  do { atomic_uint_fast16_t v; char array1[sizeof (atomic_uint_fast16_t) == sizeof (_Atomic uint_fast16_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_fast16_t) == _Alignof (_Atomic uint_fast16_t) ? 1 : -1]; } while (0);
  do { atomic_int_fast32_t v; char array1[sizeof (atomic_int_fast32_t) == sizeof (_Atomic int_fast32_t) ? 1 : -1]; char array2[_Alignof (atomic_int_fast32_t) == _Alignof (_Atomic int_fast32_t) ? 1 : -1]; } while (0);
  do { atomic_uint_fast32_t v; char array1[sizeof (atomic_uint_fast32_t) == sizeof (_Atomic uint_fast32_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_fast32_t) == _Alignof (_Atomic uint_fast32_t) ? 1 : -1]; } while (0);
  do { atomic_int_fast64_t v; char array1[sizeof (atomic_int_fast64_t) == sizeof (_Atomic int_fast64_t) ? 1 : -1]; char array2[_Alignof (atomic_int_fast64_t) == _Alignof (_Atomic int_fast64_t) ? 1 : -1]; } while (0);
  do { atomic_uint_fast64_t v; char array1[sizeof (atomic_uint_fast64_t) == sizeof (_Atomic uint_fast64_t) ? 1 : -1]; char array2[_Alignof (atomic_uint_fast64_t) == _Alignof (_Atomic uint_fast64_t) ? 1 : -1]; } while (0);
  do { atomic_intptr_t v; char array1[sizeof (atomic_intptr_t) == sizeof (_Atomic intptr_t) ? 1 : -1]; char array2[_Alignof (atomic_intptr_t) == _Alignof (_Atomic intptr_t) ? 1 : -1]; } while (0);
  do { atomic_uintptr_t v; char array1[sizeof (atomic_uintptr_t) == sizeof (_Atomic uintptr_t) ? 1 : -1]; char array2[_Alignof (atomic_uintptr_t) == _Alignof (_Atomic uintptr_t) ? 1 : -1]; } while (0);
  do { atomic_size_t v; char array1[sizeof (atomic_size_t) == sizeof (_Atomic size_t) ? 1 : -1]; char array2[_Alignof (atomic_size_t) == _Alignof (_Atomic size_t) ? 1 : -1]; } while (0);
  do { atomic_ptrdiff_t v; char array1[sizeof (atomic_ptrdiff_t) == sizeof (_Atomic ptrdiff_t) ? 1 : -1]; char array2[_Alignof (atomic_ptrdiff_t) == _Alignof (_Atomic ptrdiff_t) ? 1 : -1]; } while (0);
  do { atomic_intmax_t v; char array1[sizeof (atomic_intmax_t) == sizeof (_Atomic intmax_t) ? 1 : -1]; char array2[_Alignof (atomic_intmax_t) == _Alignof (_Atomic intmax_t) ? 1 : -1]; } while (0);
  do { atomic_uintmax_t v; char array1[sizeof (atomic_uintmax_t) == sizeof (_Atomic uintmax_t) ? 1 : -1]; char array2[_Alignof (atomic_uintmax_t) == _Alignof (_Atomic uintmax_t) ? 1 : -1]; } while (0);
}
