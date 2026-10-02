/* Portable C entry points for libc++ external threading. */
#ifndef __EDG_DOT_THREAD_ABI_H
#define __EDG_DOT_THREAD_ABI_H
#include <time.h>
#ifdef __cplusplus
extern "C" {
#endif
int __edg_libcpp_mutex_lock(unsigned long*, int);
int __edg_libcpp_mutex_trylock(unsigned long*, int);
int __edg_libcpp_mutex_unlock(unsigned long*, int);
int __edg_libcpp_mutex_destroy(unsigned long*, int);
int __edg_libcpp_recursive_mutex_init(unsigned long*);
int __edg_libcpp_condvar_signal(unsigned long*, int);
int __edg_libcpp_condvar_wait(unsigned long*, unsigned long*, const struct timespec*);
int __edg_libcpp_condvar_destroy(unsigned long*);
int __edg_libcpp_execute_once(unsigned long*, void (*)(void));
int __edg_libcpp_thread_create(unsigned long*, void* (*)(void*), void*);
unsigned long __edg_libcpp_thread_current_id(void);
unsigned long __edg_libcpp_thread_get_id(const unsigned long*);
int __edg_libcpp_thread_join(unsigned long*);
int __edg_libcpp_thread_detach(unsigned long*);
void __edg_libcpp_thread_yield(void);
void __edg_libcpp_thread_sleep_for(long);
int __edg_libcpp_tls_create(unsigned long*, void (*)(void*));
void* __edg_libcpp_tls_get(unsigned long);
int __edg_libcpp_tls_set(unsigned long, void*);
#ifdef __cplusplus
}
#endif
#endif
