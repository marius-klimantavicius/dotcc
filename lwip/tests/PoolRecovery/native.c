#include "lwip/udp.h"
#include "lwip/pbuf.h"
#include "lwip/stats.h"
#include "lwip/tcpip.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);
static atomic_int ready;static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static void cases(void*arg){(void)arg;struct udp_pcb*pcbs[MEMP_NUM_UDP_PCB];struct pbuf*packets[PBUF_POOL_SIZE];for(unsigned i=0;i<MEMP_NUM_UDP_PCB;i++){pcbs[i]=udp_new();R(pcbs[i]);}R(!udp_new());R(lwip_stats.memp[MEMP_UDP_PCB]->used==MEMP_NUM_UDP_PCB);for(unsigned i=0;i<MEMP_NUM_UDP_PCB;i++)udp_remove(pcbs[i]);R(lwip_stats.memp[MEMP_UDP_PCB]->used==0);struct udp_pcb*recovered=udp_new();R(recovered);udp_remove(recovered);for(unsigned i=0;i<PBUF_POOL_SIZE;i++){packets[i]=pbuf_alloc(PBUF_RAW,32,PBUF_POOL);R(packets[i]);}R(!pbuf_alloc(PBUF_RAW,32,PBUF_POOL));R(lwip_stats.memp[MEMP_PBUF_POOL]->used==PBUF_POOL_SIZE);for(unsigned i=0;i<PBUF_POOL_SIZE;i++)R(pbuf_free(packets[i])==1);R(lwip_stats.memp[MEMP_PBUF_POOL]->used==0);struct pbuf*p=pbuf_alloc(PBUF_RAW,32,PBUF_POOL);R(p);pbuf_free(p);printf("{\"case\":\"pool-recovery\",\"udp_capacity\":%u,\"pbuf_capacity\":%u,\"udp_after\":0,\"pbuf_after\":0}\n",MEMP_NUM_UDP_PCB,PBUF_POOL_SIZE);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(cases,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
return 0;}
