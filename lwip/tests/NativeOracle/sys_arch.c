#define _POSIX_C_SOURCE 200809L
#include "lwip/sys.h"
#include <pthread.h>
#include <errno.h>
#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdatomic.h>
#include <unistd.h>

#if !NO_SYS
typedef struct { pthread_mutex_t gate; pthread_cond_t change; void **queue; int capacity, count, head; } resource;
static pthread_t workers[16];
static unsigned worker_count;
static pthread_mutex_t protection, core;
static pthread_once_t once = PTHREAD_ONCE_INIT;
static _Thread_local sys_sem_t thread_sem;
static pthread_t core_thread;
static _Thread_local unsigned core_depth;
static atomic_int core_marked, stopping;
static void recursive_mutex(pthread_mutex_t *m) { pthread_mutexattr_t a; pthread_mutexattr_init(&a); pthread_mutexattr_settype(&a,PTHREAD_MUTEX_RECURSIVE); pthread_mutex_init(m,&a); pthread_mutexattr_destroy(&a); }
static void initialize(void) { recursive_mutex(&protection); recursive_mutex(&core); }
void sys_init(void) { pthread_once(&once,initialize); }
static resource *create(int size,int count) { resource *r=calloc(1,sizeof(*r)); if(!r)return NULL; pthread_mutex_init(&r->gate,NULL); pthread_cond_init(&r->change,NULL); r->capacity=size; r->count=count; if(size){r->queue=calloc((size_t)size,sizeof(void*));if(!r->queue){free(r);return NULL;}} return r; }
static void destroy(long *h) { resource *r=(resource*)*h; pthread_cond_destroy(&r->change);pthread_mutex_destroy(&r->gate);free(r->queue);free(r);*h=0; }
static int wait(resource*r,u32_t timeout,struct timespec *deadline) { if(!timeout)return pthread_cond_wait(&r->change,&r->gate); return pthread_cond_timedwait(&r->change,&r->gate,deadline); }
static struct timespec deadline(u32_t ms) { struct timespec t;clock_gettime(CLOCK_REALTIME,&t);t.tv_sec+=ms/1000;t.tv_nsec+=(long)(ms%1000)*1000000;if(t.tv_nsec>=1000000000){t.tv_sec++;t.tv_nsec-=1000000000;}return t; }
err_t sys_sem_new(sys_sem_t *h,u8_t count){*h=(long)create(0,count);return *h?ERR_OK:ERR_MEM;}
void sys_sem_signal(sys_sem_t*h){resource*r=(resource*)*h;pthread_mutex_lock(&r->gate);r->count=1;pthread_cond_signal(&r->change);pthread_mutex_unlock(&r->gate);}
u32_t sys_arch_sem_wait(sys_sem_t*h,u32_t timeout){resource*r=(resource*)*h;struct timespec end=deadline(timeout);u32_t start=sys_now();pthread_mutex_lock(&r->gate);while(!r->count)if(wait(r,timeout,&end)==ETIMEDOUT){pthread_mutex_unlock(&r->gate);return SYS_ARCH_TIMEOUT;}r->count--;pthread_mutex_unlock(&r->gate);return sys_now()-start;}
void sys_sem_free(sys_sem_t*h){destroy(h);}
err_t sys_mbox_new(sys_mbox_t*h,int size){*h=(long)create(size,0);return *h?ERR_OK:ERR_MEM;}
void sys_mbox_post(sys_mbox_t*h,void*msg){resource*r=(resource*)*h;pthread_mutex_lock(&r->gate);while(r->count==r->capacity)pthread_cond_wait(&r->change,&r->gate);r->queue[(r->head+r->count++)%r->capacity]=msg;pthread_cond_broadcast(&r->change);pthread_mutex_unlock(&r->gate);}
err_t sys_mbox_trypost(sys_mbox_t*h,void*msg){resource*r=(resource*)*h;pthread_mutex_lock(&r->gate);if(r->count==r->capacity){pthread_mutex_unlock(&r->gate);return ERR_MEM;}r->queue[(r->head+r->count++)%r->capacity]=msg;pthread_cond_broadcast(&r->change);pthread_mutex_unlock(&r->gate);return ERR_OK;}
err_t sys_mbox_trypost_fromisr(sys_mbox_t*h,void*msg){return sys_mbox_trypost(h,msg);}
u32_t sys_arch_mbox_fetch(sys_mbox_t*h,void**msg,u32_t timeout){resource*r=(resource*)*h;struct timespec end=deadline(timeout);u32_t start=sys_now();pthread_mutex_lock(&r->gate);while(!r->count)if(wait(r,timeout,&end)==ETIMEDOUT){pthread_mutex_unlock(&r->gate);return SYS_ARCH_TIMEOUT;}if(msg)*msg=r->queue[r->head];r->head=(r->head+1)%r->capacity;r->count--;pthread_cond_broadcast(&r->change);pthread_mutex_unlock(&r->gate);return sys_now()-start;}
u32_t sys_arch_mbox_tryfetch(sys_mbox_t*h,void**msg){resource*r=(resource*)*h;pthread_mutex_lock(&r->gate);if(!r->count){pthread_mutex_unlock(&r->gate);return SYS_MBOX_EMPTY;}if(msg)*msg=r->queue[r->head];r->head=(r->head+1)%r->capacity;r->count--;pthread_cond_broadcast(&r->change);pthread_mutex_unlock(&r->gate);return 0;}
void sys_mbox_free(sys_mbox_t*h){resource*r=(resource*)*h;if(r->count)abort();destroy(h);}
err_t sys_mutex_new(sys_mutex_t*h){resource*r=calloc(1,sizeof(*r));if(!r)return ERR_MEM;recursive_mutex(&r->gate);*h=(long)r;return ERR_OK;}
void sys_mutex_lock(sys_mutex_t*h){pthread_mutex_lock(&((resource*)*h)->gate);}
void sys_mutex_unlock(sys_mutex_t*h){pthread_mutex_unlock(&((resource*)*h)->gate);}
void sys_mutex_free(sys_mutex_t*h){resource*r=(resource*)*h;pthread_mutex_destroy(&r->gate);free(r);*h=0;}
sys_prot_t sys_arch_protect(void){sys_init();pthread_mutex_lock(&protection);return 0;}
void sys_arch_unprotect(sys_prot_t p){(void)p;pthread_mutex_unlock(&protection);}
struct start {lwip_thread_fn function;void*argument;};
static void *worker(void*arg){struct start s=*(struct start*)arg;free(arg);s.function(s.argument);return NULL;}
sys_thread_t sys_thread_new(const char*name,lwip_thread_fn f,void*arg,int stack,int priority){(void)name;(void)stack;(void)priority;struct start*s=malloc(sizeof(*s));if(!s)abort();s->function=f;s->argument=arg;if(worker_count==16||pthread_create(&workers[worker_count],NULL,worker,s))abort();return (long)++worker_count;}
void lwip_host_mark_tcpip_thread(void){core_thread=pthread_self();atomic_store(&core_marked,1);}
void lwip_host_assert_core_locked(void){if(atomic_load(&core_marked)&&!pthread_equal(core_thread,pthread_self())&&!core_depth)lwip_host_assert("core ownership",__FILE__,__LINE__);}
void lwip_host_lock_core(void){sys_init();pthread_mutex_lock(&core);core_depth++;}
void lwip_host_unlock_core(void){if(!core_depth)abort();core_depth--;pthread_mutex_unlock(&core);}
int lwip_host_worker_stopping(void){return atomic_load(&stopping);}
void lwip_host_request_worker_stop(void){atomic_store(&stopping,1);}
sys_sem_t *lwip_host_netconn_sem_get(void){return thread_sem?&thread_sem:NULL;}
void lwip_host_netconn_sem_alloc(void){if(!thread_sem&&sys_sem_new(&thread_sem,0))abort();}
void lwip_host_netconn_sem_free(void){if(thread_sem)sys_sem_free(&thread_sem);}
void native_join_workers(void){for(unsigned i=0;i<worker_count;i++)pthread_join(workers[i],NULL);}
#endif
u32_t sys_now(void){struct timespec t;clock_gettime(CLOCK_MONOTONIC,&t);return (u32_t)((unsigned long)t.tv_sec*1000+t.tv_nsec/1000000);}
u32_t sys_jiffies(void){return sys_now();}
uint32_t lwip_host_random(void){static uint32_t seed=0x12345678;seed^=seed<<13;seed^=seed>>17;seed^=seed<<5;return seed;}
void lwip_host_assert(const char*message,const char*file,int line){fprintf(stderr,"ASSERT %s:%d %s\n",file,line,message);exit(91);}
