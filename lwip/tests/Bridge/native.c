#include "lwip/netif.h"
#include "lwip/tcpip.h"
#include "lwip/timeouts.h"
#include "lwip/stats.h"
#include "netif/bridgeif.h"
#include "netif/ethernet.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);
extern int lwip_owned_bridge_barrier(void);extern void *lwip_owned_bridge_new(const unsigned char*,unsigned,unsigned,unsigned);extern struct netif *lwip_owned_bridge_netif(void*);extern int lwip_owned_bridge_add(void*,struct netif*);extern int lwip_owned_bridge_stop(void*);extern int lwip_owned_bridge_free(void*);
static atomic_int ready;static atomic_uint clock_ms=1000;u32_t sys_now(void){return atomic_load(&clock_ms);}
static struct netif ports[6];static void *bridges[2];static unsigned count,mask;static unsigned char expected[64];static unsigned baseline_mem,baseline_timeouts,baseline_pbuf;
int lwip_host_frame_send(void*n,const unsigned char*b,unsigned len){(void)n;(void)b;(void)len;return 0;}
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static err_t output(struct netif*n,struct pbuf*p){if(p->tot_len!=64)return ERR_OK;unsigned char bytes[64];R(pbuf_copy_partial(p,bytes,64,0)==64);if(bytes[12]!=0x88||bytes[13]!=0xb5)return ERR_OK;R(!memcmp(bytes,expected,64));count++;mask|=1u<<(unsigned)(n-ports);return ERR_OK;}
static err_t port_init(struct netif*n){n->flags=NETIF_FLAG_BROADCAST|NETIF_FLAG_ETHARP|NETIF_FLAG_ETHERNET|NETIF_FLAG_LINK_UP;n->name[0]='p';n->name[1]='t';n->hwaddr_len=6;n->hwaddr[0]=2;n->hwaddr[5]=(unsigned)(n-ports)+1;n->mtu=1500;n->linkoutput=output;return ERR_OK;}
static void baseline(void*arg){(void)arg;baseline_mem=lwip_stats.mem.used;baseline_timeouts=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;baseline_pbuf=lwip_stats.memp[MEMP_PBUF_POOL]->used;}
static void setup(void*arg){(void)arg;for(unsigned i=0;i<6;i++){R(netif_add(&ports[i],NULL,NULL,NULL,NULL,port_init,ethernet_input));netif_set_up(&ports[i]);}baseline(NULL);unsigned char mac[6]={2,0,0,0,0x42,1};for(unsigned b=0;b<2;b++){mac[5]=b+1;bridges[b]=lwip_owned_bridge_new(mac,3,8,2);R(bridges[b]);R(lwip_owned_bridge_free(bridges[b])==ERR_USE);for(unsigned i=0;i<3;i++)R(lwip_owned_bridge_add(bridges[b],&ports[b*3+i])==ERR_OK);R(lwip_owned_bridge_add(bridges[b],&ports[b*3])==ERR_USE);}struct netif foreign;R(lwip_owned_bridge_add(bridges[0],&foreign)==ERR_ARG);R(lwip_owned_bridge_stop((void*)1)==ERR_ARG);}
struct input{unsigned ingress,source,destination;};
static void input(void*arg){struct input*in=arg;memset(expected,0,sizeof expected);expected[0]=2;expected[5]=in->destination;expected[6]=2;expected[11]=in->source;expected[12]=0x88;expected[13]=0xb5;for(unsigned i=14;i<64;i++)expected[i]=(unsigned char)(i*7);count=mask=0;struct pbuf*p=pbuf_alloc(PBUF_RAW,sizeof expected,PBUF_POOL);R(p);R(pbuf_take(p,expected,sizeof expected)==ERR_OK);R(ports[in->ingress].input(p,&ports[in->ingress])==ERR_OK);}
static void empty(void*arg){(void)arg;}
static void send(unsigned port,unsigned src,unsigned dst,unsigned wanted){struct input in={port,src,dst};R(lwip_host_call(input,&in)==0);R(lwip_host_call(empty,NULL)==0);R(mask==wanted);R(count==(unsigned)__builtin_popcount(wanted));R(lwip_stats.memp[MEMP_PBUF_POOL]->used==baseline_pbuf);}
static void static_cases(void*arg){(void)arg;struct netif*n=lwip_owned_bridge_netif(bridges[0]);struct eth_addr a={{2,0,0,0,0,40}},b={{2,0,0,0,0,41}},c={{2,0,0,0,0,42}};R(bridgeif_fdb_add(n,&a,0)==ERR_OK);R(bridgeif_fdb_add(n,&b,4)==ERR_OK);R(bridgeif_fdb_add(n,&c,1)==ERR_MEM);R(bridgeif_fdb_remove(n,&c)==ERR_VAL);}
static void remove_static(void*arg){(void)arg;struct eth_addr a={{2,0,0,0,0,40}};R(bridgeif_fdb_remove(lwip_owned_bridge_netif(bridges[0]),&a)==ERR_OK);R(bridgeif_fdb_remove(lwip_owned_bridge_netif(bridges[0]),&a)==ERR_VAL);}
static void age(void*arg){unsigned ticks=(unsigned)(size_t)arg;for(unsigned i=0;i<ticks;i++){atomic_fetch_add(&clock_ms,1000);sys_check_timeouts();for(struct netif*n=netif_list;n;n=n->next)netif_poll(n);}}
static void link_down(void*arg){(void)arg;netif_set_link_down(&ports[2]);}
static void link_up(void*arg){(void)arg;netif_set_link_up(&ports[2]);}
static void stop_a(void*arg){(void)arg;struct netif *bridge=lwip_owned_bridge_netif(bridges[0]);struct pbuf *queued=pbuf_alloc(PBUF_RAW,20,PBUF_RAM);R(queued);memset(queued->payload,0,20);((unsigned char*)queued->payload)[0]=0x45;R(netif_loop_output(bridge,queued)==ERR_OK);pbuf_free(queued);R(bridge->loop_first);netif_input_fn installed=ports[0].input;ports[0].input=ethernet_input;R(lwip_owned_bridge_stop(bridges[0])==ERR_USE);R(lwip_owned_bridge_netif(bridges[0]));ports[0].input=installed;R(lwip_owned_bridge_stop(bridges[0])==ERR_OK);R(lwip_owned_bridge_stop(bridges[0])==ERR_OK);R(!lwip_owned_bridge_netif(bridges[0]));R(!bridge->loop_first);for(unsigned i=0;i<3;i++)R(ports[i].input==ethernet_input&&(ports[i].flags&NETIF_FLAG_ETHARP));}
static void free_a(void*arg){(void)arg;R(lwip_owned_bridge_free(bridges[0])==ERR_OK);R(lwip_owned_bridge_free(bridges[0])==ERR_ARG);}
static void stop_b(void*arg){(void)arg;R(lwip_owned_bridge_stop(bridges[1])==ERR_OK);}
static void finish(void*arg){(void)arg;R(lwip_owned_bridge_stop(bridges[1])==ERR_OK);R(lwip_owned_bridge_free(bridges[1])==ERR_OK);for(unsigned i=3;i<6;i++)R(ports[i].input==ethernet_input&&(ports[i].flags&NETIF_FLAG_ETHARP));fprintf(stderr,"memory baseline=%u actual=%u timeouts=%u/%u pbuf=%u/%u\n",baseline_mem,(unsigned)lwip_stats.mem.used,baseline_timeouts,(unsigned)lwip_stats.memp[MEMP_SYS_TIMEOUT]->used,baseline_pbuf,(unsigned)lwip_stats.memp[MEMP_PBUF_POOL]->used);R(lwip_stats.mem.used==baseline_mem);R(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==baseline_timeouts);R(lwip_stats.memp[MEMP_PBUF_POOL]->used==baseline_pbuf);for(unsigned i=0;i<6;i++)netif_remove(&ports[i]);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);send(0,10,20,6);send(1,20,10,1);send(2,10,20,2);send(1,20,10,4);R(lwip_host_call(static_cases,NULL)==0);send(0,10,40,0);send(0,10,41,4);R(lwip_host_call(remove_static,NULL)==0);send(0,10,40,6);R(lwip_host_call(link_down,NULL)==0);send(0,10,99,2);R(lwip_host_call(link_up,NULL)==0);R(lwip_host_call(age,(void*)301)==0);send(0,10,20,6);send(3,10,20,48);send(4,20,10,8);R(lwip_owned_bridge_barrier()==ERR_OK);R(lwip_host_call(stop_a,NULL)==0);R(lwip_owned_bridge_barrier()==ERR_OK);R(lwip_host_call(free_a,NULL)==0);send(4,20,10,8);R(lwip_host_call(age,(void*)301)==0);send(4,20,10,40);R(lwip_owned_bridge_barrier()==ERR_OK);R(lwip_host_call(stop_b,NULL)==0);R(lwip_owned_bridge_barrier()==ERR_OK);R(lwip_host_call(finish,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
puts("{\"case\":\"bridge-flood-learn-migrate-static-capacity-aging-link-retry-two-bridge-baseline\"}");return 0;}
