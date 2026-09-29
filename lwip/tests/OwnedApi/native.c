#include "lwip/api.h"
#include "lwip/stats.h"
#include "lwip/tcpip.h"
#include <pthread.h>
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern void native_join_workers(void);extern err_t netconn_prepare_delete(struct netconn*);
static atomic_int ready;static struct netconn *connection;static err_t received;static struct netbuf *packet;
int lwip_host_frame_send(void*n,const unsigned char*p,unsigned len){(void)n;(void)p;(void)len;return 0;}
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static void *receive(void*arg){(void)arg;netconn_thread_init();received=netconn_recv(connection,&packet);netconn_thread_cleanup();return NULL;}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}netconn_thread_init();unsigned before=lwip_stats.memp[MEMP_NETCONN]->used;connection=netconn_new(NETCONN_UDP);R(connection);R(netconn_bind(connection,IP_ANY_TYPE,29900)==ERR_OK);pthread_t thread;R(pthread_create(&thread,NULL,receive,NULL)==0);unsigned tries=0;for(;;){SYS_ARCH_DECL_PROTECT(level);SYS_ARCH_PROTECT(level);unsigned waiting=connection->mbox_threads_waiting;SYS_ARCH_UNPROTECT(level);if(waiting)break;R(tries++<5000);struct timespec t={0,1000000};nanosleep(&t,NULL);}R(netconn_prepare_delete(connection)==ERR_OK);R(pthread_join(thread,NULL)==0);R(received==ERR_CONN&&packet==NULL);R(connection->mbox_threads_waiting==0);R(lwip_stats.memp[MEMP_NETCONN]->used==before+1);R(netconn_delete(connection)==ERR_OK);R(lwip_stats.memp[MEMP_NETCONN]->used==before);netconn_thread_cleanup();R(lwip_host_stop_worker()==ERR_OK);native_join_workers();printf("{\"case\":\"netconn-prepare-wake-join-delete\",\"receive_error\":%d,\"waiters\":0,\"pool_delta\":0}\n",received);return 0;}
