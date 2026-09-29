/* Independent RFC6282/RFC4944 receive vectors; no radio hardware. */
#include "lwip/init.h"
#include "lwip/stats.h"
#include "lwip/udp.h"
#include "netif/lowpan6.h"
#include "netif/lowpan6_ble.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define REQUIRE(x) do{if(!(x)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);
extern int lwip_host_call(void(*)(void*),void*);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
static atomic_int initialized;static unsigned received;static int short_only;
static void ready(void*a){(void)a;atomic_store(&initialized,1);}
static void put16(unsigned char*p,unsigned value){p[0]=(unsigned char)(value>>8);p[1]=(unsigned char)value;}
static unsigned sum(const unsigned char*p,unsigned length,unsigned total)
{for(unsigned i=0;i<length;i+=2)total+=(p[i]<<8)|(i+1<length?p[i+1]:0);while(total>>16)total=(total&65535)+(total>>16);return total;}
static void datagram(void*a,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*address,u16_t port)
{(void)a;(void)pcb;REQUIRE(IP_IS_V6(address)&&port==9001&&p->tot_len==17);unsigned char bytes[17];REQUIRE(pbuf_copy_partial(p,bytes,17,0)==17);for(unsigned i=0;i<17;i++)REQUIRE(bytes[i]==(unsigned char)(i*7));pbuf_free(p);received++;}
static void inject(struct netif*n,int ble,int compressed,int mode)
{
 unsigned char ip[65]={0};ip[0]=0x60;put16(ip+4,25);ip[6]=17;ip[7]=64;ip[8]=ip[24]=0xfd;ip[23]=2;ip[39]=1;put16(ip+40,9001);put16(ip+42,9000);put16(ip+44,25);for(unsigned i=0;i<17;i++)ip[48+i]=(unsigned char)(i*7);put16(ip+46,~sum(ip+40,25,sum(ip+8,32,25+17)));
 unsigned char frame[128]={0};unsigned at=0;
 if(!ble){int short_source=mode&1,extended_destination=mode&2;frame[0]=0x41;frame[1]=(unsigned char)((short_source?0x80:0xc0)|(extended_destination?0x0c:0x08));frame[2]=7;frame[3]=0x34;frame[4]=0x12;at=5;frame[at]=1;at+=extended_destination?8:2;frame[at]=2;if(!short_source)frame[at+7]=2;at+=short_source?2:8;}
 if(compressed){frame[at++]=0x7a;frame[at++]=0;frame[at++]=17;memcpy(frame+at,ip+8,32);at+=32;memcpy(frame+at,ip+40,25);at+=25;}
 else{frame[at++]=0x41;memcpy(frame+at,ip,sizeof(ip));at+=sizeof(ip);}
 REQUIRE(at<=127);struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)at,PBUF_RAM);REQUIRE(p&&pbuf_take(p,frame,(u16_t)at)==0);REQUIRE((ble?rfc7668_input(p,n):lowpan6_input(p,n))==0);
}

#include "lowpan_service.h"
/* This synchronous fixture has no queued callbacks; the public owner uses
 * the product FIFO barrier outside core between these two phases. */
static int close_service(void*handle){int result=lwip_owned_lowpan_stop(handle);return result?result:lwip_owned_lowpan_free(handle);}
static void fragment(struct netif*n,int second,unsigned tag)
{
 unsigned char ip[65]={0};ip[0]=0x60;put16(ip+4,25);ip[6]=17;ip[7]=64;ip[8]=ip[24]=0xfd;ip[23]=2;ip[39]=1;put16(ip+40,9001);put16(ip+42,9000);put16(ip+44,25);for(unsigned i=0;i<17;i++)ip[48+i]=(unsigned char)(i*7);put16(ip+46,~sum(ip+40,25,sum(ip+8,32,42)));
 unsigned char f[100]={0};f[0]=0x41;f[1]=0xc8;f[2]=7;f[3]=0x34;f[4]=0x12;f[5]=1;f[7]=2;f[14]=2;f[15]=second?0xe0:0xc0;f[16]=65;put16(f+17,tag);unsigned length;
 if(second){f[19]=6;memcpy(f+20,ip+48,17);length=37;}else{f[19]=0x41;memcpy(f+20,ip,48);length=68;}
 struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)length,PBUF_RAM);REQUIRE(p&&pbuf_take(p,f,(u16_t)length)==0);REQUIRE(lowpan6_input(p,n)==0);
}
static err_t outer_output(struct netif*n,struct pbuf*p,const ip4_addr_t*a){(void)n;(void)p;(void)a;return ERR_OK;}
static err_t outer_init(struct netif*n){n->output=outer_output;n->mtu=1500;return ERR_OK;}
/* Positive IEEE802.15.4 reflected-CCITT FCS, init0; emitted little-endian.
 * ZEP retains the FCS trailer; direct lowpan_input vectors omit it by contract. */
static unsigned radio_crc(const unsigned char*bytes,unsigned count)
{unsigned crc=0;for(unsigned i=0;i<count;i++){crc^=bytes[i];for(unsigned bit=0;bit<8;bit++)crc=(crc>>1)^((crc&1)?0x8408:0);}return crc;}
static void zep_input(struct netif*outer,unsigned port)
{
 unsigned char frame[143]={0};frame[0]=0x45;put16(frame+2,sizeof(frame));frame[8]=64;frame[9]=17;frame[12]=frame[16]=10;frame[13]=frame[17]=1;frame[15]=2;frame[19]=1;put16(frame+10,~sum(frame,20,0));put16(frame+20,17754);put16(frame+22,port);put16(frame+24,sizeof(frame)-20);
 unsigned char*z=frame+28;z[0]='E';z[1]='X';z[2]=2;z[3]=1;z[7]=1;z[31]=83;
 unsigned char*r=z+32;r[0]=0x41;r[1]=0xc8;r[2]=7;r[3]=0x34;r[4]=0x12;r[5]=1;r[7]=2;r[14]=2;r[15]=0x41;
 unsigned char*ip=r+16;ip[0]=0x60;put16(ip+4,25);ip[6]=17;ip[7]=64;ip[8]=ip[24]=0xfd;ip[23]=2;ip[39]=1;put16(ip+40,9001);put16(ip+42,9000);put16(ip+44,25);for(unsigned i=0;i<17;i++)ip[48+i]=(unsigned char)(i*7);put16(ip+46,~sum(ip+40,25,sum(ip+8,32,42)));
 unsigned crc=radio_crc(r,81);r[81]=(unsigned char)crc;r[82]=(unsigned char)(crc>>8);
 struct pbuf*p=pbuf_alloc(PBUF_RAW,sizeof(frame),PBUF_RAM);REQUIRE(p&&pbuf_take(p,frame,sizeof(frame))==0);REQUIRE(ip_input(p,outer)==0);
}
static void configure(void*handle)
{struct netif*n=lwip_owned_lowpan_netif(handle);ip6_addr_t address;REQUIRE(n&&ip6addr_aton("fd00::1",&address));netif_ip6_addr_set(n,0,&address);netif_ip6_addr_set_state(n,0,IP6_ADDR_PREFERRED);netif_set_up(n);netif_set_link_up(n);}
static void run(void*argument)
{
 (void)argument;unsigned memory=lwip_stats.mem.used,pool=lwip_stats.memp[MEMP_PBUF_POOL]->used,timers=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;
 struct netif outer={0};ip4_addr_t local,mask,zero={0};REQUIRE(ip4addr_aton("10.1.0.1",&local)&&ip4addr_aton("255.255.255.0",&mask));REQUIRE(netif_add(&outer,&local,&mask,&zero,NULL,outer_init,ip_input));netif_set_up(&outer);netif_set_link_up(&outer);
 ip_addr_t any;ip_addr_set_any(IPADDR_TYPE_V6,&any);struct udp_pcb*udp=udp_new_ip_type(IPADDR_TYPE_V6);REQUIRE(udp&&udp_bind(udp,&any,9000)==0);udp_recv(udp,datagram,NULL);
 struct zepif_init init={0};init.zep_src_udp_port=17754;init.zep_netif=&outer;void*a=lwip_owned_lowpan_new(1,&init);REQUIRE(a);configure(a);REQUIRE(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers+1);REQUIRE(!lwip_owned_lowpan_new(1,&init));REQUIRE(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers+1);
 init.zep_src_udp_port=17755;void*b=lwip_owned_lowpan_new(1,&init),*radio=lwip_owned_lowpan_new(0,NULL);REQUIRE(b&&radio);configure(b);configure(radio);REQUIRE(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers+1);
 zep_input(&outer,17754);REQUIRE(received==1);zep_input(&outer,17755);REQUIRE(received==2);
 REQUIRE(close_service(a)==0&&close_service(a)==ERR_ARG);REQUIRE(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers+1);zep_input(&outer,17755);REQUIRE(received==3);fragment(lwip_owned_lowpan_netif(radio),0,0x1234);REQUIRE(received==3);REQUIRE(close_service(b)==0);REQUIRE(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers+1);lowpan6_tmr();fragment(lwip_owned_lowpan_netif(radio),1,0x1234);REQUIRE(received==4);inject(lwip_owned_lowpan_netif(radio),0,1,1);REQUIRE(received==5);fragment(lwip_owned_lowpan_netif(radio),0,0x1235);REQUIRE(close_service(radio)==0&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers);
 void*lease=lwip_owned_lowpan_new(2,NULL);REQUIRE(lease&&lwip_owned_lowpan_netif(lease)==NULL);REQUIRE(close_service(lease)==0);a=lwip_owned_lowpan_new(1,&init);REQUIRE(a);configure(a);zep_input(&outer,17755);REQUIRE(received==6);REQUIRE(close_service(a)==0);
 void*ble=lwip_owned_lowpan_new(3,NULL);REQUIRE(ble);configure(ble);REQUIRE(lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers);inject(lwip_owned_lowpan_netif(ble),1,1,0);REQUIRE(received==7);REQUIRE(close_service(ble)==0);
 udp_remove(udp);netif_set_down(&outer);netif_remove(&outer);REQUIRE(lwip_stats.mem.used==memory&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers);
}

#include "bridge_service.h"
static void *fifo_handle;static unsigned fifo_memory,fifo_pool,fifo_timers;
static void fifo_setup(void*argument)
{
 (void)argument;fifo_memory=lwip_stats.mem.used;fifo_pool=lwip_stats.memp[MEMP_PBUF_POOL]->used;fifo_timers=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;
 fifo_handle=lwip_owned_lowpan_new(0,NULL);REQUIRE(fifo_handle);configure(fifo_handle);struct netif*n=lwip_owned_lowpan_netif(fifo_handle);
 unsigned char bytes[40]={0};bytes[0]=0x60;bytes[6]=59;bytes[7]=64;bytes[8]=bytes[24]=0xfd;bytes[23]=2;bytes[39]=1;
 struct pbuf*p=pbuf_alloc(PBUF_RAW,sizeof(bytes),PBUF_RAM);REQUIRE(p&&pbuf_take(p,bytes,sizeof(bytes))==0);REQUIRE(netif_loop_output(n,p)==0);pbuf_free(p);REQUIRE(n->loop_first!=NULL);
}
static void fifo_stop(void*argument)
{
 (void)argument;struct netif*n=lwip_owned_lowpan_netif(fifo_handle);REQUIRE(n);REQUIRE(lwip_owned_lowpan_free(fifo_handle)==ERR_USE);REQUIRE(lwip_owned_lowpan_stop(fifo_handle)==0);REQUIRE(n->loop_first==NULL&&n->state==NULL);REQUIRE(lwip_owned_lowpan_netif(fifo_handle)==NULL);
}
static void fifo_free(void*argument)
{
 (void)argument;REQUIRE(lwip_owned_lowpan_free(fifo_handle)==0);REQUIRE(lwip_stats.mem.used==fifo_memory&&lwip_stats.memp[MEMP_PBUF_POOL]->used==fifo_pool&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==fifo_timers);
}
int main(int argc,char**argv){if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_host_initialize(ready,NULL);while(!atomic_load(&initialized)){}REQUIRE(lwip_host_call(run,NULL)==0);REQUIRE(lwip_host_call(fifo_setup,NULL)==0);REQUIRE(lwip_owned_bridge_barrier()==0);REQUIRE(lwip_host_call(fifo_stop,NULL)==0);REQUIRE(lwip_owned_bridge_barrier()==0);REQUIRE(lwip_host_call(fifo_free,NULL)==0);REQUIRE(lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 puts("{\"zep_inputs\":4,\"valid_radio_fcs\":true,\"ble_input\":true,\"direct_radio_survives\":true,\"shared_timer\":true,\"fragment_survives_other_close\":true,\"pending_fragment_cleanup\":true,\"queued_loopback_fifo_stop\":true,\"partial_init_failure\":true,\"restart\":true,\"baseline_restored\":true}");return 0;}
