#include "lwip/init.h"
#include "lwip/timeouts.h"
#include "lwip/stats.h"
#include "lwip/udp.h"
#include "lwip/tcp.h"
#include "netif/ppp/pppos.h"
#include "netif/ppp/ppp_impl.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define REQUIRE(x) do{if(!(x)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);
extern int lwip_host_call(void(*)(void*),void*);
extern void lwip_host_poll(void);
extern int lwip_host_stop_worker(void);
extern void native_join_workers(void);
static atomic_uint clock_ms;
u32_t sys_now(void){return atomic_load(&clock_ms);}
u32_t sys_jiffies(void){return sys_now();}
typedef struct{ppp_pcb*pcb;struct netif netif;int index,up,error;} peer;
typedef struct{int target;unsigned length;unsigned char data[2048];} chunk;
typedef struct{int opened,dead,up[2],error[2];unsigned received;} snapshot;
typedef struct{u8_t auth;int wrong,mppe;} settings;
static peer peers[2];static chunk chunks[2048];static unsigned head,count,bytes,received;
static unsigned memory,pool,timeouts;
static unsigned wire_mppe,wire_vj_full,wire_vj_compressed,wire_ip,wire_lqr_ack;
static unsigned frame_len[2],frame_escape[2];static unsigned char frames[2][4096];
static unsigned tcp_received,tcp_connected;static struct tcp_pcb*tcp_sender,*tcp_receiver,*tcp_listener;
/* Independent RFC1662 byte unescaping/FCS and RFC1332 protocol observation. */
static void observe(int index,const unsigned char*data,unsigned length)
{
 for(unsigned j=0;j<length;j++){
  unsigned char ch=data[j];
  if(ch==0x7e){
   unsigned n=frame_len[index];unsigned char*f=frames[index];
   if(n){unsigned fcs=0xffff;for(unsigned k=0;k<n;k++){fcs^=f[k];for(int bit=0;bit<8;bit++)fcs=(fcs>>1)^((fcs&1)?0x8408:0);}REQUIRE(fcs==0xf0b8);
    unsigned pos=n>2&&f[0]==0xff&&f[1]==3?2:0;unsigned protocol=f[pos++];if(!(protocol&1))protocol=(protocol<<8)|f[pos++];
    if(protocol==0xc021&&f[pos]==2){for(unsigned k=pos+4;k+2<=n-2;){unsigned length=f[k+1];REQUIRE(length>=2&&k+length<=n-2);if(f[k]==4){REQUIRE(length==8&&f[k+2]==0xc0&&f[k+3]==0x25&&f[k+7]==100);wire_lqr_ack++;}k+=length;}}
    wire_mppe+=protocol==0xfd;wire_vj_full+=protocol==0x2f;wire_vj_compressed+=protocol==0x2d;wire_ip+=protocol==0x21;
    if(protocol==0xfd){unsigned char plain[73];for(unsigned k=0;k<73;k++)plain[k]=(unsigned char)(k*7);for(unsigned k=pos;k+73<=n-2;k++)REQUIRE(memcmp(f+k,plain,73));}
   }
   frame_len[index]=frame_escape[index]=0;
  }else if(ch==0x7d)frame_escape[index]=1;
  else{if(frame_escape[index]){ch^=0x20;frame_escape[index]=0;}REQUIRE(frame_len[index]<4096);frames[index][frame_len[index]++]=ch;}
 }
}
static atomic_int initialized;
static void ready(void*arg){(void)arg;atomic_store(&initialized,1);}
static u32_t output(ppp_pcb*pcb,u8_t*data,u32_t length,void*arg)
{
 peer*p=arg;REQUIRE(pcb==p->pcb&&length<=2048&&count<2048);
 observe(p->index,data,length);
 chunk*c=&chunks[(head+count++)%2048];c->target=1-p->index;c->length=length;memcpy(c->data,data,length);bytes+=length;return length;
}
static void status(ppp_pcb*pcb,int code,void*arg){peer*p=arg;REQUIRE(pcb==p->pcb);if(code==0)p->up++;else p->error=code;}
static void sample(snapshot*s)
{
 s->opened=s->dead=1;s->received=received;
 for(int i=0;i<2;i++){peer*p=&peers[i];s->up[i]=p->up;s->error[i]=p->error;s->opened&=p->pcb->ipcp_fsm.state==PPP_FSM_OPENED&&p->pcb->ipv6cp_fsm.state==PPP_FSM_OPENED;s->dead&=p->pcb->phase==PPP_PHASE_DEAD;}
}
static void step(void*arg)
{
 #if NO_SYS
 netif_poll_all();
 #endif
 sys_check_timeouts();
 unsigned limit=4096;
 while(count&&limit--){chunk c=chunks[head];head=(head+1)%2048;count--;for(unsigned i=0;i<c.length;){unsigned length=1+i%7;if(length>c.length-i)length=c.length-i;pppos_input(peers[c.target].pcb,c.data+i,(int)length);i+=length;}}
 REQUIRE(limit);if(arg)sample(arg);
}
static void pump(snapshot*s)
{
 atomic_fetch_add(&clock_ms,50);lwip_host_poll();REQUIRE(lwip_host_call(step,s)==0);
}
static void begin(void*arg)
{
 settings*options=arg;memset(peers,0,sizeof(peers));head=count=bytes=received=0;
 wire_lqr_ack=wire_mppe=wire_vj_full=wire_vj_compressed=wire_ip=tcp_received=tcp_connected=0;memset(frame_len,0,sizeof(frame_len));memset(frame_escape,0,sizeof(frame_escape));
 memory=lwip_stats.mem.used;pool=lwip_stats.memp[MEMP_PBUF_POOL]->used;timeouts=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;
 for(int i=0;i<2;i++){
  peer*p=&peers[i];p->index=i;p->pcb=pppos_create(&p->netif,output,status,p);REQUIRE(p->pcb);
  ppp_set_auth(p->pcb,options->auth,"user",options->wrong&&i==0?"wrong":"password");
  ip4_addr_t address,remote;IP4_ADDR(&address,10,4,0,i+1);IP4_ADDR(&remote,10,4,0,2-i);ppp_set_ipcp_ouraddr(p->pcb,&address);ppp_set_ipcp_hisaddr(p->pcb,&remote);
#if LQR_SUPPORT
  p->pcb->lcp_wantoptions.neg_lqr=p->pcb->lcp_allowoptions.neg_lqr=1;p->pcb->lcp_wantoptions.lqr_period=p->pcb->lcp_allowoptions.lqr_period=100;
#endif
  if(options->mppe)ppp_set_mppe(p->pcb,PPP_MPPE_ENABLE|PPP_MPPE_REFUSE_40);
 }
 ppp_set_auth_required(peers[1].pcb,1);REQUIRE(ppp_listen(peers[1].pcb)==0&&ppp_connect(peers[0].pcb,0)==0);
}
static void datagram(void*arg,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*address,u16_t port)
{
 (void)arg;(void)pcb;REQUIRE(IP_IS_V4(address)&&port==9001&&p->tot_len==73);unsigned char payload[73];REQUIRE(pbuf_copy_partial(p,payload,73,0)==73);
 for(unsigned i=0;i<73;i++)REQUIRE(payload[i]==(unsigned char)(i*7));pbuf_free(p);received++;
}
static struct udp_pcb*sender,*receiver;
static void send_data(void*arg)
{
 settings*options=arg;if(options->mppe)REQUIRE(peers[0].pcb->ccp_gotoptions.mppe&&peers[1].pcb->ccp_gotoptions.mppe);
 ip_addr_t local,remote;REQUIRE(ipaddr_aton("10.4.0.1",&local)&&ipaddr_aton("10.4.0.2",&remote));
 receiver=udp_new();sender=udp_new();REQUIRE(receiver&&sender&&udp_bind(receiver,&remote,9000)==0&&udp_bind(sender,&local,9001)==0);udp_recv(receiver,datagram,NULL);
 struct pbuf*p=pbuf_alloc(PBUF_TRANSPORT,73,PBUF_RAM);REQUIRE(p);unsigned char payload[73];for(unsigned i=0;i<73;i++)payload[i]=(unsigned char)(i*7);REQUIRE(pbuf_take(p,payload,73)==0);
 REQUIRE(udp_sendto_if_src(sender,p,&remote,9000,&peers[0].netif,&local)==0);pbuf_free(p);
}
static err_t tcp_data(void*arg,struct tcp_pcb*pcb,struct pbuf*p,err_t error)
{
 (void)arg;REQUIRE(error==0);if(!p)return ERR_OK;unsigned char actual[73];REQUIRE(p->tot_len==73&&pbuf_copy_partial(p,actual,73,0)==73);for(unsigned i=0;i<73;i++)REQUIRE(actual[i]==(unsigned char)(i*7));tcp_recved(pcb,73);pbuf_free(p);tcp_received++;return ERR_OK;
}
static err_t accepted(void*arg,struct tcp_pcb*pcb,err_t error)
{(void)arg;REQUIRE(error==0&&tcp_receiver==NULL);tcp_receiver=pcb;tcp_bind_netif(pcb,&peers[1].netif);tcp_recv(pcb,tcp_data);return ERR_OK;}
static err_t connected(void*arg,struct tcp_pcb*pcb,err_t error)
{(void)arg;REQUIRE(error==0&&pcb==tcp_sender);tcp_connected=1;return ERR_OK;}
static void tcp_begin(void*arg)
{
 (void)arg;REQUIRE(peers[0].pcb->vj_enabled&&peers[1].pcb->vj_enabled);ip_addr_t local,remote;REQUIRE(ipaddr_aton("10.4.0.1",&local)&&ipaddr_aton("10.4.0.2",&remote));
 tcp_sender=tcp_new();struct tcp_pcb*l=tcp_new();REQUIRE(tcp_sender&&l&&tcp_bind(l,&remote,9002)==0&&tcp_bind(tcp_sender,&local,9003)==0);tcp_bind_netif(tcp_sender,&peers[0].netif);tcp_bind_netif(l,&peers[1].netif);tcp_listener=tcp_listen(l);REQUIRE(tcp_listener);tcp_accept(tcp_listener,accepted);REQUIRE(tcp_connect(tcp_sender,&remote,9002,connected)==0);
}
static void tcp_send(void*arg)
{(void)arg;unsigned char payload[73];for(unsigned i=0;i<73;i++)payload[i]=(unsigned char)(i*7);REQUIRE(tcp_write(tcp_sender,payload,73,TCP_WRITE_FLAG_COPY)==0&&tcp_output(tcp_sender)==0);}
static void tcp_sample(void*arg){unsigned*s=arg;s[0]=tcp_connected;s[1]=tcp_received;}
static void wire_check(void*arg)
{settings*options=arg;
#if LQR_SUPPORT
 REQUIRE(wire_lqr_ack>=2&&peers[0].pcb->lcp_gotoptions.neg_lqr&&peers[1].pcb->lcp_gotoptions.neg_lqr);
#endif
 if(options->mppe)REQUIRE(wire_mppe>=3&&wire_ip==0);else REQUIRE(wire_vj_full&&wire_vj_compressed);}
static void close_link(void*arg){(void)arg;if(tcp_sender){tcp_abort(tcp_sender);tcp_sender=NULL;}if(tcp_receiver){tcp_abort(tcp_receiver);tcp_receiver=NULL;}if(tcp_listener){REQUIRE(tcp_close(tcp_listener)==0);tcp_listener=NULL;}if(sender){udp_remove(sender);sender=NULL;}if(receiver){udp_remove(receiver);receiver=NULL;}REQUIRE(ppp_close(peers[0].pcb,0)==0);}
static void release(void*arg)
{
 (void)arg;for(int i=0;i<2;i++)REQUIRE(ppp_free(peers[i].pcb)==0);
 fprintf(stderr,"cleanup memory=%u baseline=%u pool=%u baseline=%u timeouts=%u baseline=%u queue=%u\n",(unsigned)lwip_stats.mem.used,memory,(unsigned)lwip_stats.memp[MEMP_PBUF_POOL]->used,pool,(unsigned)lwip_stats.memp[MEMP_SYS_TIMEOUT]->used,timeouts,count);
 REQUIRE(lwip_stats.mem.used==memory&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timeouts);
}
static void one(u8_t auth,int wrong,int mppe)
{
 settings options={auth,wrong,mppe};REQUIRE(lwip_host_call(begin,&options)==0);snapshot state={0};u32_t until=sys_now()+10000;
 do{REQUIRE((int)(until-sys_now())>0);pump(&state);}while(!state.opened&&!state.error[0]&&!state.error[1]);
 if(wrong)REQUIRE(state.up[0]==0&&state.up[1]==0&&(state.error[0]==PPPERR_AUTHFAIL||state.error[1]==PPPERR_AUTHFAIL));
 else{
  REQUIRE(state.opened&&state.up[0]==2&&state.up[1]==2);REQUIRE(lwip_host_call(send_data,&options)==0);until=sys_now()+10000;
  do{REQUIRE((int)(until-sys_now())>0);pump(&state);}while(!state.received);REQUIRE(state.received==1&&state.opened);
  REQUIRE(lwip_host_call(tcp_begin,NULL)==0);unsigned tcp_state[2]={0};until=sys_now()+10000;
  do{REQUIRE((int)(until-sys_now())>0);pump(&state);REQUIRE(lwip_host_call(tcp_sample,tcp_state)==0);}while(!tcp_state[0]);
  for(unsigned packet=1;packet<=3;packet++){REQUIRE(lwip_host_call(tcp_send,NULL)==0);until=sys_now()+10000;do{REQUIRE((int)(until-sys_now())>0);pump(&state);REQUIRE(lwip_host_call(tcp_sample,tcp_state)==0);}while(tcp_state[1]<packet);}
  REQUIRE(lwip_host_call(wire_check,&options)==0);
 }
 REQUIRE(lwip_host_call(close_link,NULL)==0);until=sys_now()+10000;
 do{REQUIRE((int)(until-sys_now())>0);pump(&state);}while(!state.dead);
 lwip_host_poll();REQUIRE(lwip_host_call(release,NULL)==0);
 printf("{\"auth\":%u,\"wrong\":%d,\"mppe\":%d,\"bytes\":%u,\"datagrams\":%u,\"tcp_packets\":%u,\"mppe_frames\":%u,\"vj_full\":%u,\"vj_compressed\":%u}\n",auth,wrong,mppe,bytes,received,tcp_received,wire_mppe,wire_vj_full,wire_vj_compressed);
}
int main(int argc,char**argv)
{
 if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_host_initialize(ready,NULL);while(!atomic_load(&initialized)){}
 for(unsigned type=argc>1&&!strcmp(argv[1],"--eap-only")?16:1;type<=16;type*=2){one((u8_t)type,0,0);one((u8_t)type,1,0);}one(PPPAUTHTYPE_MSCHAP_V2,0,1);
 REQUIRE(lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 puts("{\"serial_negotiations\":6,\"wrong_credentials\":5,\"ipv4_ipv6_control\":true,\"datagrams\":6,\"tcp_payloads\":18,\"vj_negotiated\":true,\"mppe_wire_private\":true,\"closed_and_freed\":true}");return 0;
}
