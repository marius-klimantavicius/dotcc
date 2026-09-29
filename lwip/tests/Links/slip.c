#include "lwip/init.h"
#include "lwip/mem.h"
#include "lwip/pbuf.h"
#include "lwip/sio.h"
#include "lwip/udp.h"
#include "lwip/ip.h"
#include "lwip/stats.h"
#include "slipif.c"
#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define REQUIRE(x) do {if(!(x)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x);exit(92);}}while(0)
static pthread_mutex_t gate=PTHREAD_MUTEX_INITIALIZER;
static pthread_cond_t changed=PTHREAD_COND_INITIALIZER;
static unsigned char incoming[4096],outgoing[4096],received[4096];
static size_t head,count,outcount,received_length;
static unsigned packets;
static int stopped;
extern void native_join_workers(void);
sio_fd_t sio_open(u8_t device){return device==7?(void*)1:NULL;}
void sio_send(u8_t value,sio_fd_t fd){REQUIRE(fd==(void*)1&&outcount<sizeof(outgoing));outgoing[outcount++]=value;}
u32_t sio_tryread(sio_fd_t fd,u8_t*bytes,u32_t length){REQUIRE(fd==(void*)1);pthread_mutex_lock(&gate);u32_t n=0;while(n<length&&count){bytes[n++]=incoming[head++];count--;}if(!count)head=0;pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);return n;}
u32_t sio_read(sio_fd_t fd,u8_t*bytes,u32_t length){REQUIRE(fd==(void*)1);pthread_mutex_lock(&gate);while(!count&&!stopped)pthread_cond_wait(&changed,&gate);if(stopped){pthread_mutex_unlock(&gate);pthread_exit(NULL);}u32_t n=0;while(n<length&&count){bytes[n++]=incoming[head++];count--;}if(!count)head=0;pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);return n;}
static void feed(const unsigned char*bytes,size_t length){pthread_mutex_lock(&gate);REQUIRE(head+count+length<=sizeof(incoming));memcpy(incoming+head+count,bytes,length);count+=length;pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);}
static err_t input(struct pbuf*p,struct netif*n){(void)n;pthread_mutex_lock(&gate);REQUIRE(p->tot_len<=sizeof(received));received_length=pbuf_copy_partial(p,received,p->tot_len,0);packets++;pbuf_free(p);pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);return ERR_OK;}
static void await_packets(struct netif*n,unsigned expected){
#if SLIP_USE_RX_THREAD
 struct timespec until;clock_gettime(CLOCK_REALTIME,&until);until.tv_sec+=3;pthread_mutex_lock(&gate);while(packets<expected)REQUIRE(pthread_cond_timedwait(&changed,&gate,&until)==0);pthread_mutex_unlock(&gate);
#else
 slipif_poll(n);
#endif
 REQUIRE(packets==expected);
}
static size_t encode(const unsigned char*bytes,size_t length,unsigned char*out){size_t n=0;out[n++]=0xc0;for(size_t i=0;i<length;i++){if(bytes[i]==0xc0){out[n++]=0xdb;out[n++]=0xdc;}else if(bytes[i]==0xdb){out[n++]=0xdb;out[n++]=0xdd;}else out[n++]=bytes[i];}out[n++]=0xc0;return n;}
static unsigned datagrams;
static void datagram(void*arg,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*from,u16_t port){(void)arg;(void)pcb;REQUIRE(port==9001);unsigned char bytes[73];REQUIRE(p->tot_len==73&&pbuf_copy_partial(p,bytes,73,0)==73);for(unsigned i=0;i<73;i++)REQUIRE(bytes[i]==(unsigned char)(i*7));REQUIRE(IP_IS_V6(from)==(datagrams==1));pbuf_free(p);pthread_mutex_lock(&gate);datagrams++;pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);}
static unsigned checksum(const unsigned char*p,size_t n,unsigned sum){for(size_t i=0;i<n;i+=2)sum+=(unsigned)p[i]*256+(i+1<n?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return sum;}
static void ip_case(struct netif*n,int v6){unsigned char packet[121]={0},wire[250];size_t offset=v6?40:20,total=offset+81;if(v6){packet[0]=0x60;packet[5]=81;packet[6]=17;packet[7]=64;packet[8]=packet[24]=0xfd;packet[23]=2;packet[39]=1;}else{packet[0]=0x45;packet[3]=(unsigned char)total;packet[8]=64;packet[9]=17;packet[12]=packet[16]=10;packet[13]=packet[17]=1;packet[15]=2;packet[19]=1;unsigned c=~checksum(packet,20,0);packet[10]=c>>8;packet[11]=c;}
 packet[offset]=0x23;packet[offset+1]=0x29;packet[offset+2]=0x23;packet[offset+3]=0x28;packet[offset+5]=81;for(unsigned i=0;i<73;i++)packet[offset+8+i]=(unsigned char)(i*7);
 if(v6){unsigned c=~checksum(packet+40,81,checksum(packet+8,32,81+17));packet[46]=c>>8;packet[47]=c;}
 size_t length=encode(packet,total,wire);feed(wire,length);
#if SLIP_USE_RX_THREAD
 struct timespec until;clock_gettime(CLOCK_REALTIME,&until);until.tv_sec+=3;pthread_mutex_lock(&gate);while(datagrams<(unsigned)(v6+1))REQUIRE(pthread_cond_timedwait(&changed,&gate,&until)==0);pthread_mutex_unlock(&gate);
#else
 slipif_poll(n);
#endif
 REQUIRE(datagrams==(unsigned)(v6+1));}
int main(int argc,char**argv){if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_init();unsigned baseline=lwip_stats.mem.used,pool=lwip_stats.memp[MEMP_PBUF_POOL]->used;struct netif n={0};ip4_addr_t address,mask,gateway;IP4_ADDR(&address,10,1,0,1);IP4_ADDR(&mask,255,255,255,0);ip4_addr_set_zero(&gateway);REQUIRE(netif_add(&n,&address,&mask,&gateway,(void*)7,slipif_init,input)==&n&&n.mtu==1500);netif_set_up(&n);netif_set_link_up(&n);
 unsigned char payload[300],expected[610];for(unsigned i=0;i<sizeof(payload);i++)payload[i]=(unsigned char)i;
 struct pbuf*a=pbuf_alloc(PBUF_RAW,129,PBUF_RAM),*b=pbuf_alloc(PBUF_RAW,171,PBUF_RAM);REQUIRE(a&&b);REQUIRE(pbuf_take(a,payload,129)==0&&pbuf_take(b,payload+129,171)==0);pbuf_cat(a,b);
 REQUIRE(n.output(&n,a,NULL)==0);pbuf_free(a);size_t wire=encode(payload,sizeof(payload),expected);REQUIRE(outcount==wire&&!memcmp(outgoing,expected,wire));
 /* Escape pair is split across input calls, and no final END means no delivery. */
 size_t split=0;while(split<wire&&expected[split]!=0xdb)split++;REQUIRE(split<wire);feed(expected,split+1);
#if !SLIP_USE_RX_THREAD
 slipif_poll(&n);REQUIRE(packets==0);
#endif
 feed(expected+split+1,wire-split-2);
#if !SLIP_USE_RX_THREAD
 slipif_poll(&n);REQUIRE(packets==0);
#endif
 feed(expected+wire-1,1);await_packets(&n,1);REQUIRE(received_length==sizeof(payload)&&!memcmp(received,payload,sizeof(payload)));
 unsigned char unusual[]={0xc0,0xc0,0xdb,0x01,0xc0};feed(unusual,sizeof(unusual));await_packets(&n,2);REQUIRE(received_length==1&&received[0]==1); /* upstream tolerates unknown escape */
 outcount=0;a=pbuf_alloc(PBUF_RAW,3,PBUF_RAM);REQUIRE(a&&pbuf_take(a,"abc",3)==0);REQUIRE(n.output_ip6(&n,a,NULL)==0);pbuf_free(a);REQUIRE(outcount==5&&!memcmp(outgoing,"\300abc\300",5));
 n.input=ip_input;ip6_addr_t address6;REQUIRE(ip6addr_aton("fd00::1",&address6));netif_ip6_addr_set(&n,0,&address6);netif_ip6_addr_set_state(&n,0,IP6_ADDR_PREFERRED);struct udp_pcb*udp=udp_new_ip_type(IPADDR_TYPE_ANY);REQUIRE(udp&&udp_bind(udp,IP_ANY_TYPE,9000)==0);udp_recv(udp,datagram,NULL);ip_case(&n,0);ip_case(&n,1);udp_remove(udp);
 /* Leave a live incomplete decoder frame, then stop at the serial boundary. */
 unsigned char partial[]={1,2,3,0xdb};feed(partial,sizeof(partial));
#if SLIP_USE_RX_THREAD
 pthread_mutex_lock(&gate);while(count)pthread_cond_wait(&changed,&gate);pthread_mutex_unlock(&gate);
#else
 slipif_poll(&n);
#endif
#if SLIP_USE_RX_THREAD
 pthread_mutex_lock(&gate);stopped=1;pthread_cond_broadcast(&changed);pthread_mutex_unlock(&gate);native_join_workers();
#endif
 struct slipif_priv*priv=n.state;REQUIRE(priv->q!=NULL);pbuf_free(priv->q);priv->q=priv->p=NULL;netif_remove(&n);mem_free(n.state);REQUIRE(lwip_stats.mem.used==baseline&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool);puts("{\"slip_frames\":2,\"output_families\":2,\"payload_bytes\":300,\"unknown_escape_preserved\":true,\"ip_datagrams\":2,\"decoder_pbufs_released\":true}");return 0;}
