#include "lwip/dhcp.h"
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
static atomic_int ready;static struct netif*client;static unsigned discover,request,decline,release,naks;static int silent,hold,nak_next;static unsigned char pending[8][400];static unsigned sizes[8],queued;
static unsigned short checksum(const unsigned char*p,unsigned n){unsigned sum=0;for(unsigned i=0;i<n;i+=2)sum+=(p[i]<<8)|(i+1<n?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return ~sum;}
static void option(unsigned char*p,unsigned*cursor,unsigned code,const unsigned char*data,unsigned n){p[(*cursor)++]=code;p[(*cursor)++]=n;memcpy(p+*cursor,data,n);*cursor+=n;}
static unsigned response(const unsigned char*query,unsigned char*frame,unsigned type){memset(frame,0,346);memset(frame,255,6);unsigned char mac[]={2,0,0,0,0,2},server[]={10,0,0,2},assigned[]={10,0,0,100};memcpy(frame+6,mac,6);frame[12]=8;frame[14]=0x45;frame[16]=1;frame[17]=76;frame[22]=64;frame[23]=17;memcpy(frame+26,server,4);memset(frame+30,255,4);unsigned check=checksum(frame+14,20);frame[24]=check>>8;frame[25]=check;frame[35]=67;frame[37]=68;frame[38]=1;frame[39]=56;unsigned char*p=frame+42;p[0]=2;p[1]=1;p[2]=6;memcpy(p+4,query+4,4);p[10]=0x80;memcpy(p+28,query+28,16);if(type!=6)memcpy(p+16,assigned,4);memcpy(p+20,server,4);unsigned char cookie[]={99,130,83,99};memcpy(p+236,cookie,4);unsigned c=240;unsigned char t=type;option(p,&c,53,&t,1);option(p,&c,54,server,4);if(type!=6){unsigned char lease[]={0,0,0,120},mask[]={255,255,255,0},renew[]={0,0,0,60},rebind[]={0,0,0,105};option(p,&c,51,lease,4);option(p,&c,1,mask,4);option(p,&c,3,server,4);option(p,&c,6,server,4);option(p,&c,58,renew,4);option(p,&c,59,rebind,4);}p[c]=255;return 346;}
int lwip_host_frame_send(void*interface,const unsigned char*frame,unsigned length){(void)interface;if(length>=42&&frame[12]==8&&frame[13]==6&&frame[21]==1&&frame[38]==10&&frame[41]==2){R(queued<8);unsigned char*p=pending[queued];memcpy(p,frame,42);memcpy(p,frame+6,6);unsigned char mac[]={2,0,0,0,0,2};memcpy(p+6,mac,6);p[21]=2;memcpy(p+32,frame+22,10);memcpy(p+22,mac,6);memcpy(p+28,frame+38,4);sizes[queued++]=42;return 0;}if(length<282||frame[12]!=8||frame[13]!=0||frame[23]!=17||frame[35]!=68||frame[37]!=67)return 0;unsigned header=(frame[14]&15)*4;const unsigned char*p=frame+14+header+8;unsigned plen=length-14-header-8;R(plen>=244&&p[0]==1&&p[1]==1&&p[2]==6);unsigned type=0;for(unsigned c=240;c<plen;){unsigned code=p[c++];if(code==255)break;if(!code)continue;R(c<plen);unsigned n=p[c++];R(c+n<=plen);if(code==53){R(n==1);type=p[c];break;}c+=n;}if(type==1)discover++;else if(type==3)request++;else if(type==4){decline++;return 0;}else if(type==7){release++;return 0;}else R(0);if(silent||(hold&&type==1))return 0;unsigned reply=type==1?2:5;if(type==3&&nak_next){nak_next=0;hold=1;reply=6;naks++;}R(queued<8);sizes[queued]=response(p,pending[queued],reply);queued++;return 0;}
static void initialized(void*unused){(void)unused;atomic_store(&ready,1);}
static void setup(void*unused){(void)unused;unsigned char mac[]={2,0,0,0,0,1};client=lwip_host_interface_add("0.0.0.0","0.0.0.0","0.0.0.0","fd00::1",mac);R(client);lwip_host_interface_up(client);R(dhcp_start(client)==0);}
static void tick(void*unused){(void)unused;dhcp_fine_tmr();for(unsigned i=0;i<5;i++)acd_tmr();}
static void pump(void*unused){(void)unused;unsigned n=queued;unsigned char frames[8][400];unsigned lengths[8];memcpy(frames,pending,sizeof frames);memcpy(lengths,sizes,sizeof lengths);queued=0;for(unsigned i=0;i<n;i++)R(lwip_host_frame_input(client,frames[i],lengths[i])==0);}
static void empty(void*unused){(void)unused;}
static void drain(void){for(unsigned i=0;i<8;i++){R(lwip_host_call(pump,NULL)==0);R(lwip_host_call(empty,NULL)==0);lwip_host_poll();}}
static int bound;static void check_bound(void*unused){(void)unused;bound=dhcp_supplied_address(client);}
static void until_bound(void){for(unsigned i=0;i<100;i++){drain();R(lwip_host_call(check_bound,NULL)==0);if(bound)return;R(lwip_host_call(tick,NULL)==0);}R(0);}
static void address(void*arg){int wanted=(int)(size_t)arg;R(netif_ip4_addr(client)->addr==(wanted?PP_HTONL(0x0a000064):0));if(wanted){R(netif_ip4_netmask(client)->addr==PP_HTONL(0xffffff00));R(netif_ip4_gw(client)->addr==PP_HTONL(0x0a000002));}}
static void coarse(void*unused){(void)unused;dhcp_coarse_tmr();}
static void renew(void*unused){(void)unused;R(dhcp_renew(client)==0);}
static void conflict(void*unused){(void)unused;unsigned char frame[42]={2,0,0,0,0,1,2,0,0,0,0,99,8,6,0,1,8,0,6,4,0,2,2,0,0,0,0,99,10,0,0,100,2,0,0,0,0,1,10,0,0,100};R(lwip_host_frame_input(client,frame,42)==0);}
static void cleanup(void*unused){(void)unused;dhcp_release_and_stop(client);dhcp_cleanup(client);lwip_host_interface_remove(client);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);until_bound();R(lwip_host_call(address,(void*)1)==0);R(discover&&request);puts("{\"case\":\"dhcp-dora-acd-options\"}");unsigned previous=request;R(lwip_host_call(coarse,NULL)==0);until_bound();R(request>previous);puts("{\"case\":\"dhcp-T1-renewal\"}");nak_next=1;R(lwip_host_call(renew,NULL)==0);drain();R(naks==1);R(lwip_host_call(address,NULL)==0);hold=0;until_bound();R(lwip_host_call(address,(void*)1)==0);puts("{\"case\":\"dhcp-NAK-recovery\"}");silent=1;R(lwip_host_call(coarse,NULL)==0);R(lwip_host_call(coarse,NULL)==0);drain();R(lwip_host_call(address,NULL)==0);silent=0;until_bound();puts("{\"case\":\"dhcp-expiry-recovery\"}");hold=1;R(lwip_host_call(conflict,NULL)==0);drain();R(lwip_host_call(conflict,NULL)==0);drain();R(decline>0);R(lwip_host_call(address,NULL)==0);hold=0;until_bound();R(lwip_host_call(address,(void*)1)==0);puts("{\"case\":\"dhcp-conflict-decline-recovery\"}");R(lwip_host_call(cleanup,NULL)==0);R(release>0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
return 0;}
