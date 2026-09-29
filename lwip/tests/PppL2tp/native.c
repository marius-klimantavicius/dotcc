/* Independent RFC2661 control peer over original lwIP UDP/IP. */
#include "lwip/init.h"
#include "lwip/timeouts.h"
#include "lwip/stats.h"
#include "lwip/ip.h"
#include "lwip/udp.h"
#include "netif/ppp/pppol2tp.h"
#include "netif/ppp/ppp_impl.h"
#include "netif/ppp/polarssl/md5.h"
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
static void ready(void*a){(void)a;atomic_store(&initialized,1);}
static struct netif underlying,pppif;static ppp_pcb*session;
static unsigned char frames[256][2048];static unsigned sizes[256],count,remote_tunnel,remote_session,local_port;
static int status_error;static unsigned up,datagrams,packet_count;static unsigned char packets[256][2048];static unsigned packet_sizes[256];
static unsigned u16(const unsigned char*p){return (p[0]<<8)|p[1];}
static void put16(unsigned char*p,unsigned value){p[0]=(unsigned char)(value>>8);p[1]=(unsigned char)value;}
static unsigned checksum(const unsigned char*p,unsigned length)
{unsigned sum=0;for(unsigned i=0;i<length;i+=2)sum+=(p[i]<<8)|(i+1<length?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return (~sum)&65535;}
static err_t output(struct netif*n,struct pbuf*p,const ip4_addr_t*address)
{(void)address;REQUIRE(n==&underlying&&count<256&&p->tot_len<=2048);sizes[count]=p->tot_len;REQUIRE(pbuf_copy_partial(p,frames[count],p->tot_len,0)==p->tot_len);if(p->tot_len>=36&&u16(frames[count]+28)==2){unsigned offset=34;if(u16(frames[count]+offset)==0xff03)offset+=2;REQUIRE(packet_count<256);packet_sizes[packet_count]=p->tot_len-offset;memcpy(packets[packet_count++],frames[count]+offset,p->tot_len-offset);}count++;return ERR_OK;}
static err_t init(struct netif*n){n->mtu=1500;n->output=output;return ERR_OK;}
static void status(ppp_pcb*pcb,int error,void*argument){(void)argument;REQUIRE(pcb==session);status_error=error;if(error==0)up++;}
static const unsigned char*avp(unsigned index,unsigned type,unsigned*size)
{const unsigned char*f=frames[index]+28;REQUIRE(sizes[index]>=40&&u16(f)==0xc802&&u16(f+2)+28<=sizes[index]);unsigned end=u16(f+2);for(unsigned at=12;at+6<=end;){unsigned n=u16(f+at)&1023;REQUIRE(n>=6&&at+n<=end&&u16(f+at+2)==0);if(u16(f+at+4)==type){*size=n-6;return f+at+6;}at+=n;}return NULL;}
static unsigned attribute(unsigned char*f,unsigned at,unsigned type,const void*payload,unsigned length)
{put16(f+at,0x8000|length+6);put16(f+at+2,0);put16(f+at+4,type);if(length)memcpy(f+at+6,payload,length);return at+6+length;}
static unsigned control(unsigned char*f,unsigned message,unsigned ns,unsigned nr)
{memset(f,0,2048);put16(f,0xc802);put16(f+4,remote_tunnel);put16(f+6,remote_session);put16(f+8,ns);put16(f+10,nr);unsigned at=12;if(message){unsigned char value[2];put16(value,message);at=attribute(f,at,0,value,2);}return at;}
static void inject(unsigned char*l2tp,unsigned length)
{unsigned char frame[2048]={0};REQUIRE(length<=2000);frame[0]=0x45;put16(frame+2,length+28);frame[8]=64;frame[9]=17;frame[12]=frame[16]=10;frame[13]=frame[17]=8;frame[15]=2;frame[19]=1;put16(frame+10,checksum(frame,20));put16(frame+20,1701);put16(frame+22,local_port);put16(frame+24,length+8);memcpy(frame+28,l2tp,length);struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)(length+28),PBUF_RAM);REQUIRE(p&&pbuf_take(p,frame,(u16_t)(length+28))==0&&ip_input(p,&underlying)==0);}
static void digest(unsigned id,const unsigned char*challenge,unsigned length,unsigned char*out,int wrong_secret)
{unsigned char data[256];data[0]=(unsigned char)id;memcpy(data+1,wrong_secret?"wrong!":"secret",6);memcpy(data+7,challenge,length);md5(data,(int)(length+7),out);}
static void data_packet(unsigned protocol,const unsigned char*payload,unsigned length)
{unsigned char frame[2048]={0};put16(frame,2);put16(frame+2,remote_tunnel);put16(frame+4,remote_session);put16(frame+6,protocol);if(length)memcpy(frame+8,payload,length);inject(frame,length+8);}
static void response_ppp(unsigned protocol,unsigned code,unsigned id,const unsigned char*payload,unsigned length)
{unsigned char frame[2048];frame[0]=(unsigned char)code;frame[1]=(unsigned char)id;put16(frame+2,length+4);if(length)memcpy(frame+4,payload,length);data_packet(protocol,frame,length+4);}
static void negotiate(void)
{
 unsigned sent=0;
 for(unsigned index=0;index<packet_count;index++){
  REQUIRE(index<200);const unsigned char*frame=packets[index];unsigned protocol=u16(frame),code=frame[2],id=frame[3],length=u16(frame+4);REQUIRE(length>=4&&length+2<=packet_sizes[index]);if(code!=1)continue;
  if(protocol==0xc021){response_ppp(protocol,2,id,frame+6,length-4);if(!(sent&1)){sent|=1;response_ppp(protocol,1,90,NULL,0);}}
  else if(protocol==0x8021){
   unsigned char rejected[128];unsigned rejected_length=0;for(unsigned k=6;k<2+length;){unsigned n=frame[k+1];REQUIRE(n>=2&&k+n<=2+length);if(frame[k]==2){memcpy(rejected+rejected_length,frame+k,n);rejected_length+=n;}k+=n;}
   if(rejected_length){response_ppp(protocol,4,id,rejected,rejected_length);continue;}
   response_ppp(protocol,2,id,frame+6,length-4);if(!(sent&2)){sent|=2;const unsigned char address[]={3,6,10,7,0,2};response_ppp(protocol,1,91,address,sizeof(address));}
  }else if(protocol==0x8057){response_ppp(protocol,2,id,frame+6,length-4);if(!(sent&4)){sent|=4;const unsigned char iid[]={1,10,0xee,0xdd,0xcc,0xbb,0xaa,0x99,0x88,2};response_ppp(protocol,1,92,iid,sizeof(iid));}}
  else if(protocol==0x80fd){response_ppp(protocol,4,id,frame+6,length-4);}
 }
 REQUIRE(sent==7&&up==2&&session->phase==PPP_PHASE_RUNNING);
}
static void udp_received(void*a,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*address,u16_t port)
{(void)a;(void)pcb;REQUIRE(IP_IS_V4(address)&&port==9001&&p->tot_len==73);unsigned char bytes[73];REQUIRE(pbuf_copy_partial(p,bytes,73,0)==73);for(unsigned i=0;i<73;i++)REQUIRE(bytes[i]==(unsigned char)(i*7));pbuf_free(p);datagrams++;}
static void exchange(void)
{
 ip_addr_t local,remote;REQUIRE(ipaddr_aton("10.7.0.1",&local)&&ipaddr_aton("10.7.0.2",&remote));struct udp_pcb*udp=udp_new();REQUIRE(udp&&udp_bind(udp,&local,9000)==0);udp_recv(udp,udp_received,NULL);
 unsigned char ip[101]={0};ip[0]=0x45;put16(ip+2,101);ip[8]=64;ip[9]=17;ip[12]=ip[16]=10;ip[13]=ip[17]=7;ip[15]=2;ip[19]=1;put16(ip+10,checksum(ip,20));put16(ip+20,9001);put16(ip+22,9000);put16(ip+24,81);for(unsigned i=0;i<73;i++)ip[28+i]=(unsigned char)(i*7);
 data_packet(0x21,ip,sizeof(ip));REQUIRE(datagrams==1);
 unsigned start=packet_count;struct pbuf*p=pbuf_alloc(PBUF_TRANSPORT,73,PBUF_RAM);REQUIRE(p&&pbuf_take(p,ip+28,73)==0);REQUIRE(udp_sendto_if_src(udp,p,&remote,9001,&pppif,&local)==0);pbuf_free(p);REQUIRE(packet_count==start+1);const unsigned char*wire=packets[start];REQUIRE(u16(wire)==0x21&&wire[11]==17&&u16(wire+22)==9000&&u16(wire+24)==9001&&!memcmp(wire+30,ip+28,73));udp_remove(udp);
}
static void one(void*argument)
{
 int wrong=*(int*)argument;count=remote_tunnel=remote_session=packet_count=up=datagrams=0;status_error=0;unsigned memory=lwip_stats.mem.used,pool=lwip_stats.memp[MEMP_PBUF_POOL]->used,timers=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;
 ip4_addr_t local,mask,gateway;IP4_ADDR(&local,10,8,0,1);IP4_ADDR(&mask,255,255,255,0);IP4_ADDR(&gateway,0,0,0,0);REQUIRE(netif_add(&underlying,&local,&mask,&gateway,NULL,init,ip_input));netif_set_up(&underlying);netif_set_link_up(&underlying);
 ip_addr_t remote;REQUIRE(ipaddr_aton("10.8.0.2",&remote));session=pppol2tp_create(&pppif,&underlying,&remote,1701,(const u8_t*)"secret",6,status,NULL);REQUIRE(session);ip4_addr_t inner_local,inner_remote;IP4_ADDR(&inner_local,10,7,0,1);IP4_ADDR(&inner_remote,10,7,0,2);ppp_set_ipcp_ouraddr(session,&inner_local);ppp_set_ipcp_hisaddr(session,&inner_remote);REQUIRE(ppp_connect(session,0)==0&&count==1);local_port=u16(frames[0]+20);unsigned n;const unsigned char*v=avp(0,0,&n);REQUIRE(v&&n==2&&u16(v)==1);v=avp(0,9,&n);REQUIRE(v&&n==2);remote_tunnel=u16(v);v=avp(0,11,&n);REQUIRE(v&&n==16);unsigned char response[16];digest(2,v,n,response,wrong);
 unsigned char frame[2048],value[2],challenge[16];for(unsigned i=0;i<16;i++)challenge[i]=(unsigned char)(i+1);unsigned length=control(frame,2,0,1);put16(value,0x1122);length=attribute(frame,length,9,value,2);length=attribute(frame,length,13,response,16);length=attribute(frame,length,11,challenge,16);put16(frame+2,length);inject(frame,length);
 if(wrong){REQUIRE(session->phase==PPP_PHASE_DEAD&&status_error!=0);}
 else{
  REQUIRE(count==3);v=avp(1,0,&n);REQUIRE(v&&n==2&&u16(v)==3);v=avp(1,13,&n);digest(3,challenge,16,response,0);REQUIRE(v&&n==16&&!memcmp(v,response,16));v=avp(2,0,&n);REQUIRE(v&&n==2&&u16(v)==10);v=avp(2,14,&n);REQUIRE(v&&n==2);remote_session=u16(v);
  length=control(frame,11,1,3);put16(value,0x3344);length=attribute(frame,length,14,value,2);put16(frame+2,length);inject(frame,length);REQUIRE(count==4);v=avp(3,0,&n);REQUIRE(v&&n==2&&u16(v)==12);
  length=control(frame,0,2,4);put16(frame+2,length);inject(frame,length);REQUIRE(count==5&&u16(frames[4]+28)==2&&u16(frames[4]+30)==0x1122&&u16(frames[4]+32)==0x3344);REQUIRE(session->phase==PPP_PHASE_ESTABLISH);
  /* PPP control is an actual data packet in the authenticated tunnel. */
  unsigned offset=34;if(u16(frames[4]+offset)==0xff03)offset+=2;REQUIRE(u16(frames[4]+offset)==0xc021&&frames[4][offset+2]==1);
  negotiate();exchange();
  REQUIRE(ppp_close(session,0)==0);unsigned last=count-1;offset=34;if(u16(frames[last]+offset)==0xff03)offset+=2;REQUIRE(u16(frames[last]+offset)==0xc021&&frames[last][offset+2]==5);
  memset(frame,0,sizeof(frame));put16(frame,2);put16(frame+2,remote_tunnel);put16(frame+4,remote_session);put16(frame+6,0xc021);frame[8]=6;frame[9]=frames[last][offset+3];put16(frame+10,4);inject(frame,12);REQUIRE(session->phase==PPP_PHASE_DEAD);v=avp(count-1,0,&n);REQUIRE(v&&n==2&&u16(v)==4);
 }
 REQUIRE(ppp_free(session)==0);session=NULL;netif_set_down(&underlying);netif_remove(&underlying);REQUIRE(lwip_stats.mem.used==memory&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers);
}
int main(int argc,char**argv)
{if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_host_initialize(ready,NULL);while(!atomic_load(&initialized)){}for(int wrong=0;wrong<2;wrong++)REQUIRE(lwip_host_call(one,&wrong)==0);REQUIRE(lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 puts("{\"tunnel_auth\":true,\"wrong_secret_rejected\":true,\"mutual_challenge\":true,\"ppp_data_control\":true,\"dual_stack_negotiated\":true,\"udp_both_directions\":true,\"stopccn\":true,\"baseline_restored\":true}");return 0;}
