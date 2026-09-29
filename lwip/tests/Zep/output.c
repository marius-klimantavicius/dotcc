/* Valid original radio output, fragmentation/reassembly and IPv6 MTU controls.
 * Delivery changes order or repeats complete unmodified frames only. */
#include "lwip/init.h"
#include "lwip/stats.h"
#include "lwip/udp.h"
#include "netif/lowpan6.h"
#include "lowpan_service.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define REQUIRE(x) do{if(!(x)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);
extern int lwip_host_call(void(*)(void*),void*);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
static atomic_int initialized;
static unsigned char frames[64][127];static unsigned lengths[64],count,received,expected;
static void ready(void*a){(void)a;atomic_store(&initialized,1);}
static unsigned crc(const unsigned char*bytes,unsigned length)
{unsigned value=0;for(unsigned i=0;i<length;i++){value^=bytes[i];for(unsigned bit=0;bit<8;bit++)value=(value>>1)^((value&1)?0x8408:0);}return value;}
static unsigned char pattern(unsigned i){return (unsigned char)(i*13+(i>>8));}
static err_t capture(struct netif*n,struct pbuf*p)
{
 (void)n;REQUIRE(count<64&&p->tot_len>=2&&p->tot_len<=127);unsigned length=p->tot_len;
 REQUIRE(pbuf_copy_partial(p,frames[count],(u16_t)length,0)==length);lengths[count]=length;
 REQUIRE(crc(frames[count],length-2)==(unsigned)(frames[count][length-2]|(frames[count][length-1]<<8)));count++;return ERR_OK;
}
static void input(void*a,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*address,u16_t port)
{
 (void)a;(void)pcb;REQUIRE(IP_IS_V6(address)&&port==9101&&p->tot_len==expected);
 unsigned char bytes[1500];REQUIRE(expected<=sizeof(bytes)&&pbuf_copy_partial(p,bytes,(u16_t)expected,0)==expected);
 for(unsigned i=0;i<expected;i++)REQUIRE(bytes[i]==pattern(i));pbuf_free(p);received++;
}
static void deliver(struct netif*receiver,unsigned index)
{
 REQUIRE(index<count);unsigned length=lengths[index]-2;struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)length,PBUF_RAM);
 REQUIRE(p&&pbuf_take(p,frames[index],(u16_t)length)==0);REQUIRE(lowpan6_input(p,receiver)==ERR_OK);
}
static void send(struct udp_pcb*sender,struct netif*net,const ip_addr_t*source,unsigned size)
{
 expected=size;count=0;ip_addr_t destination;REQUIRE(ipaddr_aton("ff02::1",&destination));ip6_addr_assign_zone(ip_2_ip6(&destination),IP6_MULTICAST,net);
 unsigned char bytes[1500];REQUIRE(size<=sizeof(bytes));for(unsigned i=0;i<size;i++)bytes[i]=pattern(i);
 struct pbuf*p=pbuf_alloc(PBUF_TRANSPORT,(u16_t)size,PBUF_RAM);REQUIRE(p&&pbuf_take(p,bytes,(u16_t)size)==0);
 REQUIRE(udp_sendto_if_src(sender,p,&destination,9100,net,source)==ERR_OK);pbuf_free(p);REQUIRE(count>0);
}
static void run(void*argument)
{
 (void)argument;unsigned memory=lwip_stats.mem.used,pool=lwip_stats.memp[MEMP_PBUF_POOL]->used,timers=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;
 void*a=lwip_owned_lowpan_new(0,NULL),*b=lwip_owned_lowpan_new(0,NULL);REQUIRE(a&&b);struct netif*na=lwip_owned_lowpan_netif(a),*nb=lwip_owned_lowpan_netif(b);REQUIRE(na&&nb);
 ip_addr_t source,local,any;REQUIRE(ipaddr_aton("fd00::2",&source)&&ipaddr_aton("fd00::1",&local));ip_addr_set_any(IPADDR_TYPE_V6,&any);
 netif_ip6_addr_set(na,0,ip_2_ip6(&source));netif_ip6_addr_set_state(na,0,IP6_ADDR_PREFERRED);netif_ip6_addr_set(nb,0,ip_2_ip6(&local));netif_ip6_addr_set_state(nb,0,IP6_ADDR_PREFERRED);
 na->hwaddr_len=nb->hwaddr_len=6;na->hwaddr[0]=nb->hwaddr[0]=2;na->hwaddr[5]=2;nb->hwaddr[5]=1;na->linkoutput=nb->linkoutput=capture;
 netif_set_up(na);netif_set_up(nb);netif_set_link_up(na);netif_set_link_up(nb);netif_set_default(na);REQUIRE(netif_default==na&&na->mtu==1280);
 REQUIRE(lowpan6_set_pan_id(0x1234)==ERR_OK);ip6_addr_t prefix;REQUIRE(ip6addr_aton("fd00::",&prefix)&&lowpan6_set_context(0,&prefix)==ERR_OK);
 struct udp_pcb*sender=udp_new_ip_type(IPADDR_TYPE_V6),*receiver=udp_new_ip_type(IPADDR_TYPE_V6);REQUIRE(sender&&receiver&&udp_bind(sender,&source,9101)==ERR_OK&&udp_bind(receiver,&any,9100)==ERR_OK);udp_recv(receiver,input,NULL);
 unsigned sizes[]={17,900,1232,1233};
 for(unsigned j=0;j<sizeof(sizes)/sizeof(sizes[0]);j++){unsigned before=received;send(sender,na,&source,sizes[j]);if(sizes[j]>=900)REQUIRE(count>2);fprintf(stderr,"valid output payload=%u frames=%u\n",sizes[j],count);for(unsigned i=0;i<count;i++)deliver(nb,i);fprintf(stderr,"after delivery payload=%u received_delta=%u ip6_memerr=%u max_pbufs=%u loopback=%u\n",sizes[j],received-before,lwip_stats.ip6_frag.memerr,IP_REASS_MAX_PBUFS,(unsigned)(na->loop_first!=NULL));REQUIRE(received==before+1);}
 /* A duplicate valid FRAGN with unchanged bytes is ignored by the original algorithm. */
 fprintf(stderr,"valid duplicate fragment control\n");unsigned before=received;send(sender,na,&source,900);REQUIRE(count>3);deliver(nb,0);deliver(nb,1);deliver(nb,1);for(unsigned i=2;i<count;i++)deliver(nb,i);REQUIRE(received==before+1);
 /* FRAG1 arrives, then a later complete FRAGN precedes its intermediate peers. */
 fprintf(stderr,"valid reordered fragment control\n");before=received;send(sender,na,&source,900);REQUIRE(count>3);deliver(nb,0);deliver(nb,count-1);for(unsigned i=1;i<count-1;i++)deliver(nb,i);REQUIRE(received==before+1);
 /* Loss followed by original two-tick expiry does not retain reassembly. */
 before=received;send(sender,na,&source,900);deliver(nb,0);lowpan6_tmr();lowpan6_tmr();for(unsigned i=1;i<count;i++)deliver(nb,i);REQUIRE(received==before);
 udp_remove(sender);udp_remove(receiver);REQUIRE(lwip_owned_lowpan_stop(a)==0&&lwip_owned_lowpan_free(a)==0&&lwip_owned_lowpan_stop(b)==0&&lwip_owned_lowpan_free(b)==0);
 REQUIRE(lwip_stats.mem.used==memory&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers);
}
int main(int argc,char**argv)
{
 if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_host_initialize(ready,NULL);while(!atomic_load(&initialized)){}REQUIRE(lwip_host_call(run,NULL)==0);REQUIRE(lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 puts("{\"radio_output_sizes\":4,\"valid_fcs\":true,\"compression\":true,\"fragmentation_reassembly\":true,\"ipv6_mtu_boundary\":true,\"valid_duplicate\":true,\"valid_reorder\":true,\"expiry\":true,\"baseline_restored\":true}");return 0;
}
