/* C11 atomic facade for the EDG portable profile. GNU builtins retain exact
   object width and dispatch to AtomicOps. C++ uses its own <atomic> provider. */
#ifndef __EDG_DOT_LIBC_STDATOMIC_H
#define __EDG_DOT_LIBC_STDATOMIC_H
#ifndef __cplusplus
#include <stdint.h>
#include <stddef.h>
#include <uchar.h>
typedef enum { memory_order_relaxed=0,memory_order_consume=1,memory_order_acquire=2,
 memory_order_release=3,memory_order_acq_rel=4,memory_order_seq_cst=5 } memory_order;
typedef _Atomic(_Bool) atomic_bool;
typedef _Atomic(char) atomic_char;
typedef _Atomic(signed char) atomic_schar;
typedef _Atomic(unsigned char) atomic_uchar;
typedef _Atomic(short) atomic_short;
typedef _Atomic(unsigned short) atomic_ushort;
typedef _Atomic(int) atomic_int;
typedef _Atomic(unsigned) atomic_uint;
typedef _Atomic(long) atomic_long;
typedef _Atomic(unsigned long) atomic_ulong;
typedef _Atomic(long long) atomic_llong;
typedef _Atomic(unsigned long long) atomic_ullong;
typedef _Atomic(char16_t) atomic_char16_t;
typedef _Atomic(char32_t) atomic_char32_t;
typedef _Atomic(wchar_t) atomic_wchar_t;
typedef _Atomic(int_least8_t) atomic_int_least8_t;
typedef _Atomic(uint_least8_t) atomic_uint_least8_t;
typedef _Atomic(int_least16_t) atomic_int_least16_t;
typedef _Atomic(uint_least16_t) atomic_uint_least16_t;
typedef _Atomic(int_least32_t) atomic_int_least32_t;
typedef _Atomic(uint_least32_t) atomic_uint_least32_t;
typedef _Atomic(int_least64_t) atomic_int_least64_t;
typedef _Atomic(uint_least64_t) atomic_uint_least64_t;
typedef _Atomic(int_fast8_t) atomic_int_fast8_t;
typedef _Atomic(uint_fast8_t) atomic_uint_fast8_t;
typedef _Atomic(int_fast16_t) atomic_int_fast16_t;
typedef _Atomic(uint_fast16_t) atomic_uint_fast16_t;
typedef _Atomic(int_fast32_t) atomic_int_fast32_t;
typedef _Atomic(uint_fast32_t) atomic_uint_fast32_t;
typedef _Atomic(int_fast64_t) atomic_int_fast64_t;
typedef _Atomic(uint_fast64_t) atomic_uint_fast64_t;
typedef _Atomic(intptr_t) atomic_intptr_t;
typedef _Atomic(uintptr_t) atomic_uintptr_t;
typedef _Atomic(size_t) atomic_size_t;
typedef _Atomic(ptrdiff_t) atomic_ptrdiff_t;
typedef _Atomic(intmax_t) atomic_intmax_t;
typedef _Atomic(uintmax_t) atomic_uintmax_t;
#define ATOMIC_BOOL_LOCK_FREE 1
#define ATOMIC_CHAR_LOCK_FREE 1
#define ATOMIC_CHAR16_T_LOCK_FREE 1
#define ATOMIC_CHAR32_T_LOCK_FREE 1
#define ATOMIC_WCHAR_T_LOCK_FREE 1
#define ATOMIC_SHORT_LOCK_FREE 1
#define ATOMIC_INT_LOCK_FREE 1
#define ATOMIC_LONG_LOCK_FREE 1
#define ATOMIC_LLONG_LOCK_FREE 1
#define ATOMIC_POINTER_LOCK_FREE 1
#define ATOMIC_VAR_INIT(value) (value)
#define atomic_init(object,value) atomic_store_explicit(object,value,memory_order_relaxed)
#define kill_dependency(value) (value)
#define atomic_is_lock_free(object) __atomic_is_lock_free(sizeof(*(object)),(object))
static __inline void atomic_thread_fence(memory_order order){__atomic_thread_fence(order);}
static __inline void atomic_signal_fence(memory_order order){__atomic_signal_fence(order);}
/* Comma-expression rvalue conversion strips _Atomic and cv from the scratch
   value's type, without reading the object in this unevaluated operand. */
#define __EDG_ATOMIC_VALUE_TYPE(pointer) __typeof__((void)0,*(pointer))
#define atomic_load_explicit(object,order) __extension__ ({ \
 __auto_type __edg_atomic_p=(object);__EDG_ATOMIC_VALUE_TYPE(__edg_atomic_p) __edg_atomic_v; \
 __atomic_load(__edg_atomic_p,&__edg_atomic_v,(order));__edg_atomic_v; })
#define atomic_store_explicit(object,value,order) __extension__ ({ \
 __auto_type __edg_atomic_p=(object);__EDG_ATOMIC_VALUE_TYPE(__edg_atomic_p) __edg_atomic_v=(value); \
 __atomic_store(__edg_atomic_p,&__edg_atomic_v,(order)); })
#define atomic_exchange_explicit(object,value,order) __extension__ ({ \
 __auto_type __edg_atomic_p=(object);__EDG_ATOMIC_VALUE_TYPE(__edg_atomic_p) __edg_atomic_v=(value),__edg_atomic_old; \
 __atomic_exchange(__edg_atomic_p,&__edg_atomic_v,&__edg_atomic_old,(order));__edg_atomic_old; })
#define __EDG_ATOMIC_CAS(object,expected,desired,weak,success,failure) __extension__ ({ \
 __auto_type __edg_atomic_p=(object);__EDG_ATOMIC_VALUE_TYPE(__edg_atomic_p) __edg_atomic_v=(desired); \
 __atomic_compare_exchange(__edg_atomic_p,(expected),&__edg_atomic_v,(weak),(success),(failure)); })
#define atomic_compare_exchange_strong_explicit(object,expected,desired,success,failure) __EDG_ATOMIC_CAS(object,expected,desired,0,success,failure)
#define atomic_compare_exchange_weak_explicit(object,expected,desired,success,failure) __EDG_ATOMIC_CAS(object,expected,desired,1,success,failure)
#define atomic_load(object) atomic_load_explicit(object,memory_order_seq_cst)
#define atomic_store(object,value) atomic_store_explicit(object,value,memory_order_seq_cst)
#define atomic_exchange(object,value) atomic_exchange_explicit(object,value,memory_order_seq_cst)
#define atomic_compare_exchange_strong(object,expected,desired) atomic_compare_exchange_strong_explicit(object,expected,desired,memory_order_seq_cst,memory_order_seq_cst)
#define atomic_compare_exchange_weak(object,expected,desired) atomic_compare_exchange_weak_explicit(object,expected,desired,memory_order_seq_cst,memory_order_seq_cst)
/* C11 pointer fetch arithmetic counts elements; GNU builtins count bytes. The
   unchosen branch is char* so the sizeof remains valid for integer objects. */
#define __EDG_ATOMIC_STRIDE(pointer) sizeof(*__builtin_choose_expr(__builtin_classify_type(*(pointer))==5,*(pointer),(char*)0))
#define atomic_fetch_add_explicit(object,value,order) __extension__ ({ \
 __auto_type __edg_atomic_p=(object);__atomic_fetch_add(__edg_atomic_p,(value)*__EDG_ATOMIC_STRIDE(__edg_atomic_p),(order)); })
#define atomic_fetch_sub_explicit(object,value,order) __extension__ ({ \
 __auto_type __edg_atomic_p=(object);__atomic_fetch_sub(__edg_atomic_p,(value)*__EDG_ATOMIC_STRIDE(__edg_atomic_p),(order)); })
#define atomic_fetch_and_explicit(object,value,order) __atomic_fetch_and((object),(value),(order))
#define atomic_fetch_or_explicit(object,value,order) __atomic_fetch_or((object),(value),(order))
#define atomic_fetch_xor_explicit(object,value,order) __atomic_fetch_xor((object),(value),(order))
#define atomic_fetch_add(object,value) atomic_fetch_add_explicit(object,value,memory_order_seq_cst)
#define atomic_fetch_sub(object,value) atomic_fetch_sub_explicit(object,value,memory_order_seq_cst)
#define atomic_fetch_and(object,value) atomic_fetch_and_explicit(object,value,memory_order_seq_cst)
#define atomic_fetch_or(object,value) atomic_fetch_or_explicit(object,value,memory_order_seq_cst)
#define atomic_fetch_xor(object,value) atomic_fetch_xor_explicit(object,value,memory_order_seq_cst)
typedef struct { _Atomic(_Bool) __value; } atomic_flag;
#define ATOMIC_FLAG_INIT {0}
static __inline _Bool atomic_flag_test_and_set_explicit(volatile atomic_flag *object,memory_order order){return __atomic_test_and_set(object,order);}
static __inline _Bool atomic_flag_test_and_set(volatile atomic_flag *object){return atomic_flag_test_and_set_explicit(object,memory_order_seq_cst);}
static __inline void atomic_flag_clear_explicit(volatile atomic_flag *object,memory_order order){__atomic_clear(object,order);}
static __inline void atomic_flag_clear(volatile atomic_flag *object){atomic_flag_clear_explicit(object,memory_order_seq_cst);}
#endif
#endif
