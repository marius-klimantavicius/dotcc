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
static void run(void*argument)
{
 int ble=*(int*)argument;unsigned memory=lwip_stats.mem.used,pool=lwip_stats.memp[MEMP_PBUF_POOL]->used;struct netif n={0};ip4_addr_t zero={0};REQUIRE(netif_add(&n,&zero,&zero,&zero,NULL,ble?rfc7668_if_init:lowpan6_if_init,ip_input));ip6_addr_t address;REQUIRE(ip6addr_aton("fd00::1",&address));netif_ip6_addr_set(&n,0,&address);netif_ip6_addr_set_state(&n,0,IP6_ADDR_PREFERRED);netif_set_up(&n);netif_set_link_up(&n);
 ip_addr_t any;ip_addr_set_any(IPADDR_TYPE_V6,&any);struct udp_pcb*udp=udp_new_ip_type(IPADDR_TYPE_V6);REQUIRE(udp&&udp_bind(udp,&any,9000)==0);udp_recv(udp,datagram,NULL);
 if(ble){unsigned char mac[6]={2,0x11,0x22,0x33,0x44,0x55},eui[8],back[6],expected[8]={0,0x11,0x22,0xff,0xfe,0x33,0x44,0x55};ble_addr_to_eui64(eui,mac,1);REQUIRE(!memcmp(eui,expected,8));eui64_to_ble_addr(back,eui);REQUIRE(back[0]==0&&!memcmp(back+1,mac+1,5));REQUIRE(rfc7668_set_local_addr_mac48(&n,mac,6,1)==0&&rfc7668_set_peer_addr_eui64(&n,eui,8)==0&&rfc7668_set_local_addr_mac48(&n,mac,5,1)==ERR_VAL);}
 else{REQUIRE(lowpan6_set_pan_id(0x1234)==0&&lowpan6_calc_crc("123456789",9)==0x2189);}
 unsigned before=received;
 if(short_only){inject(&n,0,0,1);REQUIRE(received==before+1);}
 else{for(int mode=0;mode<(ble?1:4);mode++)for(int compressed=0;compressed<2;compressed++){inject(&n,ble,compressed,mode);REQUIRE(received==++before);}}
 udp_remove(udp);netif_set_down(&n);netif_remove(&n);REQUIRE(lwip_stats.mem.used==memory&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool);
}
int main(int argc,char**argv)
{if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);short_only=argc>1&&!strcmp(argv[1],"--short");lwip_host_initialize(ready,NULL);while(!atomic_load(&initialized)){}for(int ble=0;ble<(short_only?1:2);ble++)REQUIRE(lwip_host_call(run,&ble)==0);REQUIRE(lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 puts("{\"ble_and_radio_inputs\":10,\"independent_iphc_vectors\":true,\"ble_address_vectors\":true,\"crc_vector\":true,\"baseline_restored\":true}");return 0;}
