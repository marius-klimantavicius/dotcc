//type: rp
//options: --c11
# 0 "./atomic/stdatomic-fence.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/stdatomic-fence.c"





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
# 7 "./atomic/stdatomic-fence.c" 2


# 8 "./atomic/stdatomic-fence.c"
int
main ()
{
  
# 11 "./atomic/stdatomic-fence.c" 3 4
 __atomic_thread_fence (
# 11 "./atomic/stdatomic-fence.c"
 memory_order_relaxed
# 11 "./atomic/stdatomic-fence.c" 3 4
 )
# 11 "./atomic/stdatomic-fence.c"
                                           ;
  
# 12 "./atomic/stdatomic-fence.c" 3 4
 __atomic_thread_fence (
# 12 "./atomic/stdatomic-fence.c"
 memory_order_consume
# 12 "./atomic/stdatomic-fence.c" 3 4
 )
# 12 "./atomic/stdatomic-fence.c"
                                           ;
  
# 13 "./atomic/stdatomic-fence.c" 3 4
 __atomic_thread_fence (
# 13 "./atomic/stdatomic-fence.c"
 memory_order_acquire
# 13 "./atomic/stdatomic-fence.c" 3 4
 )
# 13 "./atomic/stdatomic-fence.c"
                                           ;
  
# 14 "./atomic/stdatomic-fence.c" 3 4
 __atomic_thread_fence (
# 14 "./atomic/stdatomic-fence.c"
 memory_order_release
# 14 "./atomic/stdatomic-fence.c" 3 4
 )
# 14 "./atomic/stdatomic-fence.c"
                                           ;
  
# 15 "./atomic/stdatomic-fence.c" 3 4
 __atomic_thread_fence (
# 15 "./atomic/stdatomic-fence.c"
 memory_order_acq_rel
# 15 "./atomic/stdatomic-fence.c" 3 4
 )
# 15 "./atomic/stdatomic-fence.c"
                                           ;
  
# 16 "./atomic/stdatomic-fence.c" 3 4
 __atomic_thread_fence (
# 16 "./atomic/stdatomic-fence.c"
 memory_order_seq_cst
# 16 "./atomic/stdatomic-fence.c" 3 4
 )
# 16 "./atomic/stdatomic-fence.c"
                                           ;

  
# 18 "./atomic/stdatomic-fence.c" 3 4
 __atomic_signal_fence (
# 18 "./atomic/stdatomic-fence.c"
 memory_order_relaxed
# 18 "./atomic/stdatomic-fence.c" 3 4
 )
# 18 "./atomic/stdatomic-fence.c"
                                           ;
  
# 19 "./atomic/stdatomic-fence.c" 3 4
 __atomic_signal_fence (
# 19 "./atomic/stdatomic-fence.c"
 memory_order_consume
# 19 "./atomic/stdatomic-fence.c" 3 4
 )
# 19 "./atomic/stdatomic-fence.c"
                                           ;
  
# 20 "./atomic/stdatomic-fence.c" 3 4
 __atomic_signal_fence (
# 20 "./atomic/stdatomic-fence.c"
 memory_order_acquire
# 20 "./atomic/stdatomic-fence.c" 3 4
 )
# 20 "./atomic/stdatomic-fence.c"
                                           ;
  
# 21 "./atomic/stdatomic-fence.c" 3 4
 __atomic_signal_fence (
# 21 "./atomic/stdatomic-fence.c"
 memory_order_release
# 21 "./atomic/stdatomic-fence.c" 3 4
 )
# 21 "./atomic/stdatomic-fence.c"
                                           ;
  
# 22 "./atomic/stdatomic-fence.c" 3 4
 __atomic_signal_fence (
# 22 "./atomic/stdatomic-fence.c"
 memory_order_acq_rel
# 22 "./atomic/stdatomic-fence.c" 3 4
 )
# 22 "./atomic/stdatomic-fence.c"
                                           ;
  
# 23 "./atomic/stdatomic-fence.c" 3 4
 __atomic_signal_fence (
# 23 "./atomic/stdatomic-fence.c"
 memory_order_seq_cst
# 23 "./atomic/stdatomic-fence.c" 3 4
 )
# 23 "./atomic/stdatomic-fence.c"
                                           ;

  return 0;
}
