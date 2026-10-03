#include <threads.h>
#include <stdlib.h>
#ifdef __cplusplus
extern "C" {
#endif
int __edg_c11_thread_create(unsigned long *,int (*)(void *),void *);
void __edg_c11_thread_exit(int);
unsigned long __edg_c11_thread_current(void);
int __edg_c11_thread_join(unsigned long,int *);
int __edg_c11_thread_detach(unsigned long);
int __edg_c11_thread_sleep(const struct timespec *,struct timespec *);
void __edg_c11_thread_yield(void);
void __edg_c11_call_once(unsigned long *,void (*)(void));
int __edg_c11_mutex_init(unsigned long *,int);
void __edg_c11_mutex_destroy(unsigned long *);
int __edg_c11_mutex_lock(unsigned long *);
int __edg_c11_mutex_trylock(unsigned long *);
int __edg_c11_mutex_unlock(unsigned long *);
int __edg_c11_mutex_timedlock(unsigned long *,const struct timespec *);
int __edg_c11_condition_init(unsigned long *);
void __edg_c11_condition_destroy(unsigned long *);
int __edg_c11_condition_signal(unsigned long *,int);
int __edg_c11_condition_wait(unsigned long *,unsigned long *,const struct timespec *);
int __edg_c11_tss_create(unsigned long *,void (*)(void *));
void __edg_c11_tss_delete(unsigned long);
void *__edg_c11_tss_get(unsigned long);
int __edg_c11_tss_set(unsigned long,void *);
#ifdef __cplusplus
}
#endif
int thrd_create(thrd_t*t,thrd_start_t f,void*a){return __edg_c11_thread_create(t,f,a);}
void thrd_exit(int result){__edg_c11_thread_exit(result);abort();}
thrd_t thrd_current(void){return __edg_c11_thread_current();}
int thrd_equal(thrd_t a,thrd_t b){return a==b;}
int thrd_join(thrd_t t,int*r){return __edg_c11_thread_join(t,r);}
int thrd_detach(thrd_t t){return __edg_c11_thread_detach(t);}
int thrd_sleep(const struct timespec*t,struct timespec*r){return __edg_c11_thread_sleep(t,r);}
void thrd_yield(void){__edg_c11_thread_yield();}
void call_once(once_flag*f,void(*body)(void)){__edg_c11_call_once(f,body);}
int mtx_init(mtx_t*m,int t){return __edg_c11_mutex_init(&m->__token,t);}
void mtx_destroy(mtx_t*m){__edg_c11_mutex_destroy(&m->__token);}
int mtx_lock(mtx_t*m){return __edg_c11_mutex_lock(&m->__token);}
int mtx_trylock(mtx_t*m){return __edg_c11_mutex_trylock(&m->__token);}
int mtx_unlock(mtx_t*m){return __edg_c11_mutex_unlock(&m->__token);}
int mtx_timedlock(mtx_t*m,const struct timespec*t){return __edg_c11_mutex_timedlock(&m->__token,t);}
int cnd_init(cnd_t*c){return __edg_c11_condition_init(&c->__token);}
void cnd_destroy(cnd_t*c){__edg_c11_condition_destroy(&c->__token);}
int cnd_signal(cnd_t*c){return __edg_c11_condition_signal(&c->__token,0);}
int cnd_broadcast(cnd_t*c){return __edg_c11_condition_signal(&c->__token,1);}
int cnd_wait(cnd_t*c,mtx_t*m){return __edg_c11_condition_wait(&c->__token,&m->__token,0);}
int cnd_timedwait(cnd_t*c,mtx_t*m,const struct timespec*t){return __edg_c11_condition_wait(&c->__token,&m->__token,t);}
int tss_create(tss_t*k,tss_dtor_t d){return __edg_c11_tss_create(k,d);}
void tss_delete(tss_t k){__edg_c11_tss_delete(k);}
void*tss_get(tss_t k){return __edg_c11_tss_get(k);}
int tss_set(tss_t k,void*v){return __edg_c11_tss_set(k,v);}
