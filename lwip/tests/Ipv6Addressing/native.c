#include "lwip/netif.h"
#include "lwip/tcpip.h"
#include "lwip/nd6.h"
#include "lwip/dhcp6.h"
#include "lwip/dns.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);extern void lwip_host_poll(void);
extern void*lwip_host_interface_add(const char*,const char*,const char*,const char*,const unsigned char*);extern void lwip_host_interface_up(void*);extern void lwip_host_interface_remove(void*);extern int lwip_host_frame_input(void*,const unsigned char*,unsigned);
static struct netif*interface;static unsigned char captured[128][1600];static unsigned lengths[128],count;static atomic_int ready;
int lwip_host_frame_send(void*n,const unsigned char*b,unsigned len){(void)n;R(count<128&&len<=1600);memcpy(captured[count],b,len);lengths[count++]=len;return 0;}
static void initialized(void*unused){(void)unused;atomic_store(&ready,1);}
static unsigned short checksum(const unsigned char*p,unsigned n){unsigned sum=0;for(unsigned i=0;i<n;i+=2)sum+=(p[i]<<8)|(i+1<n?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return ~sum;}
static void ip(unsigned char*p,const char*s){ip6_addr_t a;R(ip6addr_aton(s,&a));memcpy(p,a.addr,16);}
static void u16(unsigned char*p,unsigned n){p[0]=n>>8;p[1]=n;}
static void u32(unsigned char*p,unsigned n){p[0]=n>>24;p[1]=n>>16;p[2]=n>>8;p[3]=n;}
static unsigned frame(unsigned char*out,unsigned protocol,const unsigned char*payload,unsigned n,const unsigned char*src,const unsigned char*dst,unsigned hop){memset(out,0,n+54);out[0]=out[1]=0x33;memcpy(out+2,dst+12,4);out[6]=2;out[11]=2;out[12]=0x86;out[13]=0xdd;out[14]=0x60;u16(out+18,n);out[20]=protocol;out[21]=hop;memcpy(out+22,src,16);memcpy(out+38,dst,16);memcpy(out+54,payload,n);unsigned char pseudo[512]={0};R(n+40<=512);memcpy(pseudo,src,16);memcpy(pseudo+16,dst,16);u32(pseudo+32,n);pseudo[39]=protocol;memcpy(pseudo+40,payload,n);unsigned check=checksum(pseudo,n+40);u16(out+(protocol==17?60:56),check?check:65535);return n+54;}
static unsigned advertisement(unsigned char*out,unsigned prefix,unsigned valid,unsigned preferred,unsigned hop){unsigned char ra[56]={0},src[16],dst[16];ra[0]=134;ra[4]=64;ra[5]=0x40;u16(ra+6,30);ra[16]=ra[17]=1;ra[18]=2;ra[23]=2;ra[24]=3;ra[25]=4;ra[26]=64;ra[27]=0xc0;u32(ra+28,valid);u32(ra+32,preferred);char address[40];snprintf(address,sizeof(address),"2001:db8:%u::",prefix);ip(ra+40,address);ip(src,"fe80::2");ip(dst,"ff02::1");return frame(out,58,ra,56,src,dst,hop);}
struct input{unsigned char bytes[512];unsigned length;};static void input(void*arg){struct input*p=arg;R(lwip_host_frame_input(interface,p->bytes,p->length)==0);}
static void empty(void*unused){(void)unused;}
static void inject(struct input*p){R(lwip_host_call(input,p)==0);R(lwip_host_call(empty,NULL)==0);lwip_host_poll();R(lwip_host_call(empty,NULL)==0);}
static void tick(void*unused){(void)unused;nd6_tmr();}
static void dhcp_tick(void*unused){(void)unused;for(unsigned i=0;i<4;i++)dhcp6_tmr();}
static unsigned requests(unsigned char*out){unsigned n=0;for(unsigned i=0;i<count;i++)if(lengths[i]>=66&&captured[i][20]==17&&captured[i][54]==2&&captured[i][55]==34){memcpy(out,captured[i],lengths[i]);n++;}return n;}
static unsigned state;static void state_get(void*arg){state=netif_ip6_addr_state(interface,(unsigned)(size_t)arg);}
static unsigned get(unsigned index){R(lwip_host_call(state_get,(void*)(size_t)index)==0);return state;}
static void setup(void*unused){(void)unused;unsigned char mac[]={2,0,0,0,0,1};interface=lwip_host_interface_add("10.0.0.1","255.255.255.0","0.0.0.0","",mac);R(interface);lwip_host_interface_up(interface);netif_create_ip6_linklocal_address(interface,1);R(dhcp6_enable_stateless(interface)==0);}
static int dns;static unsigned dns_last=0x53;static void dns_check(void*unused){(void)unused;unsigned char wanted[16];ip(wanted,"2001:db8::53");wanted[15]=dns_last;dns=memcmp(ip_2_ip6(dns_getserver(0))->addr,wanted,16)==0;}
static void reply(struct input*p,const unsigned char*query,int wrong){unsigned char udp[32]={0},src[16];u16(udp,547);u16(udp+2,546);u16(udp+4,32);udp[8]=7;memcpy(udp+9,query+63,3);if(wrong)udp[11]^=1;u16(udp+12,23);u16(udp+14,16);ip(udp+16,"2001:db8::53");udp[31]=dns_last;ip(src,"fe80::2");p->length=frame(p->bytes,17,udp,32,src,query+22,64);unsigned char mac[]={2,0,0,0,0,1};memcpy(p->bytes,mac,6);}
static void conflict(struct input*p){unsigned char na[32]={0},src[16],dst[16];na[0]=136;na[4]=0x20;memcpy(na+8,netif_ip6_addr(interface,1)->addr,16);na[24]=2;na[25]=1;na[26]=2;na[31]=2;ip(src,"fe80::2");ip(dst,"ff02::1");p->length=frame(p->bytes,58,na,32,src,dst,255);}
static void cleanup(void*unused){(void)unused;dhcp6_disable(interface);dhcp6_cleanup(interface);lwip_host_interface_remove(interface);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);for(unsigned i=0;i<4&&get(0)!=IP6_ADDR_PREFERRED;i++)R(lwip_host_call(tick,NULL)==0);R(get(0)==IP6_ADDR_PREFERRED);struct input in;in.length=advertisement(in.bytes,1,8,4,64);inject(&in);R(get(1)==IP6_ADDR_INVALID);in.length=advertisement(in.bytes,1,8,4,255);in.bytes[56]^=1;inject(&in);R(get(1)==IP6_ADDR_INVALID);in.length=advertisement(in.bytes,1,8,4,255);inject(&in);R(ip6_addr_istentative(get(1)));unsigned char request[1600];unsigned found=0;for(unsigned i=0;i<count;i++)if(lengths[i]>=66&&captured[i][20]==17&&captured[i][54]==2&&captured[i][55]==34){memcpy(request,captured[i],lengths[i]);found++;}R(found&&request[62]==11);reply(&in,request,1);inject(&in);R(lwip_host_call(dns_check,NULL)==0);R(!dns);R(lwip_host_call(dhcp_tick,NULL)==0);unsigned char retried[1600];R(requests(retried)==found+1);R(memcmp(retried+63,request+63,3)==0);reply(&in,request,0);inject(&in);R(lwip_host_call(dns_check,NULL)==0);R(dns);in.length=advertisement(in.bytes,1,8,4,255);inject(&in);R(requests(request)==found+2);dns_last=0x54;reply(&in,request,0);inject(&in);R(lwip_host_call(dns_check,NULL)==0);R(dns);for(unsigned i=0;i<4&&get(1)!=IP6_ADDR_PREFERRED;i++)R(lwip_host_call(tick,NULL)==0);R(get(1)==IP6_ADDR_PREFERRED);for(unsigned i=0;i<8&&get(1)!=IP6_ADDR_DEPRECATED;i++)R(lwip_host_call(tick,NULL)==0);R(get(1)==IP6_ADDR_DEPRECATED);for(unsigned i=0;i<10&&get(1)!=IP6_ADDR_INVALID;i++)R(lwip_host_call(tick,NULL)==0);R(get(1)==IP6_ADDR_INVALID);in.length=advertisement(in.bytes,2,30,20,255);inject(&in);R(ip6_addr_istentative(get(1)));conflict(&in);inject(&in);R(get(1)==IP6_ADDR_DUPLICATED);R(lwip_host_call(cleanup,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
puts("{\"case\":\"ipv6-RA-SLAAC-DAD-DHCP6-lifetimes\"}");return 0;}
