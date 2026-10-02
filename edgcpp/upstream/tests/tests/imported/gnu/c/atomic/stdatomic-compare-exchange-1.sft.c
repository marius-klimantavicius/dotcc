//type: rp
//options: --c11
# 0 "./atomic/stdatomic-compare-exchange-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/stdatomic-compare-exchange-1.c"





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
# 7 "./atomic/stdatomic-compare-exchange-1.c" 2


# 8 "./atomic/stdatomic-compare-exchange-1.c"
extern void abort (void);

_Atomic char v = 
# 10 "./atomic/stdatomic-compare-exchange-1.c" 3 4
                (
# 10 "./atomic/stdatomic-compare-exchange-1.c"
                0
# 10 "./atomic/stdatomic-compare-exchange-1.c" 3 4
                )
# 10 "./atomic/stdatomic-compare-exchange-1.c"
                                   ;
char expected = 0;
char max = ~0;
char desired = ~0;
char zero = 0;

int
main ()
{

  if (!
# 20 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 20 "./atomic/stdatomic-compare-exchange-1.c"
      &v
# 20 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 20 "./atomic/stdatomic-compare-exchange-1.c"
      max
# 20 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 20 "./atomic/stdatomic-compare-exchange-1.c"
      &expected
# 20 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (
# 20 "./atomic/stdatomic-compare-exchange-1.c"
      memory_order_relaxed
# 20 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), (
# 20 "./atomic/stdatomic-compare-exchange-1.c"
      memory_order_relaxed
# 20 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      )); })
# 20 "./atomic/stdatomic-compare-exchange-1.c"
                                                                                                              )
    abort ();
  if (expected != 0)
    abort ();

  if (
# 25 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 25 "./atomic/stdatomic-compare-exchange-1.c"
     &v
# 25 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 25 "./atomic/stdatomic-compare-exchange-1.c"
     0
# 25 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 25 "./atomic/stdatomic-compare-exchange-1.c"
     &expected
# 25 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ), &__atomic_compare_exchange_tmp, 0, (
# 25 "./atomic/stdatomic-compare-exchange-1.c"
     memory_order_acquire
# 25 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ), (
# 25 "./atomic/stdatomic-compare-exchange-1.c"
     memory_order_relaxed
# 25 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     )); })
# 25 "./atomic/stdatomic-compare-exchange-1.c"
                                                                                                           )
    abort ();
  if (expected != max)
    abort ();

  if (!
# 30 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 30 "./atomic/stdatomic-compare-exchange-1.c"
      &v
# 30 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 30 "./atomic/stdatomic-compare-exchange-1.c"
      0
# 30 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 30 "./atomic/stdatomic-compare-exchange-1.c"
      &expected
# 30 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (
# 30 "./atomic/stdatomic-compare-exchange-1.c"
      memory_order_release
# 30 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), (
# 30 "./atomic/stdatomic-compare-exchange-1.c"
      memory_order_acquire
# 30 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      )); })
# 30 "./atomic/stdatomic-compare-exchange-1.c"
                                                                                                            )
    abort ();
  if (expected != max)
    abort ();
  if (v != 0)
    abort ();

  if (
# 37 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 37 "./atomic/stdatomic-compare-exchange-1.c"
     &v
# 37 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 37 "./atomic/stdatomic-compare-exchange-1.c"
     desired
# 37 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 37 "./atomic/stdatomic-compare-exchange-1.c"
     &expected
# 37 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ), &__atomic_compare_exchange_tmp, 1, (
# 37 "./atomic/stdatomic-compare-exchange-1.c"
     memory_order_acq_rel
# 37 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ), (
# 37 "./atomic/stdatomic-compare-exchange-1.c"
     memory_order_acquire
# 37 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     )); })
# 37 "./atomic/stdatomic-compare-exchange-1.c"
                                                                                                               )
    abort ();
  if (expected != 0)
    abort ();

  if (!
# 42 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 42 "./atomic/stdatomic-compare-exchange-1.c"
      &v
# 42 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 42 "./atomic/stdatomic-compare-exchange-1.c"
      desired
# 42 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 42 "./atomic/stdatomic-compare-exchange-1.c"
      &expected
# 42 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (
# 42 "./atomic/stdatomic-compare-exchange-1.c"
      memory_order_seq_cst
# 42 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), (
# 42 "./atomic/stdatomic-compare-exchange-1.c"
      memory_order_seq_cst
# 42 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      )); })
# 42 "./atomic/stdatomic-compare-exchange-1.c"
                                                                                                                  )
    abort ();
  if (expected != 0)
    abort ();
  if (v != max)
    abort ();

  v = 0;

  if (!
# 51 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 51 "./atomic/stdatomic-compare-exchange-1.c"
      &v
# 51 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 51 "./atomic/stdatomic-compare-exchange-1.c"
      max
# 51 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 51 "./atomic/stdatomic-compare-exchange-1.c"
      &expected
# 51 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
# 51 "./atomic/stdatomic-compare-exchange-1.c"
                                                         )
    abort ();
  if (expected != 0)
    abort ();

  if (
# 56 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 56 "./atomic/stdatomic-compare-exchange-1.c"
     &v
# 56 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 56 "./atomic/stdatomic-compare-exchange-1.c"
     zero
# 56 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 56 "./atomic/stdatomic-compare-exchange-1.c"
     &expected
# 56 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
# 56 "./atomic/stdatomic-compare-exchange-1.c"
                                                         )
    abort ();
  if (expected != max)
    abort ();

  if (!
# 61 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 61 "./atomic/stdatomic-compare-exchange-1.c"
      &v
# 61 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 61 "./atomic/stdatomic-compare-exchange-1.c"
      zero
# 61 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 61 "./atomic/stdatomic-compare-exchange-1.c"
      &expected
# 61 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
# 61 "./atomic/stdatomic-compare-exchange-1.c"
                                                          )
    abort ();
  if (expected != max)
    abort ();
  if (v != 0)
    abort ();

  if (
# 68 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 68 "./atomic/stdatomic-compare-exchange-1.c"
     &v
# 68 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 68 "./atomic/stdatomic-compare-exchange-1.c"
     desired
# 68 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 68 "./atomic/stdatomic-compare-exchange-1.c"
     &expected
# 68 "./atomic/stdatomic-compare-exchange-1.c" 3 4
     ), &__atomic_compare_exchange_tmp, 1, (5), (5)); })
# 68 "./atomic/stdatomic-compare-exchange-1.c"
                                                          )
    abort ();
  if (expected != 0)
    abort ();

  if (!
# 73 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 73 "./atomic/stdatomic-compare-exchange-1.c"
      &v
# 73 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 73 "./atomic/stdatomic-compare-exchange-1.c"
      desired
# 73 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 73 "./atomic/stdatomic-compare-exchange-1.c"
      &expected
# 73 "./atomic/stdatomic-compare-exchange-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
# 73 "./atomic/stdatomic-compare-exchange-1.c"
                                                             )
    abort ();
  if (expected != 0)
    abort ();
  if (v != max)
    abort ();

  return 0;
}
