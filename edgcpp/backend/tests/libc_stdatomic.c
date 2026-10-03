#include <stdatomic.h>
#include <stdint.h>
#ifndef __cplusplus
#ifdef __STDC_NO_ATOMICS__
#error C11 atomics must not advertise lack of atomic type support
#endif
static atomic_int counter=ATOMIC_VAR_INIT(0);
static atomic_flag flag=ATOMIC_FLAG_INIT;
typedef struct { long a,b; } Pair;
void stdatomic_reset(void){atomic_store(&counter,0);}
void stdatomic_increment(void){atomic_fetch_add_explicit(&counter,1,memory_order_relaxed);}
int stdatomic_counter(void){return atomic_load(&counter);}
int stdatomic_cases(void){
 atomic_int x=ATOMIC_VAR_INIT(3);atomic_init(&x,7);
 _Static_assert(_Generic((__EDG_ATOMIC_VALUE_TYPE(&x)*)0,int*:1,default:0),"scratch type must be non-atomic");
 if(atomic_load(&x)!=7||atomic_exchange_explicit(&x,11,memory_order_acq_rel)!=7)return __LINE__;
 int expected=7;if(atomic_compare_exchange_strong(&x,&expected,19)||expected!=11)return __LINE__;
 while(!atomic_compare_exchange_weak_explicit(&x,&expected,19,memory_order_acq_rel,memory_order_acquire))if(expected!=11)return __LINE__;
 if(atomic_fetch_add(&x,3)!=19||atomic_fetch_sub_explicit(&x,2,memory_order_relaxed)!=22)return __LINE__;
 if(atomic_fetch_or(&x,3)!=20||atomic_fetch_and(&x,7)!=23||atomic_fetch_xor(&x,5)!=7||atomic_load(&x)!=2)return __LINE__;
 atomic_int array[2]={ATOMIC_VAR_INIT(31),ATOMIC_VAR_INIT(41)};int index=0;
 if(atomic_fetch_add(&array[index++],5)!=31||index!=1||atomic_load(&array[0])!=36)return __LINE__;
 int values[8];_Atomic(int*) pointer=ATOMIC_VAR_INIT(values+2);
 if(atomic_fetch_add_explicit(&pointer,3,memory_order_seq_cst)!=values+2||atomic_load(&pointer)!=values+5)return __LINE__;
 if(atomic_fetch_sub(&pointer,2)!=values+5||atomic_load(&pointer)!=values+3)return __LINE__;
 if(atomic_fetch_add(&pointer,-1)!=values+3||atomic_load(&pointer)!=values+2)return __LINE__;
 if(atomic_fetch_sub(&pointer,-1)!=values+2||atomic_load(&pointer)!=values+3)return __LINE__;
 volatile atomic_int v=ATOMIC_VAR_INIT(5);const volatile atomic_int *cv=&v;
 if(atomic_load_explicit(cv,memory_order_acquire)!=5)return __LINE__;
 Pair first={17,23},second={31,41};_Atomic(Pair) pair=ATOMIC_VAR_INIT(first);
 Pair result=atomic_exchange(&pair,second);if(result.a!=17||result.b!=23)return __LINE__;
 if(atomic_compare_exchange_strong(&pair,&result,first)||result.a!=31||result.b!=41)return __LINE__;
 if(!atomic_compare_exchange_strong(&pair,&result,first))return __LINE__;
 _Atomic(double) real=ATOMIC_VAR_INIT(-0.0);union {double value;unsigned long bits;} bits;
 bits.value=atomic_load(&real);if(bits.bits!=0x8000000000000000UL)return __LINE__;
 double replacement=3.5;if(atomic_compare_exchange_strong(&real,&replacement,4.5)||replacement!=0.0)return __LINE__;
 if(!atomic_compare_exchange_strong(&real,&replacement,4.5)||atomic_load(&real)!=4.5)return __LINE__;
 atomic_flag_clear(&flag);_Bool(*set)(volatile atomic_flag*)=atomic_flag_test_and_set;
 if(set(&flag)||!atomic_flag_test_and_set_explicit(&flag,memory_order_acquire))return __LINE__;
 atomic_flag_clear_explicit(&flag,memory_order_release);if(set(&flag))return __LINE__;
 void(*fence)(memory_order)=atomic_thread_fence;fence(memory_order_seq_cst);atomic_signal_fence(memory_order_seq_cst);
 if(kill_dependency(17)!=17||ATOMIC_INT_LOCK_FREE!=1)return __LINE__;
 return 0;
}
#ifdef STDATOMIC_ORACLE_MAIN
int main(void){return stdatomic_cases();}
#endif
#else
/* C++17 compatibility include must not redefine language or std::atomic names. */
int stdatomic_include_cpp(void){return 0;}
#endif
