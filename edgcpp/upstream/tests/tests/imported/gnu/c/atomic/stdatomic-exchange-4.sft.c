//type: rp
//options: --c11
# 0 "./atomic/stdatomic-exchange-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/stdatomic-exchange-4.c"





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
# 7 "./atomic/stdatomic-exchange-4.c" 2


# 8 "./atomic/stdatomic-exchange-4.c"
extern void abort (void);

_Atomic long long v;
long long count, ret;

int
main ()
{
  v = 0;
  count = 0;

  if (
# 19 "./atomic/stdatomic-exchange-4.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 19 "./atomic/stdatomic-exchange-4.c"
     &v
# 19 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 19 "./atomic/stdatomic-exchange-4.c"
     count + 1
# 19 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 19 "./atomic/stdatomic-exchange-4.c"
     memory_order_relaxed
# 19 "./atomic/stdatomic-exchange-4.c" 3 4
     )); __atomic_exchange_tmp; }) 
# 19 "./atomic/stdatomic-exchange-4.c"
                                                                    != count)
    abort ();
  count++;

  if (
# 23 "./atomic/stdatomic-exchange-4.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 23 "./atomic/stdatomic-exchange-4.c"
     &v
# 23 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 23 "./atomic/stdatomic-exchange-4.c"
     count + 1
# 23 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 23 "./atomic/stdatomic-exchange-4.c"
     memory_order_acquire
# 23 "./atomic/stdatomic-exchange-4.c" 3 4
     )); __atomic_exchange_tmp; }) 
# 23 "./atomic/stdatomic-exchange-4.c"
                                                                    != count)
    abort ();
  count++;

  if (
# 27 "./atomic/stdatomic-exchange-4.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 27 "./atomic/stdatomic-exchange-4.c"
     &v
# 27 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 27 "./atomic/stdatomic-exchange-4.c"
     count + 1
# 27 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 27 "./atomic/stdatomic-exchange-4.c"
     memory_order_release
# 27 "./atomic/stdatomic-exchange-4.c" 3 4
     )); __atomic_exchange_tmp; }) 
# 27 "./atomic/stdatomic-exchange-4.c"
                                                                    != count)
    abort ();
  count++;

  if (
# 31 "./atomic/stdatomic-exchange-4.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 31 "./atomic/stdatomic-exchange-4.c"
     &v
# 31 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 31 "./atomic/stdatomic-exchange-4.c"
     count + 1
# 31 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 31 "./atomic/stdatomic-exchange-4.c"
     memory_order_acq_rel
# 31 "./atomic/stdatomic-exchange-4.c" 3 4
     )); __atomic_exchange_tmp; }) 
# 31 "./atomic/stdatomic-exchange-4.c"
                                                                    != count)
    abort ();
  count++;

  if (
# 35 "./atomic/stdatomic-exchange-4.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 35 "./atomic/stdatomic-exchange-4.c"
     &v
# 35 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 35 "./atomic/stdatomic-exchange-4.c"
     count + 1
# 35 "./atomic/stdatomic-exchange-4.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 35 "./atomic/stdatomic-exchange-4.c"
     memory_order_seq_cst
# 35 "./atomic/stdatomic-exchange-4.c" 3 4
     )); __atomic_exchange_tmp; }) 
# 35 "./atomic/stdatomic-exchange-4.c"
                                                                    != count)
    abort ();
  count++;

  count++;

  ret = 
# 41 "./atomic/stdatomic-exchange-4.c" 3 4
       __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 41 "./atomic/stdatomic-exchange-4.c"
       &v
# 41 "./atomic/stdatomic-exchange-4.c" 3 4
       ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 41 "./atomic/stdatomic-exchange-4.c"
       count
# 41 "./atomic/stdatomic-exchange-4.c" 3 4
       ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (5)); __atomic_exchange_tmp; })
# 41 "./atomic/stdatomic-exchange-4.c"
                                  ;
  if (ret != count - 1 || v != count)
    abort ();

  return 0;
}
