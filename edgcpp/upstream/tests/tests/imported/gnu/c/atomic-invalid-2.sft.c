//type: fp
//options: 
# 0 "./atomic-invalid-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic-invalid-2.c"





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

typedef _Atomic unsigned char atomic_char8_t;

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
# 7 "./atomic-invalid-2.c" 2






# 12 "./atomic-invalid-2.c"
void
store (atomic_int *i)
{
  
# 15 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 15 "./atomic-invalid-2.c"
 i
# 15 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 15 "./atomic-invalid-2.c"
 0
# 15 "./atomic-invalid-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (
# 15 "./atomic-invalid-2.c"
 memory_order_consume
# 15 "./atomic-invalid-2.c" 3 4
 )); })
# 15 "./atomic-invalid-2.c"
                                                   ;
  
# 16 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 16 "./atomic-invalid-2.c"
 i
# 16 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 16 "./atomic-invalid-2.c"
 0
# 16 "./atomic-invalid-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (
# 16 "./atomic-invalid-2.c"
 memory_order_acquire
# 16 "./atomic-invalid-2.c" 3 4
 )); })
# 16 "./atomic-invalid-2.c"
                                                   ;
  
# 17 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 17 "./atomic-invalid-2.c"
 i
# 17 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 17 "./atomic-invalid-2.c"
 0
# 17 "./atomic-invalid-2.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (
# 17 "./atomic-invalid-2.c"
 memory_order_acq_rel
# 17 "./atomic-invalid-2.c" 3 4
 )); })
# 17 "./atomic-invalid-2.c"
                                                   ;
}





void
load (atomic_int *i)
{
  atomic_int j = 
# 27 "./atomic-invalid-2.c" 3 4
                __extension__ ({ __auto_type __atomic_load_ptr = (
# 27 "./atomic-invalid-2.c"
                i
# 27 "./atomic-invalid-2.c" 3 4
                ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 27 "./atomic-invalid-2.c"
                memory_order_release
# 27 "./atomic-invalid-2.c" 3 4
                )); __atomic_load_tmp; })
# 27 "./atomic-invalid-2.c"
                                                              ;
  atomic_int k = 
# 28 "./atomic-invalid-2.c" 3 4
                __extension__ ({ __auto_type __atomic_load_ptr = (
# 28 "./atomic-invalid-2.c"
                i
# 28 "./atomic-invalid-2.c" 3 4
                ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 28 "./atomic-invalid-2.c"
                memory_order_acq_rel
# 28 "./atomic-invalid-2.c" 3 4
                )); __atomic_load_tmp; })
# 28 "./atomic-invalid-2.c"
                                                              ;
}






void
exchange (atomic_int *i)
{
  int r;

  
# 41 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 41 "./atomic-invalid-2.c"
 i
# 41 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 41 "./atomic-invalid-2.c"
 0
# 41 "./atomic-invalid-2.c" 3 4
 ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 41 "./atomic-invalid-2.c"
 &r
# 41 "./atomic-invalid-2.c" 3 4
 ), &__atomic_compare_exchange_tmp, 0, (
# 41 "./atomic-invalid-2.c"
 memory_order_seq_cst
# 41 "./atomic-invalid-2.c" 3 4
 ), (
# 41 "./atomic-invalid-2.c"
 memory_order_release
# 41 "./atomic-invalid-2.c" 3 4
 )); })
# 41 "./atomic-invalid-2.c"
                                                                                               ;
  
# 42 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 42 "./atomic-invalid-2.c"
 i
# 42 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 42 "./atomic-invalid-2.c"
 0
# 42 "./atomic-invalid-2.c" 3 4
 ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 42 "./atomic-invalid-2.c"
 &r
# 42 "./atomic-invalid-2.c" 3 4
 ), &__atomic_compare_exchange_tmp, 0, (
# 42 "./atomic-invalid-2.c"
 memory_order_seq_cst
# 42 "./atomic-invalid-2.c" 3 4
 ), (
# 42 "./atomic-invalid-2.c"
 memory_order_acq_rel
# 42 "./atomic-invalid-2.c" 3 4
 )); })
# 42 "./atomic-invalid-2.c"
                                                                                               ;
  
# 43 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 43 "./atomic-invalid-2.c"
 i
# 43 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 43 "./atomic-invalid-2.c"
 0
# 43 "./atomic-invalid-2.c" 3 4
 ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 43 "./atomic-invalid-2.c"
 &r
# 43 "./atomic-invalid-2.c" 3 4
 ), &__atomic_compare_exchange_tmp, 0, (
# 43 "./atomic-invalid-2.c"
 memory_order_relaxed
# 43 "./atomic-invalid-2.c" 3 4
 ), (
# 43 "./atomic-invalid-2.c"
 memory_order_consume
# 43 "./atomic-invalid-2.c" 3 4
 )); })
# 43 "./atomic-invalid-2.c"
                                                                                               ;

  
# 45 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 45 "./atomic-invalid-2.c"
 i
# 45 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 45 "./atomic-invalid-2.c"
 0
# 45 "./atomic-invalid-2.c" 3 4
 ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 45 "./atomic-invalid-2.c"
 &r
# 45 "./atomic-invalid-2.c" 3 4
 ), &__atomic_compare_exchange_tmp, 1, (
# 45 "./atomic-invalid-2.c"
 memory_order_seq_cst
# 45 "./atomic-invalid-2.c" 3 4
 ), (
# 45 "./atomic-invalid-2.c"
 memory_order_release
# 45 "./atomic-invalid-2.c" 3 4
 )); })
# 45 "./atomic-invalid-2.c"
                                                                                             ;
  
# 46 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 46 "./atomic-invalid-2.c"
 i
# 46 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 46 "./atomic-invalid-2.c"
 0
# 46 "./atomic-invalid-2.c" 3 4
 ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 46 "./atomic-invalid-2.c"
 &r
# 46 "./atomic-invalid-2.c" 3 4
 ), &__atomic_compare_exchange_tmp, 1, (
# 46 "./atomic-invalid-2.c"
 memory_order_seq_cst
# 46 "./atomic-invalid-2.c" 3 4
 ), (
# 46 "./atomic-invalid-2.c"
 memory_order_acq_rel
# 46 "./atomic-invalid-2.c" 3 4
 )); })
# 46 "./atomic-invalid-2.c"
                                                                                             ;
  
# 47 "./atomic-invalid-2.c" 3 4
 __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 47 "./atomic-invalid-2.c"
 i
# 47 "./atomic-invalid-2.c" 3 4
 ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 47 "./atomic-invalid-2.c"
 0
# 47 "./atomic-invalid-2.c" 3 4
 ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 47 "./atomic-invalid-2.c"
 &r
# 47 "./atomic-invalid-2.c" 3 4
 ), &__atomic_compare_exchange_tmp, 1, (
# 47 "./atomic-invalid-2.c"
 memory_order_relaxed
# 47 "./atomic-invalid-2.c" 3 4
 ), (
# 47 "./atomic-invalid-2.c"
 memory_order_consume
# 47 "./atomic-invalid-2.c" 3 4
 )); })
# 47 "./atomic-invalid-2.c"
                                                                                             ;
}





void
clear (atomic_int *i)
{
  
# 57 "./atomic-invalid-2.c" 3 4
 __atomic_clear ((
# 57 "./atomic-invalid-2.c"
 i
# 57 "./atomic-invalid-2.c" 3 4
 ), (
# 57 "./atomic-invalid-2.c"
 memory_order_acquire
# 57 "./atomic-invalid-2.c" 3 4
 ))
# 57 "./atomic-invalid-2.c"
                                                     ;
  
# 58 "./atomic-invalid-2.c" 3 4
 __atomic_clear ((
# 58 "./atomic-invalid-2.c"
 i
# 58 "./atomic-invalid-2.c" 3 4
 ), (
# 58 "./atomic-invalid-2.c"
 memory_order_acq_rel
# 58 "./atomic-invalid-2.c" 3 4
 ))
# 58 "./atomic-invalid-2.c"
                                                     ;
}
