#include <threads.h>
#include <time.h>
#include <stdlib.h>
#ifdef __cplusplus
extern "C" {
#endif
static mtx_t mutex, timed_mutex, recursive;
static cnd_t condition;
static tss_t key;
static once_flag once=ONCE_FLAG_INIT;
static int count,destructor_calls,once_calls,ready;
static thread_local int local_value;
struct item { thrd_t self;thrd_t *published;int rounds;int exit; };
static void destructor(void*p){struct item*d=(struct item*)p;mtx_lock(&mutex);++destructor_calls;mtx_unlock(&mutex);if(--d->rounds)tss_set(key,p);}
static void initialize_once(void){++once_calls;}
static int worker(void*p){struct item*d=(struct item*)p;d->self=thrd_current();if(!thrd_equal(d->self,*d->published))return -4;if(local_value)return -1;local_value=37;if(tss_set(key,p)!=thrd_success)return -2;for(int i=0;i<1200;i++){if(mtx_lock(&mutex)!=thrd_success)return -3;++count;mtx_unlock(&mutex);}call_once(&once,initialize_once);if(d->exit)thrd_exit(-71);return 83;}
static struct timespec deadline(long ms){struct timespec t;timespec_get(&t,TIME_UTC);t.tv_nsec+=ms*1000000;if(t.tv_nsec>=1000000000){t.tv_nsec-=1000000000;t.tv_sec++;}return t;}
static int timed_worker(void*p){(void)p;struct timespec t=deadline(30);return mtx_timedlock(&timed_mutex,&t);}
static int notifier(void*p){(void)p;mtx_lock(&mutex);ready=1;cnd_signal(&condition);mtx_unlock(&mutex);return 9;}
int c11_thread_cases(void){
if(mtx_init(&mutex,mtx_plain)!=thrd_success||tss_create(&key,destructor)!=thrd_success||cnd_init(&condition)!=thrd_success)return 1;
struct item items[4];thrd_t threads[4];count=destructor_calls=once_calls=ready=0;
for(int i=0;i<4;i++){items[i].rounds=6;items[i].exit=i%2;items[i].published=threads+i;if(thrd_create(threads+i,worker,items+i)!=thrd_success)return 2;}
for(int i=0;i<4;i++){int result;if(thrd_join(threads[i],&result)!=thrd_success||result!=(i%2?-71:83)||!thrd_equal(items[i].self,threads[i])||items[i].rounds!=2)return 3;}
if(count!=4800||destructor_calls!=16||once_calls!=1||local_value)return 4;
if(mtx_init(&timed_mutex,mtx_timed)!=thrd_success||mtx_lock(&timed_mutex)!=thrd_success)return 5;
thrd_t t;int result;if(thrd_create(&t,timed_worker,0)!=thrd_success||thrd_join(t,&result)!=thrd_success||result!=thrd_timedout)return 6;mtx_unlock(&timed_mutex);
if(mtx_init(&recursive,mtx_recursive|mtx_timed)!=thrd_success||mtx_lock(&recursive)!=thrd_success||mtx_trylock(&recursive)!=thrd_success)return 7;mtx_unlock(&recursive);struct timespec past={0,0};if(cnd_timedwait(&condition,&recursive,&past)!=thrd_timedout)return 8;mtx_unlock(&recursive);
mtx_lock(&mutex);if(thrd_create(&t,notifier,0)!=thrd_success)return 9;while(!ready)if(cnd_wait(&condition,&mutex)!=thrd_success)return 10;mtx_unlock(&mutex);if(thrd_join(t,&result)!=thrd_success||result!=9)return 11;
struct timespec duration={0,1000000};if(thrd_sleep(&duration,&duration)!=0)return 12;thrd_yield();
tss_delete(key);cnd_destroy(&condition);mtx_destroy(&mutex);mtx_destroy(&recursive);mtx_destroy(&timed_mutex);return 0;
}
static tss_t exit_key;
static int exit_destructor_calls;
static void exit_destructor(void*p){(void)p;++exit_destructor_calls;thrd_exit(53);}
static int exit_destructor_worker(void*p){(void)p;if(tss_set(exit_key,(void*)1)!=thrd_success)return -1;return 1;}
int c11_destructor_exit_cases(void){exit_destructor_calls=0;if(tss_create(&exit_key,exit_destructor)!=thrd_success)return 1;thrd_t t;int result;if(thrd_create(&t,exit_destructor_worker,0)!=thrd_success||thrd_join(t,&result)!=thrd_success)return 2;tss_delete(exit_key);return result!=53||exit_destructor_calls!=1?3:0;}
int c11_thread_count(void){return count;}
#ifdef __cplusplus
}
#endif
#ifdef __cplusplus
#if defined(EDG_C11_EXIT_INTEROP)
extern "C" int __edg_cxx_uncaught_exceptions() noexcept;
static unsigned long mixed_trace;
static int mixed_caught,mixed_tls_error,mixed_result;
static tss_t mixed_key;
static void mixed_mark(int n){if(__edg_cxx_uncaught_exceptions()!=0)mixed_tls_error=1;mixed_trace=mixed_trace*10+n;}
struct MixedAutomatic {int id;MixedAutomatic(int n):id(n){} ~MixedAutomatic(){mixed_mark(id);} };
struct MixedTls { int value; ~MixedTls(){mixed_mark(6);} };
static thread_local MixedTls mixed_tls;
static void mixed_key_destructor(void*p){(void)p;if(mixed_trace!=543216)mixed_tls_error=1;mixed_mark(7);}
static void mixed_exit_noexcept() noexcept {MixedAutomatic tail(5);thrd_exit(39);}
static void mixed_nested(){MixedAutomatic array[3]={2,3,4};mixed_exit_noexcept();}
static int mixed_worker(void*p){(void)p;MixedAutomatic outer(1);mixed_tls.value=1;tss_set(mixed_key,&mixed_tls);try{mixed_nested();}catch(...){++mixed_caught;}return -1;}
struct MixedPayload {MixedAutomatic member;MixedPayload():member(7){}MixedPayload(const MixedPayload&):member(8){MixedAutomatic local(9);thrd_exit(41);}~MixedPayload(){mixed_mark(6);}};
static MixedPayload& mixed_reference(MixedPayload&p){return p;}
static int mixed_copy_worker(void*p){(void)p;MixedAutomatic outer(1);MixedPayload payload;try{throw mixed_reference(payload);}catch(...){++mixed_caught;}return -1;}
extern "C" int c11_mixed_cases(void){
mixed_trace=0;mixed_caught=mixed_tls_error=0;if(tss_create(&mixed_key,mixed_key_destructor)!=thrd_success)return 1;thrd_t t;int r;if(thrd_create(&t,mixed_worker,0)!=thrd_success||thrd_join(t,&r)!=thrd_success)return 2;tss_delete(mixed_key);if(r!=39||mixed_trace!=5432167||mixed_caught||mixed_tls_error)return 3;
mixed_trace=0;if(thrd_create(&t,mixed_copy_worker,0)!=thrd_success||thrd_join(t,&r)!=thrd_success)return 4;mixed_result=r;return r!=41||mixed_trace!=98671||mixed_caught||mixed_tls_error?5:0;}
static tss_t tls_exit_key;
struct RemainingTls {int value;~RemainingTls(){mixed_mark(3);}};
struct ExitingTls {int value;~ExitingTls() noexcept(false){mixed_mark(2);thrd_exit(59);}};
static RemainingTls& remaining_tls(){static thread_local RemainingTls v;return v;}
static ExitingTls& exiting_tls(){static thread_local ExitingTls v;return v;}
static void tls_exit_key_destructor(void*p){(void)p;if(mixed_trace!=123)mixed_tls_error=1;mixed_mark(4);}
static int tls_exit_worker(void*p){(void)p;MixedAutomatic automatic(1);remaining_tls().value=1;exiting_tls().value=1;tss_set(tls_exit_key,(void*)1);return 7;}
extern "C" int c11_tls_exit_cases(void){mixed_trace=0;mixed_tls_error=0;if(tss_create(&tls_exit_key,tls_exit_key_destructor)!=thrd_success)return 1;thrd_t t;int result;if(thrd_create(&t,tls_exit_worker,0)!=thrd_success||thrd_join(t,&result)!=thrd_success)return 2;tss_delete(tls_exit_key);mixed_result=result;return result!=59||mixed_trace!=1234||mixed_tls_error?3:0;}
extern "C" unsigned long c11_mixed_diagnostic(void){return mixed_trace;}
extern "C" int c11_mixed_flags(void){return mixed_result+100*mixed_tls_error+1000*mixed_caught;}
#else
extern "C" int c11_mixed_cases(void){return 0;}
extern "C" int c11_tls_exit_cases(void){return 0;}
extern "C" unsigned long c11_mixed_diagnostic(void){return 0;}
extern "C" int c11_mixed_flags(void){return 0;}
#endif
#else
int c11_mixed_cases(void){return 0;}
int c11_tls_exit_cases(void){return 0;}
unsigned long c11_mixed_diagnostic(void){return 0;}
int c11_mixed_flags(void){return 0;}
#endif
