#include "lwip/netif.h"
#include "lwip/tcpip.h"
#include "lwip/stats.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);extern void*lwip_host_interface_add(const char*,const char*,const char*,const char*,const unsigned char*);extern void lwip_host_interface_up(void*);extern void lwip_host_interface_remove(void*);extern void lwip_host_interface_stop(void*);extern void lwip_host_interface_free(void*);extern int lwip_host_barrier(void);
static atomic_int ready;static struct netif *interface;static unsigned baseline;static int original;
int lwip_host_frame_send(void*n,const unsigned char*p,unsigned len){(void)n;(void)p;(void)len;return 0;}
static void initialized(void*p){(void)p;atomic_store(&ready,1);}
static void barrier(void){R(lwip_host_barrier()==ERR_OK);}
static void setup(void*p){(void)p;unsigned char mac[]={2,0,0,0,9,1};interface=lwip_host_interface_add("10.9.0.1","255.255.255.0","0.0.0.0","",mac);R(interface);lwip_host_interface_up(interface);baseline=lwip_stats.mem.used;}
static void retire(void*p){(void)p;struct pbuf*packet=pbuf_alloc(PBUF_RAW,40,PBUF_RAM);R(packet);memset(packet->payload,0,40);unsigned char *ip6=packet->payload;ip6[0]=0x60;ip6[6]=59;ip6[7]=1;ip6[8]=0xfe;ip6[9]=0x80;ip6[23]=2;ip6[24]=0xff;ip6[25]=2;ip6[39]=1;R(netif_loop_output(interface,packet)==ERR_OK);pbuf_free(packet);R(interface->loop_first);if(original){lwip_host_interface_remove(interface);return;}lwip_host_interface_stop(interface);R(interface->loop_first==NULL);}
int main(int argc,char**argv){if(argc>1&&!strcmp(argv[1],"--negative"))R(0);original=argc>1&&!strcmp(argv[1],"--original");lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);R(lwip_host_call(retire,NULL)==0);barrier();R(lwip_stats.mem.used==baseline);if(!original)lwip_host_interface_free(interface);R(lwip_host_stop_worker()==ERR_OK);
#if !NO_SYS
native_join_workers();
#endif
puts("{\"case\":\"interface-retire-drain-fifo-free\",\"heap_delta\":0,\"pending_loopback\":0}");return 0;}
