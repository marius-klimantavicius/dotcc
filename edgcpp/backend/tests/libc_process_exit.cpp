#include <stdlib.h>
#include <stdio.h>
#include <threads.h>
static FILE *output;
static mtx_t mutex;
static cnd_t condition;
static int ready;
static tss_t key;
static void mark(int value){fputc(value,stderr);if(output)fputc(value,output);}
static void late(void){mark('C');}
static void first(void){mark('A');}
static void second(void){mark('B');if(atexit(late))_Exit(99);}
static void quick_first(void){mark('x');}
static void quick_late(void){mark('z');}
static void quick_second(void){mark('y');if(at_quick_exit(quick_late))_Exit(98);}
static void repeated(void){mark('R');}
static void tss_destructor(void*p){(void)p;mark('K');}
#ifdef __cplusplus
struct Global {~Global(){mark('G');}} global;
struct LocalTls {~LocalTls(){mark('T');}};
static LocalTls& local_tls(){static thread_local LocalTls value;return value;}
struct Automatic {~Automatic(){mark('M');}};
struct WorkerAutomatic {~WorkerAutomatic(){mark('W');}};
#endif
static int worker(void*p){(void)p;
#ifdef __cplusplus
WorkerAutomatic automatic;
#endif
mtx_lock(&mutex);ready=1;cnd_signal(&condition);mtx_unlock(&mutex);for(;;)thrd_yield();}
int main(int argc,char**argv){
if(argc!=3)return 90;output=fopen(argv[2],"wb");if(!output)return 91;
if(setvbuf(output,0,_IOFBF,8192))return 92;
int (*volatile register_normal)(void(*)(void))=atexit;int (*volatile register_quick)(void(*)(void))=at_quick_exit;
if(register_normal(first)||register_normal(second)||register_quick(quick_first)||register_quick(quick_second))return 93;
if(tss_create(&key,tss_destructor)||tss_set(key,(void*)1))return 94;
#ifdef __cplusplus
Automatic automatic;(void)local_tls();
#endif
int mode=argv[1][0]-'0';
if(mode==4){for(int i=0;i<40;i++)if(atexit(repeated))return 95;}
if(mode==5||mode==6){thrd_t thread;if(mtx_init(&mutex,mtx_plain)||cnd_init(&condition))return 96;mtx_lock(&mutex);if(thrd_create(&thread,worker,0))return 97;while(!ready)cnd_wait(&condition,&mutex);mtx_unlock(&mutex);}
fputs("body:",output);
if(mode==1||mode==5){void(*volatile stop)(int)=exit;stop(18);}
if(mode==2){void(*volatile stop)(int)=quick_exit;stop(19);}
if(mode==3){void(*volatile stop)(int)=_Exit;stop(20);}
return 17;
}
