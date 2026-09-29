#include "lwip/autoip.h"
#include "lwip/acd.h"
#include "lwip/tcpip.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);extern void lwip_host_poll(void);
extern void*lwip_host_interface_add(const char*,const char*,const char*,const char*,const unsigned char*);extern void lwip_host_interface_up(void*);extern void lwip_host_interface_remove(void*);extern int lwip_host_frame_input(void*,const unsigned char*,unsigned);
static atomic_int ready;static struct netif*client;static struct autoip state;static unsigned probes,announcements;static int bound;static unsigned address;
static unsigned read32(const unsigned char*p){return (unsigned)p[0]<<24|(unsigned)p[1]<<16|(unsigned)p[2]<<8|p[3];}
int lwip_host_frame_send(void*interface,const unsigned char*frame,unsigned length){(void)interface;if(length>=42&&frame[12]==8&&frame[13]==6&&frame[21]==1){unsigned sender=read32(frame+28),target=read32(frame+38);if((target>>16)==0xa9fe&&!sender)probes++;if((sender>>16)==0xa9fe&&sender==target)announcements++;}return 0;}
static void initialized(void*unused){(void)unused;atomic_store(&ready,1);}
static void setup(void*unused){(void)unused;unsigned char mac[]={2,0,0,0,0,1};client=lwip_host_interface_add("0.0.0.0","0.0.0.0","0.0.0.0","fd00::1",mac);R(client);lwip_host_interface_up(client);autoip_set_struct(client,&state);R(autoip_start(client)==0);}
static void tick(void*unused){(void)unused;acd_tmr();}
static void empty(void*unused){(void)unused;}
static void drain(void){R(lwip_host_call(empty,NULL)==0);lwip_host_poll();R(lwip_host_call(empty,NULL)==0);}
static void check(void*unused){(void)unused;bound=autoip_supplied_address(client);if(bound){address=lwip_ntohl(netif_ip4_addr(client)->addr);R(address>=0xa9fe0100&&address<=0xa9fefeff);R(netif_ip4_netmask(client)->addr==PP_HTONL(0xffff0000));R(netif_ip4_gw(client)->addr==0);}}
static void until_bound(void){for(unsigned i=0;i<400;i++){drain();R(lwip_host_call(check,NULL)==0);if(bound)return;R(lwip_host_call(tick,NULL)==0);}fprintf(stderr,"autoip state=%u acd=%u ttw=%u probe=%u announce=%u ip=%x selected=%x\n",state.state,state.acd.state,state.acd.ttw,probes,announcements,netif_ip4_addr(client)->addr,state.llipaddr.addr);R(0);}
static void conflict(void*unused){(void)unused;unsigned char frame[42]={2,0,0,0,0,1,2,0,0,0,0,99,8,6,0,1,8,0,6,4,0,2,2,0,0,0,0,99,0,0,0,0,2,0,0,0,0,1};for(unsigned i=0;i<4;i++)frame[28+i]=frame[38+i]=address>>(24-8*i);R(lwip_host_frame_input(client,frame,42)==0);}
static void link_cycle(void*unused){(void)unused;netif_set_link_down(client);netif_set_link_up(client);R(!autoip_supplied_address(client));R(autoip_start(client)==0);}
static void cleanup(void*unused){(void)unused;R(autoip_stop(client)==0);acd_stop(&state.acd);acd_remove(client,&state.acd);autoip_remove_struct(client);lwip_host_interface_remove(client);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);until_bound();unsigned first=address;R(probes>=3&&announcements>=1);unsigned before=announcements;R(lwip_host_call(conflict,NULL)==0);drain();R(lwip_host_call(check,NULL)==0);R(bound&&announcements>before);R(lwip_host_call(conflict,NULL)==0);drain();R(lwip_host_call(check,NULL)==0);R(!bound);until_bound();R(address!=first);unsigned second=address;R(lwip_host_call(link_cycle,NULL)==0);until_bound();R(address==second);R(lwip_host_call(cleanup,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
printf("{\"case\":\"autoip-probe-conflict-link-recovery\",\"probes\":%u,\"announcements\":%u}\n",probes,announcements);return 0;}
