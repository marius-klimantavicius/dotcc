#include "lwip/sockets.h"
#include "lwip/stats.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <string.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern void native_join_workers(void);extern void *lwip_owned_socket_pin(int);extern void lwip_owned_socket_unpin(void*);
static atomic_int ready;static void initialized(void*p){(void)p;atomic_store(&ready,1);}
int lwip_host_frame_send(void*n,const unsigned char*p,unsigned len){(void)n;(void)p;(void)len;return 0;}
int main(int argc,char**argv){int pin=argc<2||strcmp(argv[1],"--original");if(argc>1&&!strcmp(argv[1],"--negative"))R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}lwip_socket_thread_init();unsigned baseline=lwip_stats.memp[MEMP_NETCONN]->used;int original=lwip_socket(AF_INET,SOCK_DGRAM,0);R(original>=0);void*lease=pin?lwip_owned_socket_pin(original):NULL;R(!pin||lease);R(lwip_close(original)==0);int other=lwip_socket(AF_INET,SOCK_DGRAM,0);R(other>=0);fprintf(stderr,"descriptor original=%d reallocated=%d pin=%d\n",original,other,pin);R(other!=original);char byte;R(lwip_recv(original,&byte,1,MSG_DONTWAIT)==-1&&errno==EBADF);int kind=0;socklen_t size=sizeof kind;R(lwip_getsockopt(other,SOL_SOCKET,SO_TYPE,&kind,&size)==0&&kind==SOCK_DGRAM);lwip_owned_socket_unpin(lease);int reused=lwip_socket(AF_INET,SOCK_DGRAM,0);R(reused==original);R(lwip_close(reused)==0);R(lwip_close(other)==0);R(lwip_stats.memp[MEMP_NETCONN]->used==baseline);lwip_socket_thread_cleanup();R(lwip_host_stop_worker()==0);native_join_workers();puts("{\"case\":\"socket-admission-pin-prevents-close-before-syscall-fd-reuse\",\"old_operation_errno\":9,\"reuse_after_unpin\":true,\"netconn_delta\":0}");return 0;}
