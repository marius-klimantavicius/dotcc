//type: rp
//options: --c23
# 0 "./atomic/stdatomic-bitint-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./atomic/stdatomic-bitint-1.c"




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
# 6 "./atomic/stdatomic-bitint-1.c" 2


# 7 "./atomic/stdatomic-bitint-1.c"
extern void abort (void);


_Atomic _BitInt(127) v;
_BitInt(127) count, res;
const _BitInt(127) init = ~(_BitInt(127)) 0wb;

void
test_fetch_add ()
{
  
# 17 "./atomic/stdatomic-bitint-1.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 17 "./atomic/stdatomic-bitint-1.c"
 &v
# 17 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 17 "./atomic/stdatomic-bitint-1.c"
 13505789527944801758751150119415226784wb
# 17 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (0)); })
# 17 "./atomic/stdatomic-bitint-1.c"
                                                           ;
  count = -64910836855286429164283779649638556795wb;

  if (
# 20 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 20 "./atomic/stdatomic-bitint-1.c"
     &v
# 20 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 20 "./atomic/stdatomic-bitint-1.c"
     count
# 20 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 20 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 20 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 21 "./atomic/stdatomic-bitint-1.c"
     != 13505789527944801758751150119415226784wb)
    abort ();

  if (
# 24 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 24 "./atomic/stdatomic-bitint-1.c"
     &v
# 24 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 24 "./atomic/stdatomic-bitint-1.c"
     2227507280963412295355244564739509222wb
# 24 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 24 "./atomic/stdatomic-bitint-1.c"
     memory_order_consume
# 24 "./atomic/stdatomic-bitint-1.c" 3 4
     ))

      
# 26 "./atomic/stdatomic-bitint-1.c"
     != -51405047327341627405532629530223330011wb)
    abort ();

  if (
# 29 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 29 "./atomic/stdatomic-bitint-1.c"
     &v
# 29 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 29 "./atomic/stdatomic-bitint-1.c"
     count
# 29 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 29 "./atomic/stdatomic-bitint-1.c"
     memory_order_acquire
# 29 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 30 "./atomic/stdatomic-bitint-1.c"
     != -49177540046378215110177384965483820789wb)
    abort ();

  if (
# 33 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 33 "./atomic/stdatomic-bitint-1.c"
     &v
# 33 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 33 "./atomic/stdatomic-bitint-1.c"
     42245667388877614520169143618236120405wb
# 33 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 33 "./atomic/stdatomic-bitint-1.c"
     memory_order_release
# 33 "./atomic/stdatomic-bitint-1.c" 3 4
     ))

      
# 35 "./atomic/stdatomic-bitint-1.c"
     != 56052806558804587457226139100761728144wb)
    abort ();

  if (
# 38 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 38 "./atomic/stdatomic-bitint-1.c"
     &v
# 38 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 38 "./atomic/stdatomic-bitint-1.c"
     count
# 38 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 38 "./atomic/stdatomic-bitint-1.c"
     memory_order_acq_rel
# 38 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 39 "./atomic/stdatomic-bitint-1.c"
     != -71842709512787029754292020996886257179wb)
    abort ();

  if (
# 42 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 42 "./atomic/stdatomic-bitint-1.c"
     &v
# 42 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 42 "./atomic/stdatomic-bitint-1.c"
     77995075987640754057679086146674392947wb
# 42 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 42 "./atomic/stdatomic-bitint-1.c"
     memory_order_seq_cst
# 42 "./atomic/stdatomic-bitint-1.c" 3 4
     ))

      
# 44 "./atomic/stdatomic-bitint-1.c"
     != 33387637092395772813111503069359291754wb)
    abort ();

  if (
# 47 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_add ((
# 47 "./atomic/stdatomic-bitint-1.c"
     &v
# 47 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 47 "./atomic/stdatomic-bitint-1.c"
     11810767284628435493328779846084830297wb
# 47 "./atomic/stdatomic-bitint-1.c" 3 4
     ), 5)
      
# 48 "./atomic/stdatomic-bitint-1.c"
     != -58758470380432704860896714499850421027wb)
    abort ();

  if (
# 51 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 51 "./atomic/stdatomic-bitint-1.c"
     &v
# 51 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 51 "./atomic/stdatomic-bitint-1.c"
                      != -46947703095804269367567934653765590730wb)
    abort ();
}

void
test_fetch_sub ()
{
  
# 58 "./atomic/stdatomic-bitint-1.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 58 "./atomic/stdatomic-bitint-1.c"
 &v
# 58 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 58 "./atomic/stdatomic-bitint-1.c"
 30796781768365552851024605388374299173wb
# 58 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (
# 58 "./atomic/stdatomic-bitint-1.c"
 memory_order_release
# 58 "./atomic/stdatomic-bitint-1.c" 3 4
 )); })
                         
# 59 "./atomic/stdatomic-bitint-1.c"
                        ;
  count = 32457177597484647488149720668185011722wb;

  if (
# 62 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 62 "./atomic/stdatomic-bitint-1.c"
     &v
# 62 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 62 "./atomic/stdatomic-bitint-1.c"
     count
# 62 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 62 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 62 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 63 "./atomic/stdatomic-bitint-1.c"
     != 30796781768365552851024605388374299173wb)
    abort ();

  if (
# 66 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 66 "./atomic/stdatomic-bitint-1.c"
     &v
# 66 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 66 "./atomic/stdatomic-bitint-1.c"
     54614103079293459991417218347656369566wb
# 66 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 66 "./atomic/stdatomic-bitint-1.c"
     memory_order_consume
# 66 "./atomic/stdatomic-bitint-1.c" 3 4
     ))

      
# 68 "./atomic/stdatomic-bitint-1.c"
     != -1660395829119094637125115279810712549wb)
    abort ();

  if (
# 71 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 71 "./atomic/stdatomic-bitint-1.c"
     &v
# 71 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 71 "./atomic/stdatomic-bitint-1.c"
     count
# 71 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 71 "./atomic/stdatomic-bitint-1.c"
     memory_order_acquire
# 71 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 72 "./atomic/stdatomic-bitint-1.c"
     != -56274498908412554628542333627467082115wb)
    abort ();

  if (
# 75 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 75 "./atomic/stdatomic-bitint-1.c"
     &v
# 75 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 75 "./atomic/stdatomic-bitint-1.c"
     -44514083923735151931107302009741400482wb
# 75 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 75 "./atomic/stdatomic-bitint-1.c"
     memory_order_release
# 75 "./atomic/stdatomic-bitint-1.c" 3 4
     ))

      
# 77 "./atomic/stdatomic-bitint-1.c"
     != 81409506954572029614995249420232011891wb)
    abort ();

  if (
# 80 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 80 "./atomic/stdatomic-bitint-1.c"
     &v
# 80 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 80 "./atomic/stdatomic-bitint-1.c"
     count
# 80 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 80 "./atomic/stdatomic-bitint-1.c"
     memory_order_acq_rel
# 80 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 81 "./atomic/stdatomic-bitint-1.c"
     != -44217592582162050185584752285910693355wb)
    abort ();

  if (
# 84 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 84 "./atomic/stdatomic-bitint-1.c"
     &v
# 84 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 84 "./atomic/stdatomic-bitint-1.c"
     30348078982452392099140613411731040827wb
# 84 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 84 "./atomic/stdatomic-bitint-1.c"
     memory_order_seq_cst
# 84 "./atomic/stdatomic-bitint-1.c" 3 4
     ))

      
# 86 "./atomic/stdatomic-bitint-1.c"
     != -76674770179646697673734472954095705077wb)
    abort ();

  if (
# 89 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_sub ((
# 89 "./atomic/stdatomic-bitint-1.c"
     &v
# 89 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 89 "./atomic/stdatomic-bitint-1.c"
     -82224045897086857020012824788652775087wb
# 89 "./atomic/stdatomic-bitint-1.c" 3 4
     ), 5)
      
# 90 "./atomic/stdatomic-bitint-1.c"
     != 63118334298370141958812217350057359824wb)
    abort ();

  if (
# 93 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 93 "./atomic/stdatomic-bitint-1.c"
     &v
# 93 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 93 "./atomic/stdatomic-bitint-1.c"
     memory_order_acquire
# 93 "./atomic/stdatomic-bitint-1.c" 3 4
     )); __atomic_load_tmp; })
      
# 94 "./atomic/stdatomic-bitint-1.c"
     != -24798803265012232752862261577173970817wb)
    abort ();
}

void
test_fetch_and ()
{
  
# 101 "./atomic/stdatomic-bitint-1.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 101 "./atomic/stdatomic-bitint-1.c"
 &v
# 101 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 101 "./atomic/stdatomic-bitint-1.c"
 init
# 101 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (5)); })
# 101 "./atomic/stdatomic-bitint-1.c"
                        ;

  if (
# 103 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 103 "./atomic/stdatomic-bitint-1.c"
     &v
# 103 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 103 "./atomic/stdatomic-bitint-1.c"
     0
# 103 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 103 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 103 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 103 "./atomic/stdatomic-bitint-1.c"
                                                             != init)
    abort ();

  if (
# 106 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 106 "./atomic/stdatomic-bitint-1.c"
     &v
# 106 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 106 "./atomic/stdatomic-bitint-1.c"
     init
# 106 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 106 "./atomic/stdatomic-bitint-1.c"
     memory_order_consume
# 106 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 106 "./atomic/stdatomic-bitint-1.c"
                                                                != 0)
    abort ();

  if (
# 109 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 109 "./atomic/stdatomic-bitint-1.c"
     &v
# 109 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 109 "./atomic/stdatomic-bitint-1.c"
     0
# 109 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 109 "./atomic/stdatomic-bitint-1.c"
     memory_order_acquire
# 109 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 109 "./atomic/stdatomic-bitint-1.c"
                                                             != 0)
    abort ();

  v = ~v;
  if (
# 113 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 113 "./atomic/stdatomic-bitint-1.c"
     &v
# 113 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 113 "./atomic/stdatomic-bitint-1.c"
     init
# 113 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 113 "./atomic/stdatomic-bitint-1.c"
     memory_order_release
# 113 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 113 "./atomic/stdatomic-bitint-1.c"
                                                                != init)
    abort ();

  if (
# 116 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 116 "./atomic/stdatomic-bitint-1.c"
     &v
# 116 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 116 "./atomic/stdatomic-bitint-1.c"
     0
# 116 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 116 "./atomic/stdatomic-bitint-1.c"
     memory_order_acq_rel
# 116 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 116 "./atomic/stdatomic-bitint-1.c"
                                                             != init)
    abort ();

  if (
# 119 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 119 "./atomic/stdatomic-bitint-1.c"
     &v
# 119 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 119 "./atomic/stdatomic-bitint-1.c"
     0
# 119 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 119 "./atomic/stdatomic-bitint-1.c"
     memory_order_seq_cst
# 119 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 119 "./atomic/stdatomic-bitint-1.c"
                                                             != 0)
    abort ();

  if (
# 122 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_and ((
# 122 "./atomic/stdatomic-bitint-1.c"
     &v
# 122 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 122 "./atomic/stdatomic-bitint-1.c"
     0
# 122 "./atomic/stdatomic-bitint-1.c" 3 4
     ), 5) 
# 122 "./atomic/stdatomic-bitint-1.c"
                              != 0)
    abort ();
}

void
test_fetch_xor ()
{
  v = init;
  count = 0;

  if (
# 132 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 132 "./atomic/stdatomic-bitint-1.c"
     &v
# 132 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 132 "./atomic/stdatomic-bitint-1.c"
     count
# 132 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 132 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 132 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 132 "./atomic/stdatomic-bitint-1.c"
                                                                 != init)
    abort ();

  if (
# 135 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 135 "./atomic/stdatomic-bitint-1.c"
     &v
# 135 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 135 "./atomic/stdatomic-bitint-1.c"
     ~count
# 135 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 135 "./atomic/stdatomic-bitint-1.c"
     memory_order_consume
# 135 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 135 "./atomic/stdatomic-bitint-1.c"
                                                                  != init)
    abort ();

  if (
# 138 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 138 "./atomic/stdatomic-bitint-1.c"
     &v
# 138 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 138 "./atomic/stdatomic-bitint-1.c"
     0
# 138 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 138 "./atomic/stdatomic-bitint-1.c"
     memory_order_acquire
# 138 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 138 "./atomic/stdatomic-bitint-1.c"
                                                             != 0)
    abort ();

  if (
# 141 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 141 "./atomic/stdatomic-bitint-1.c"
     &v
# 141 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 141 "./atomic/stdatomic-bitint-1.c"
     ~count
# 141 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 141 "./atomic/stdatomic-bitint-1.c"
     memory_order_release
# 141 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 141 "./atomic/stdatomic-bitint-1.c"
                                                                  != 0)
    abort ();

  if (
# 144 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 144 "./atomic/stdatomic-bitint-1.c"
     &v
# 144 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 144 "./atomic/stdatomic-bitint-1.c"
     0
# 144 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 144 "./atomic/stdatomic-bitint-1.c"
     memory_order_acq_rel
# 144 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 144 "./atomic/stdatomic-bitint-1.c"
                                                             != init)
    abort ();

  if (
# 147 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 147 "./atomic/stdatomic-bitint-1.c"
     &v
# 147 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 147 "./atomic/stdatomic-bitint-1.c"
     ~count
# 147 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 147 "./atomic/stdatomic-bitint-1.c"
     memory_order_seq_cst
# 147 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 147 "./atomic/stdatomic-bitint-1.c"
                                                                  != init)
    abort ();

  if (
# 150 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_xor ((
# 150 "./atomic/stdatomic-bitint-1.c"
     &v
# 150 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 150 "./atomic/stdatomic-bitint-1.c"
     ~count
# 150 "./atomic/stdatomic-bitint-1.c" 3 4
     ), 5) 
# 150 "./atomic/stdatomic-bitint-1.c"
                                   != 0)
    abort ();
}

void
test_fetch_or ()
{
  v = 0;
  count = 17592186044416wb;

  if (
# 160 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 160 "./atomic/stdatomic-bitint-1.c"
     &v
# 160 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 160 "./atomic/stdatomic-bitint-1.c"
     count
# 160 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 160 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 160 "./atomic/stdatomic-bitint-1.c" 3 4
     )) 
# 160 "./atomic/stdatomic-bitint-1.c"
                                                                != 0)
    abort ();

  count *= 2;
  if (
# 164 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 164 "./atomic/stdatomic-bitint-1.c"
     &v
# 164 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 164 "./atomic/stdatomic-bitint-1.c"
     35184372088832wb
# 164 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 164 "./atomic/stdatomic-bitint-1.c"
     memory_order_consume
# 164 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 165 "./atomic/stdatomic-bitint-1.c"
     != 17592186044416wb)
    abort ();

  count *= 2;
  if (
# 169 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 169 "./atomic/stdatomic-bitint-1.c"
     &v
# 169 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 169 "./atomic/stdatomic-bitint-1.c"
     count
# 169 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 169 "./atomic/stdatomic-bitint-1.c"
     memory_order_acquire
# 169 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 170 "./atomic/stdatomic-bitint-1.c"
     != 52776558133248wb)
    abort ();

  count *= 2;
  if (
# 174 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 174 "./atomic/stdatomic-bitint-1.c"
     &v
# 174 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 174 "./atomic/stdatomic-bitint-1.c"
     140737488355328wb
# 174 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 174 "./atomic/stdatomic-bitint-1.c"
     memory_order_release
# 174 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 175 "./atomic/stdatomic-bitint-1.c"
     != 123145302310912wb)
    abort ();

  count *= 2;
  if (
# 179 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 179 "./atomic/stdatomic-bitint-1.c"
     &v
# 179 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 179 "./atomic/stdatomic-bitint-1.c"
     count
# 179 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 179 "./atomic/stdatomic-bitint-1.c"
     memory_order_acq_rel
# 179 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 180 "./atomic/stdatomic-bitint-1.c"
     != 263882790666240wb)
    abort ();

  count *= 2;
  if (
# 184 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 184 "./atomic/stdatomic-bitint-1.c"
     &v
# 184 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 184 "./atomic/stdatomic-bitint-1.c"
     count
# 184 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 184 "./atomic/stdatomic-bitint-1.c"
     memory_order_seq_cst
# 184 "./atomic/stdatomic-bitint-1.c" 3 4
     ))
      
# 185 "./atomic/stdatomic-bitint-1.c"
     != 545357767376896wb)
    abort ();

  count *= 2;
  if (
# 189 "./atomic/stdatomic-bitint-1.c" 3 4
     __atomic_fetch_or ((
# 189 "./atomic/stdatomic-bitint-1.c"
     &v
# 189 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 189 "./atomic/stdatomic-bitint-1.c"
     count
# 189 "./atomic/stdatomic-bitint-1.c" 3 4
     ), 5) 
# 189 "./atomic/stdatomic-bitint-1.c"
                                 != 1108307720798208wb)
    abort ();
}




void
test_add ()
{
  v = 0;
  count = 4722366482869645213696wb;

  
# 202 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_add ((
# 202 "./atomic/stdatomic-bitint-1.c"
 &v
# 202 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 202 "./atomic/stdatomic-bitint-1.c"
 count
# 202 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 202 "./atomic/stdatomic-bitint-1.c"
                             ;
  if (v != 4722366482869645213696wb)
    abort ();

  
# 206 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_add ((
# 206 "./atomic/stdatomic-bitint-1.c"
 &v
# 206 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 206 "./atomic/stdatomic-bitint-1.c"
 count
# 206 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 206 "./atomic/stdatomic-bitint-1.c"
 memory_order_consume
# 206 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 206 "./atomic/stdatomic-bitint-1.c"
                                                            ;
  if (v != 9444732965739290427392wb)
    abort ();

  
# 210 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_add ((
# 210 "./atomic/stdatomic-bitint-1.c"
 &v
# 210 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 210 "./atomic/stdatomic-bitint-1.c"
 4722366482869645213696wb
# 210 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 210 "./atomic/stdatomic-bitint-1.c"
                                                ;
  if (v != 14167099448608935641088wb)
    abort ();

  
# 214 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_add ((
# 214 "./atomic/stdatomic-bitint-1.c"
 &v
# 214 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 214 "./atomic/stdatomic-bitint-1.c"
 4722366482869645213696wb
# 214 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 214 "./atomic/stdatomic-bitint-1.c"
 memory_order_release
# 214 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
                             
# 215 "./atomic/stdatomic-bitint-1.c"
                            ;
  if (v != 18889465931478580854784wb)
    abort ();

  
# 219 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_add ((
# 219 "./atomic/stdatomic-bitint-1.c"
 &v
# 219 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 219 "./atomic/stdatomic-bitint-1.c"
 4722366482869645213696wb
# 219 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 219 "./atomic/stdatomic-bitint-1.c"
                                                ;
  if (v != 23611832414348226068480wb)
    abort ();

  
# 223 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_add ((
# 223 "./atomic/stdatomic-bitint-1.c"
 &v
# 223 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 223 "./atomic/stdatomic-bitint-1.c"
 count
# 223 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 223 "./atomic/stdatomic-bitint-1.c"
 memory_order_seq_cst
# 223 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 223 "./atomic/stdatomic-bitint-1.c"
                                                            ;
  if (v != 28334198897217871282176wb)
    abort ();
}

void
test_sub ()
{
  v = res = -3638804536836293398783417724445294828wb;
  count = 0;

  
# 234 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_sub ((
# 234 "./atomic/stdatomic-bitint-1.c"
 &v
# 234 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 234 "./atomic/stdatomic-bitint-1.c"
 count + 1
# 234 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 234 "./atomic/stdatomic-bitint-1.c"
                                 ;
  if (v != --res)
    abort ();

  
# 238 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_sub ((
# 238 "./atomic/stdatomic-bitint-1.c"
 &v
# 238 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 238 "./atomic/stdatomic-bitint-1.c"
 count + 1
# 238 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 238 "./atomic/stdatomic-bitint-1.c"
 memory_order_consume
# 238 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 238 "./atomic/stdatomic-bitint-1.c"
                                                                ;
  if (v != --res)
    abort ();

  
# 242 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_sub ((
# 242 "./atomic/stdatomic-bitint-1.c"
 &v
# 242 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 242 "./atomic/stdatomic-bitint-1.c"
 1
# 242 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 242 "./atomic/stdatomic-bitint-1.c"
                         ;
  if (v != --res)
    abort ();

  
# 246 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_sub ((
# 246 "./atomic/stdatomic-bitint-1.c"
 &v
# 246 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 246 "./atomic/stdatomic-bitint-1.c"
 1
# 246 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 246 "./atomic/stdatomic-bitint-1.c"
 memory_order_release
# 246 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 246 "./atomic/stdatomic-bitint-1.c"
                                                        ;
  if (v != --res)
    abort ();

  
# 250 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_sub ((
# 250 "./atomic/stdatomic-bitint-1.c"
 &v
# 250 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 250 "./atomic/stdatomic-bitint-1.c"
 count + 1
# 250 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 250 "./atomic/stdatomic-bitint-1.c"
                                 ;
  if (v != --res)
    abort ();

  
# 254 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_sub ((
# 254 "./atomic/stdatomic-bitint-1.c"
 &v
# 254 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 254 "./atomic/stdatomic-bitint-1.c"
 count + 1
# 254 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 254 "./atomic/stdatomic-bitint-1.c"
 memory_order_seq_cst
# 254 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 254 "./atomic/stdatomic-bitint-1.c"
                                                                ;
  if (v != --res)
    abort ();
}

void
test_and ()
{
  v = init;

  
# 264 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_and ((
# 264 "./atomic/stdatomic-bitint-1.c"
 &v
# 264 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 264 "./atomic/stdatomic-bitint-1.c"
 0
# 264 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 264 "./atomic/stdatomic-bitint-1.c"
                         ;
  if (v != 0)
    abort ();

  v = init;
  
# 269 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_and ((
# 269 "./atomic/stdatomic-bitint-1.c"
 &v
# 269 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 269 "./atomic/stdatomic-bitint-1.c"
 init
# 269 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 269 "./atomic/stdatomic-bitint-1.c"
 memory_order_consume
# 269 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 269 "./atomic/stdatomic-bitint-1.c"
                                                           ;
  if (v != init)
    abort ();

  
# 273 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_and ((
# 273 "./atomic/stdatomic-bitint-1.c"
 &v
# 273 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 273 "./atomic/stdatomic-bitint-1.c"
 0
# 273 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 273 "./atomic/stdatomic-bitint-1.c"
                         ;
  if (v != 0)
    abort ();

  v = ~v;
  
# 278 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_and ((
# 278 "./atomic/stdatomic-bitint-1.c"
 &v
# 278 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 278 "./atomic/stdatomic-bitint-1.c"
 init
# 278 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 278 "./atomic/stdatomic-bitint-1.c"
 memory_order_release
# 278 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 278 "./atomic/stdatomic-bitint-1.c"
                                                           ;
  if (v != init)
    abort ();

  
# 282 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_and ((
# 282 "./atomic/stdatomic-bitint-1.c"
 &v
# 282 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 282 "./atomic/stdatomic-bitint-1.c"
 0
# 282 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 282 "./atomic/stdatomic-bitint-1.c"
                         ;
  if (v != 0)
    abort ();

  v = ~v;
  
# 287 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_and ((
# 287 "./atomic/stdatomic-bitint-1.c"
 &v
# 287 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 287 "./atomic/stdatomic-bitint-1.c"
 0
# 287 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 287 "./atomic/stdatomic-bitint-1.c"
 memory_order_seq_cst
# 287 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 287 "./atomic/stdatomic-bitint-1.c"
                                                        ;
  if (v != 0)
    abort ();
}

void
test_xor ()
{
  v = init;
  count = 0;

  
# 298 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_xor ((
# 298 "./atomic/stdatomic-bitint-1.c"
 &v
# 298 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 298 "./atomic/stdatomic-bitint-1.c"
 count
# 298 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 298 "./atomic/stdatomic-bitint-1.c"
                             ;
  if (v != init)
    abort ();

  
# 302 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_xor ((
# 302 "./atomic/stdatomic-bitint-1.c"
 &v
# 302 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 302 "./atomic/stdatomic-bitint-1.c"
 ~count
# 302 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 302 "./atomic/stdatomic-bitint-1.c"
 memory_order_consume
# 302 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 302 "./atomic/stdatomic-bitint-1.c"
                                                             ;
  if (v != 0)
    abort ();

  
# 306 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_xor ((
# 306 "./atomic/stdatomic-bitint-1.c"
 &v
# 306 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 306 "./atomic/stdatomic-bitint-1.c"
 0
# 306 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 306 "./atomic/stdatomic-bitint-1.c"
                         ;
  if (v != 0)
    abort ();

  
# 310 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_xor ((
# 310 "./atomic/stdatomic-bitint-1.c"
 &v
# 310 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 310 "./atomic/stdatomic-bitint-1.c"
 ~count
# 310 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 310 "./atomic/stdatomic-bitint-1.c"
 memory_order_release
# 310 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 310 "./atomic/stdatomic-bitint-1.c"
                                                             ;
  if (v != init)
    abort ();

  
# 314 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_xor ((
# 314 "./atomic/stdatomic-bitint-1.c"
 &v
# 314 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 314 "./atomic/stdatomic-bitint-1.c"
 0
# 314 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 314 "./atomic/stdatomic-bitint-1.c"
 memory_order_acq_rel
# 314 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 314 "./atomic/stdatomic-bitint-1.c"
                                                        ;
  if (v != init)
    abort ();

  
# 318 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_xor ((
# 318 "./atomic/stdatomic-bitint-1.c"
 &v
# 318 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 318 "./atomic/stdatomic-bitint-1.c"
 ~count
# 318 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 318 "./atomic/stdatomic-bitint-1.c"
                              ;
  if (v != 0)
    abort ();
}

void
test_or ()
{
  v = 0;
  count = 19342813113834066795298816wb;

  
# 329 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_or ((
# 329 "./atomic/stdatomic-bitint-1.c"
 &v
# 329 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 329 "./atomic/stdatomic-bitint-1.c"
 count
# 329 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 329 "./atomic/stdatomic-bitint-1.c"
                            ;
  if (v != 19342813113834066795298816wb)
    abort ();

  count *= 2;
  
# 334 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_or ((
# 334 "./atomic/stdatomic-bitint-1.c"
 &v
# 334 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 334 "./atomic/stdatomic-bitint-1.c"
 count
# 334 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 334 "./atomic/stdatomic-bitint-1.c"
 memory_order_consume
# 334 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 334 "./atomic/stdatomic-bitint-1.c"
                                                           ;
  if (v != 58028439341502200385896448wb)
    abort ();

  count *= 2;
  
# 339 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_or ((
# 339 "./atomic/stdatomic-bitint-1.c"
 &v
# 339 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 339 "./atomic/stdatomic-bitint-1.c"
 77371252455336267181195264wb
# 339 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 339 "./atomic/stdatomic-bitint-1.c"
                                                   ;
  if (v != 135399691796838467567091712wb)
    abort ();

  count *= 2;
  
# 344 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_or ((
# 344 "./atomic/stdatomic-bitint-1.c"
 &v
# 344 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 344 "./atomic/stdatomic-bitint-1.c"
 154742504910672534362390528wb
# 344 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 344 "./atomic/stdatomic-bitint-1.c"
 memory_order_release
# 344 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
                            
# 345 "./atomic/stdatomic-bitint-1.c"
                           ;
  if (v != 290142196707511001929482240wb)
    abort ();

  count *= 2;
  
# 350 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_or ((
# 350 "./atomic/stdatomic-bitint-1.c"
 &v
# 350 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 350 "./atomic/stdatomic-bitint-1.c"
 count
# 350 "./atomic/stdatomic-bitint-1.c" 3 4
 ), 5)
# 350 "./atomic/stdatomic-bitint-1.c"
                            ;
  if (v != 599627206528856070654263296wb)
    abort ();

  count *= 2;
  
# 355 "./atomic/stdatomic-bitint-1.c" 3 4
 __atomic_fetch_or ((
# 355 "./atomic/stdatomic-bitint-1.c"
 &v
# 355 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 355 "./atomic/stdatomic-bitint-1.c"
 count
# 355 "./atomic/stdatomic-bitint-1.c" 3 4
 ), (
# 355 "./atomic/stdatomic-bitint-1.c"
 memory_order_seq_cst
# 355 "./atomic/stdatomic-bitint-1.c" 3 4
 ))
# 355 "./atomic/stdatomic-bitint-1.c"
                                                           ;
  if (v != 1218597226171546208103825408wb)
    abort ();
}

void
test_exchange (void)
{
  
# 363 "./atomic/stdatomic-bitint-1.c" 3 4
 __extension__ ({ __auto_type __atomic_store_ptr = (
# 363 "./atomic/stdatomic-bitint-1.c"
 &v
# 363 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __typeof__ ((void)0, *__atomic_store_ptr) __atomic_store_tmp = (
# 363 "./atomic/stdatomic-bitint-1.c"
 15794812138349191682564933935017390008wb
# 363 "./atomic/stdatomic-bitint-1.c" 3 4
 ); __atomic_store (__atomic_store_ptr, &__atomic_store_tmp, (5)); })
# 363 "./atomic/stdatomic-bitint-1.c"
                                                            ;
  if (
# 364 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 364 "./atomic/stdatomic-bitint-1.c"
     &v
# 364 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 364 "./atomic/stdatomic-bitint-1.c"
     -2166613183393424891717146518563613668wb
# 364 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (5)); __atomic_exchange_tmp; })
      
# 365 "./atomic/stdatomic-bitint-1.c"
     != 15794812138349191682564933935017390008wb
      || 
# 366 "./atomic/stdatomic-bitint-1.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 366 "./atomic/stdatomic-bitint-1.c"
        &v
# 366 "./atomic/stdatomic-bitint-1.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 366 "./atomic/stdatomic-bitint-1.c"
                         != -2166613183393424891717146518563613668wb)
    abort ();
  if (
# 368 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_exchange_ptr = (
# 368 "./atomic/stdatomic-bitint-1.c"
     &v
# 368 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_val = (
# 368 "./atomic/stdatomic-bitint-1.c"
     61251098386268815852902382804483910638wb
# 368 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_exchange_ptr) __atomic_exchange_tmp; __atomic_exchange (__atomic_exchange_ptr, &__atomic_exchange_val, &__atomic_exchange_tmp, (
# 368 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 368 "./atomic/stdatomic-bitint-1.c" 3 4
     )); __atomic_exchange_tmp; })

      
# 370 "./atomic/stdatomic-bitint-1.c"
     != -2166613183393424891717146518563613668wb
      || (
# 371 "./atomic/stdatomic-bitint-1.c" 3 4
         __extension__ ({ __auto_type __atomic_load_ptr = (
# 371 "./atomic/stdatomic-bitint-1.c"
         &v
# 371 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 371 "./atomic/stdatomic-bitint-1.c"
         memory_order_acquire
# 371 "./atomic/stdatomic-bitint-1.c" 3 4
         )); __atomic_load_tmp; })
   
# 372 "./atomic/stdatomic-bitint-1.c"
  != 61251098386268815852902382804483910638wb))
    abort ();

  count = -2166613183393424891717146518563613668wb;
  if (
# 376 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 376 "./atomic/stdatomic-bitint-1.c"
     &v
# 376 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 376 "./atomic/stdatomic-bitint-1.c"
     -36677332297536901313774263310237646448wb
# 376 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 376 "./atomic/stdatomic-bitint-1.c"
     &count
# 376 "./atomic/stdatomic-bitint-1.c" 3 4
     ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
                                                    
# 377 "./atomic/stdatomic-bitint-1.c"
                                                   )
    abort ();
  if (count != 61251098386268815852902382804483910638wb
      || 
# 380 "./atomic/stdatomic-bitint-1.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 380 "./atomic/stdatomic-bitint-1.c"
        &v
# 380 "./atomic/stdatomic-bitint-1.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 380 "./atomic/stdatomic-bitint-1.c"
                         != 61251098386268815852902382804483910638wb)
    abort ();
  if (!
# 382 "./atomic/stdatomic-bitint-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 382 "./atomic/stdatomic-bitint-1.c"
      &v
# 382 "./atomic/stdatomic-bitint-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 382 "./atomic/stdatomic-bitint-1.c"
      -36677332297536901313774263310237646448wb
# 382 "./atomic/stdatomic-bitint-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 382 "./atomic/stdatomic-bitint-1.c"
      &count
# 382 "./atomic/stdatomic-bitint-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (5), (5)); })
                                                     
# 383 "./atomic/stdatomic-bitint-1.c"
                                                    )
    abort ();
  if (count != 61251098386268815852902382804483910638wb
      || 
# 386 "./atomic/stdatomic-bitint-1.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 386 "./atomic/stdatomic-bitint-1.c"
        &v
# 386 "./atomic/stdatomic-bitint-1.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 386 "./atomic/stdatomic-bitint-1.c"
                         != -36677332297536901313774263310237646448wb)
    abort ();

  count = -2166613183393424891717146518563613668wb;
  if (
# 390 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 390 "./atomic/stdatomic-bitint-1.c"
     &v
# 390 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 390 "./atomic/stdatomic-bitint-1.c"
     73949932022761409003352953944661689416wb
# 390 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 390 "./atomic/stdatomic-bitint-1.c"
     &count
# 390 "./atomic/stdatomic-bitint-1.c" 3 4
     ), &__atomic_compare_exchange_tmp, 0, (
# 390 "./atomic/stdatomic-bitint-1.c"
     memory_order_seq_cst
# 390 "./atomic/stdatomic-bitint-1.c" 3 4
     ), (
# 390 "./atomic/stdatomic-bitint-1.c"
     memory_order_relaxed
# 390 "./atomic/stdatomic-bitint-1.c" 3 4
     )); })


                                 
# 393 "./atomic/stdatomic-bitint-1.c"
                                )
    abort ();
  if (count != -36677332297536901313774263310237646448wb
      || 
# 396 "./atomic/stdatomic-bitint-1.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 396 "./atomic/stdatomic-bitint-1.c"
        &v
# 396 "./atomic/stdatomic-bitint-1.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 396 "./atomic/stdatomic-bitint-1.c"
                         != -36677332297536901313774263310237646448wb)
    abort ();
  if (!
# 398 "./atomic/stdatomic-bitint-1.c" 3 4
      __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 398 "./atomic/stdatomic-bitint-1.c"
      &v
# 398 "./atomic/stdatomic-bitint-1.c" 3 4
      ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 398 "./atomic/stdatomic-bitint-1.c"
      73949932022761409003352953944661689416wb
# 398 "./atomic/stdatomic-bitint-1.c" 3 4
      ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 398 "./atomic/stdatomic-bitint-1.c"
      &count
# 398 "./atomic/stdatomic-bitint-1.c" 3 4
      ), &__atomic_compare_exchange_tmp, 0, (
# 398 "./atomic/stdatomic-bitint-1.c"
      memory_order_seq_cst
# 398 "./atomic/stdatomic-bitint-1.c" 3 4
      ), (
# 398 "./atomic/stdatomic-bitint-1.c"
      memory_order_seq_cst
# 398 "./atomic/stdatomic-bitint-1.c" 3 4
      )); })


                           
# 401 "./atomic/stdatomic-bitint-1.c"
                          )
    abort ();
  if (count != -36677332297536901313774263310237646448wb
      || 
# 404 "./atomic/stdatomic-bitint-1.c" 3 4
        __extension__ ({ __auto_type __atomic_load_ptr = (
# 404 "./atomic/stdatomic-bitint-1.c"
        &v
# 404 "./atomic/stdatomic-bitint-1.c" 3 4
        ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 404 "./atomic/stdatomic-bitint-1.c"
                         != 73949932022761409003352953944661689416wb)
    abort ();

  count = 
# 407 "./atomic/stdatomic-bitint-1.c" 3 4
         __extension__ ({ __auto_type __atomic_load_ptr = (
# 407 "./atomic/stdatomic-bitint-1.c"
         &v
# 407 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; })
# 407 "./atomic/stdatomic-bitint-1.c"
                         ;
  do
    res = count + -82256758205518164043596305502815392646wb;
  while (!
# 410 "./atomic/stdatomic-bitint-1.c" 3 4
         __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 410 "./atomic/stdatomic-bitint-1.c"
         &v
# 410 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 410 "./atomic/stdatomic-bitint-1.c"
         res
# 410 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 410 "./atomic/stdatomic-bitint-1.c"
         &count
# 410 "./atomic/stdatomic-bitint-1.c" 3 4
         ), &__atomic_compare_exchange_tmp, 1, (5), (5)); })
# 410 "./atomic/stdatomic-bitint-1.c"
                                                       );
  if (
# 411 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 411 "./atomic/stdatomic-bitint-1.c"
     &v
# 411 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 411 "./atomic/stdatomic-bitint-1.c"
                      != -8306826182756755040243351558153703230wb)
    abort ();

  count = 
# 414 "./atomic/stdatomic-bitint-1.c" 3 4
         __extension__ ({ __auto_type __atomic_load_ptr = (
# 414 "./atomic/stdatomic-bitint-1.c"
         &v
# 414 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (
# 414 "./atomic/stdatomic-bitint-1.c"
         memory_order_acquire
# 414 "./atomic/stdatomic-bitint-1.c" 3 4
         )); __atomic_load_tmp; })
# 414 "./atomic/stdatomic-bitint-1.c"
                                                        ;
  do
    res = count + 48855144829609538366772317026461909818wb;
  while (!
# 417 "./atomic/stdatomic-bitint-1.c" 3 4
         __extension__ ({ __auto_type __atomic_compare_exchange_ptr = (
# 417 "./atomic/stdatomic-bitint-1.c"
         &v
# 417 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __typeof__ ((void)0, *__atomic_compare_exchange_ptr) __atomic_compare_exchange_tmp = (
# 417 "./atomic/stdatomic-bitint-1.c"
         res
# 417 "./atomic/stdatomic-bitint-1.c" 3 4
         ); __atomic_compare_exchange (__atomic_compare_exchange_ptr, (
# 417 "./atomic/stdatomic-bitint-1.c"
         &count
# 417 "./atomic/stdatomic-bitint-1.c" 3 4
         ), &__atomic_compare_exchange_tmp, 1, (
# 417 "./atomic/stdatomic-bitint-1.c"
         memory_order_relaxed
# 417 "./atomic/stdatomic-bitint-1.c" 3 4
         ), (
# 417 "./atomic/stdatomic-bitint-1.c"
         memory_order_relaxed
# 417 "./atomic/stdatomic-bitint-1.c" 3 4
         )); })

                            
# 419 "./atomic/stdatomic-bitint-1.c"
                           );
  if (
# 420 "./atomic/stdatomic-bitint-1.c" 3 4
     __extension__ ({ __auto_type __atomic_load_ptr = (
# 420 "./atomic/stdatomic-bitint-1.c"
     &v
# 420 "./atomic/stdatomic-bitint-1.c" 3 4
     ); __typeof__ ((void)0, *__atomic_load_ptr) __atomic_load_tmp; __atomic_load (__atomic_load_ptr, &__atomic_load_tmp, (5)); __atomic_load_tmp; }) 
# 420 "./atomic/stdatomic-bitint-1.c"
                      != 40548318646852783326528965468308206588wb)
    abort ();
}


int
main ()
{

  test_fetch_add ();
  test_fetch_sub ();
  test_fetch_and ();
  test_fetch_xor ();
  test_fetch_or ();
  test_add ();
  test_sub ();
  test_and ();
  test_xor ();
  test_or ();
  test_exchange ();

  return 0;
}
