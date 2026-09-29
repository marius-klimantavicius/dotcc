/* Independently encoded RFC2516 access-concentrator discovery/control peer.
 * The peer does not reuse lwIP's discovery implementation or structures. */
#include "lwip/init.h"
#include "lwip/timeouts.h"
#include "lwip/stats.h"
#include "lwip/udp.h"
#include "netif/ppp/pppoe.h"
#include "netif/ppp/ppp_impl.h"
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
static struct netif ethernet,pppif;static ppp_pcb*session;
static unsigned char captured[256][2048];static unsigned lengths[256],count,up,datagrams;
static unsigned char unique[32];static unsigned unique_length;
static unsigned short u16(const unsigned char*p){return (unsigned short)((p[0]<<8)|p[1]);}
static void put16(unsigned char*p,unsigned value){p[0]=(unsigned char)(value>>8);p[1]=(unsigned char)value;}
static err_t output(struct netif*n,struct pbuf*p)
{REQUIRE(n==&ethernet&&count<256&&p->tot_len<=2048);lengths[count]=p->tot_len;REQUIRE(pbuf_copy_partial(p,captured[count],p->tot_len,0)==p->tot_len);count++;return ERR_OK;}
static void status(ppp_pcb*pcb,int error,void*argument){(void)argument;REQUIRE(pcb==session);if(error==0)up++;}
static const unsigned char*tag(const unsigned char*f,unsigned length,unsigned code,unsigned*size)
{
 REQUIRE(length>=20&&u16(f+18)+20<=length);unsigned limit=20+u16(f+18);
 for(unsigned offset=20;offset+4<=limit;){unsigned n=u16(f+offset+2);REQUIRE(offset+4+n<=limit);if(u16(f+offset)==code){*size=n;return f+offset+4;}offset+=4+n;}return NULL;
}
static unsigned append(unsigned char*frame,unsigned at,unsigned type,const void*data,unsigned length)
{put16(frame+at,type);put16(frame+at+2,length);if(length)memcpy(frame+at+4,data,length);return at+4+length;}
static void inject(unsigned char*frame,unsigned length,int data)
{struct pbuf*p=pbuf_alloc(PBUF_RAW,(u16_t)length,PBUF_RAM);REQUIRE(p&&pbuf_take(p,frame,(u16_t)length)==0);if(data)pppoe_data_input(&ethernet,p);else pppoe_disc_input(&ethernet,p);}
static unsigned header(unsigned char*frame,unsigned code,unsigned id,int data)
{
 memset(frame,0,2048);memcpy(frame,ethernet.hwaddr,6);const unsigned char peer[]={2,0,0,0,0,2};memcpy(frame+6,peer,6);put16(frame+12,data?0x8864:0x8863);frame[14]=0x11;frame[15]=(unsigned char)code;put16(frame+16,id);return 20;
}
static void response(unsigned protocol,unsigned code,unsigned id,const unsigned char*payload,unsigned payload_length)
{
 unsigned char frame[2048];unsigned length=header(frame,0,0x1234,1);put16(frame+length,protocol);frame[length+2]=(unsigned char)code;frame[length+3]=(unsigned char)id;put16(frame+length+4,payload_length+4);if(payload_length)memcpy(frame+length+6,payload,payload_length);length+=6+payload_length;put16(frame+18,length-20);inject(frame,length,1);
}
static void negotiate(void)
{
 unsigned sent=0;
 for(unsigned index=3;index<count;index++){
  REQUIRE(index<200);const unsigned char*frame=captured[index];if(u16(frame+12)!=0x8864)continue;unsigned protocol=u16(frame+20),code=frame[22],id=frame[23],length=u16(frame+24);REQUIRE(length>=4&&length+22<=lengths[index]);if(code!=1)continue;
  if(protocol==0xc021){response(protocol,2,id,frame+26,length-4);if(!(sent&1)){sent|=1;response(protocol,1,90,NULL,0);}}
  else if(protocol==0x8021){
   unsigned char rejected[128];unsigned rejected_length=0;for(unsigned k=26;k<22+length;){unsigned n=frame[k+1];REQUIRE(n>=2&&k+n<=22+length);if(frame[k]==2){memcpy(rejected+rejected_length,frame+k,n);rejected_length+=n;}k+=n;}
   if(rejected_length){response(protocol,4,id,rejected,rejected_length);continue;}
   response(protocol,2,id,frame+26,length-4);if(!(sent&2)){sent|=2;const unsigned char address[]={3,6,10,7,0,2};response(protocol,1,91,address,sizeof(address));}
  }else if(protocol==0x8057){response(protocol,2,id,frame+26,length-4);if(!(sent&4)){sent|=4;const unsigned char iid[]={1,10,0xee,0xdd,0xcc,0xbb,0xaa,0x99,0x88,2};response(protocol,1,92,iid,sizeof(iid));}}
  else if(protocol==0x80fd){response(protocol,4,id,frame+26,length-4);}
 }
 REQUIRE(sent==7&&up==2&&session->phase==PPP_PHASE_RUNNING);
}
static void udp_received(void*a,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*address,u16_t port)
{(void)a;(void)pcb;REQUIRE(IP_IS_V4(address)&&port==9001&&p->tot_len==73);unsigned char bytes[73];REQUIRE(pbuf_copy_partial(p,bytes,73,0)==73);for(unsigned i=0;i<73;i++)REQUIRE(bytes[i]==(unsigned char)(i*7));pbuf_free(p);datagrams++;}
static unsigned checksum(const unsigned char*p,unsigned length)
{unsigned sum=0;for(unsigned i=0;i<length;i+=2)sum+=(p[i]<<8)|(i+1<length?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return (~sum)&65535;}
static void exchange(void)
{
 ip_addr_t local,remote;REQUIRE(ipaddr_aton("10.7.0.1",&local)&&ipaddr_aton("10.7.0.2",&remote));struct udp_pcb*udp=udp_new();REQUIRE(udp&&udp_bind(udp,&local,9000)==0);udp_recv(udp,udp_received,NULL);
 unsigned char frame[2048];header(frame,0,0x1234,1);put16(frame+20,0x21);unsigned char*ip=frame+22;ip[0]=0x45;put16(ip+2,101);ip[8]=64;ip[9]=17;ip[12]=ip[16]=10;ip[13]=ip[17]=7;ip[15]=2;ip[19]=1;put16(ip+10,checksum(ip,20));put16(ip+20,9001);put16(ip+22,9000);put16(ip+24,81);for(unsigned i=0;i<73;i++)ip[28+i]=(unsigned char)(i*7);put16(frame+18,103);
 inject(frame,123,1);REQUIRE(datagrams==1);
 unsigned start=count;struct pbuf*p=pbuf_alloc(PBUF_TRANSPORT,73,PBUF_RAM);REQUIRE(p&&pbuf_take(p,ip+28,73)==0);REQUIRE(udp_sendto_if_src(udp,p,&remote,9001,&pppif,&local)==0);pbuf_free(p);REQUIRE(count==start+1);const unsigned char*wire=captured[start];REQUIRE(u16(wire+12)==0x8864&&u16(wire+16)==0x1234&&u16(wire+20)==0x21&&wire[31]==17&&u16(wire+42)==9000&&u16(wire+44)==9001&&!memcmp(wire+50,ip+28,73));udp_remove(udp);
}
static void run(void*argument)
{
 (void)argument;unsigned baseline=lwip_stats.mem.used,pool=lwip_stats.memp[MEMP_PBUF_POOL]->used,timers=lwip_stats.memp[MEMP_SYS_TIMEOUT]->used;
 memset(&ethernet,0,sizeof(ethernet));ethernet.hwaddr_len=6;ethernet.hwaddr[0]=2;ethernet.hwaddr[5]=1;ethernet.mtu=1500;ethernet.linkoutput=output;
 session=pppoe_create(&pppif,&ethernet,"service","concentrator",status,NULL);REQUIRE(session);ip4_addr_t local,remote;IP4_ADDR(&local,10,7,0,1);IP4_ADDR(&remote,10,7,0,2);ppp_set_ipcp_ouraddr(session,&local);ppp_set_ipcp_hisaddr(session,&remote);REQUIRE(ppp_connect(session,0)==0&&count==1);
 unsigned n=0;const unsigned char*value;REQUIRE(u16(captured[0]+12)==0x8863&&captured[0][15]==9&&u16(captured[0]+16)==0);value=tag(captured[0],lengths[0],0x0103,&n);REQUIRE(value&&n>0&&n<=sizeof(unique));memcpy(unique,value,n);unique_length=n;
 value=tag(captured[0],lengths[0],0x0101,&n);REQUIRE(value);
#if PPPOE_SCNAME_SUPPORT
 REQUIRE(n==7&&!memcmp(value,"service",7));value=tag(captured[0],lengths[0],0x0102,&n);REQUIRE(value&&n==12&&!memcmp(value,"concentrator",12));
#else
 REQUIRE(n==0);
#endif
 unsigned char frame[2048];unsigned length=header(frame,7,0,0);length=append(frame,length,0x0101,"service",7);length=append(frame,length,0x0102,"concentrator",12);length=append(frame,length,0x0104,"cookie",6);length=append(frame,length,0x0103,unique,unique_length);put16(frame+18,length-20);
 inject(frame,length,0);REQUIRE(count==2&&captured[1][15]==0x19);value=tag(captured[1],lengths[1],0x0104,&n);REQUIRE(value&&n==6&&!memcmp(value,"cookie",6));
 length=header(frame,0x65,0x1234,0);length=append(frame,length,0x0103,unique,unique_length);put16(frame+18,length-20);inject(frame,length,0);
 REQUIRE(count==3&&u16(captured[2]+12)==0x8864&&u16(captured[2]+16)==0x1234&&u16(captured[2]+20)==0xc021&&captured[2][22]==1);REQUIRE(session->phase==PPP_PHASE_ESTABLISH);
 /* Independent LCP Configure-Reject deliberately rejects every offered option.
  * The real FSM sends a fresh Configure-Request with a new identifier. */
 unsigned offered=u16(captured[2]+24);REQUIRE(offered>=4);length=header(frame,0,0x1234,1);frame[20]=0xc0;frame[21]=0x21;memcpy(frame+22,captured[2]+22,offered);frame[22]=4;length=22+offered;put16(frame+18,length-20);inject(frame,length,1);REQUIRE(count>=4&&captured[count-1][22]==1&&captured[count-1][23]!=captured[2][23]);
 negotiate();exchange();
 REQUIRE(ppp_close(session,0)==0);REQUIRE(captured[count-1][22]==5);unsigned char termination_id=captured[count-1][23];length=header(frame,0,0x1234,1);frame[20]=0xc0;frame[21]=0x21;frame[22]=6;frame[23]=termination_id;put16(frame+24,4);put16(frame+18,6);inject(frame,26,1);REQUIRE(session->phase==PPP_PHASE_DEAD);REQUIRE(captured[count-1][15]==0xa7&&u16(captured[count-1]+16)==0x1234);REQUIRE(ppp_free(session)==0);session=NULL;
#if NO_SYS
 netif_poll_all();
#endif
 REQUIRE(lwip_stats.mem.used==baseline&&lwip_stats.memp[MEMP_PBUF_POOL]->used==pool&&lwip_stats.memp[MEMP_SYS_TIMEOUT]->used==timers);
 printf("{\"discovery\":true,\"cookie_echo\":true,\"lcp_reject_progress\":true,\"dual_stack_negotiated\":true,\"udp_both_directions\":true,\"padt\":true,\"service_name_option\":%d,\"baseline_restored\":true}\n",PPPOE_SCNAME_SUPPORT);
}
int main(int argc,char**argv)
{if(argc>1&&!strcmp(argv[1],"--negative"))REQUIRE(0);lwip_host_initialize(ready,NULL);while(!atomic_load(&initialized)){}REQUIRE(lwip_host_call(run,NULL)==0&&lwip_host_stop_worker()==0);
#if !NO_SYS
 native_join_workers();
#endif
 return 0;}
