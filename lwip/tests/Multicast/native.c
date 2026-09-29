#include "lwip/udp.h"
#include "lwip/igmp.h"
#include "lwip/mld6.h"
#include "lwip/tcpip.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);extern void lwip_host_poll(void);
extern void*lwip_host_interface_add(const char*,const char*,const char*,const char*,const unsigned char*);extern void lwip_host_interface_up(void*);extern void lwip_host_interface_remove(void*);extern int lwip_host_frame_input(void*,const unsigned char*,unsigned);
static struct netif*interface;static struct udp_pcb*receiver;static atomic_int ready;static unsigned reports,leaves,received;static int ipv6;static unsigned char group6[16]={0xff,2,0,0,0,0,0,0,0,0,0,0,0,0,0x12,0x34},group4[]={239,1,2,3};
int lwip_host_frame_send(void*n,const unsigned char*f,unsigned len){(void)n;unsigned offset;if(!ipv6){if(len<42||f[12]!=8||f[13]||f[23]!=2)return 0;offset=14+(f[14]&15)*4;if(len<offset+8||memcmp(f+offset+4,group4,4))return 0;}else{if(len<78||f[12]!=0x86||f[13]!=0xdd)return 0;offset=54;unsigned next=f[20];if(next==0){next=f[offset];offset+=(f[offset+1]+1)*8;}if(next!=58||offset+24>len||memcmp(f+offset+8,group6,16))return 0;}if(f[offset]==(ipv6?131:0x16))reports++;if(f[offset]==(ipv6?132:0x17))leaves++;return 0;}
static void initialized(void*unused){(void)unused;atomic_store(&ready,1);}
static unsigned short checksum(const unsigned char*p,unsigned n){unsigned sum=0;for(unsigned i=0;i<n;i+=2)sum+=(p[i]<<8)|(i+1<n?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return ~sum;}
static void u16(unsigned char*p,unsigned n){p[0]=n>>8;p[1]=n;}
struct input{unsigned char bytes[128];unsigned length;};
static void datagram(struct input*out){unsigned char*f=out->bytes;memset(f,0,128);f[6]=2;f[11]=2;unsigned off;if(ipv6){f[0]=f[1]=0x33;memcpy(f+2,group6+12,4);f[12]=0x86;f[13]=0xdd;f[14]=0x60;f[19]=13;f[20]=17;f[21]=1;f[22]=0xfd;f[37]=2;memcpy(f+38,group6,16);off=54;}else{f[0]=1;f[2]=0x5e;f[3]=1;f[4]=2;f[5]=3;f[12]=8;f[14]=0x45;f[17]=33;f[22]=1;f[23]=17;f[26]=10;f[29]=2;memcpy(f+30,group4,4);u16(f+24,checksum(f+14,20));off=34;}u16(f+off,28601);u16(f+off+2,28600);u16(f+off+4,13);memcpy(f+off+8,"group",5);out->length=off+13;if(ipv6){unsigned char pseudo[53]={0};memcpy(pseudo,f+22,32);pseudo[35]=13;pseudo[39]=17;memcpy(pseudo+40,f+off,13);unsigned check=checksum(pseudo,53);u16(f+off+6,check?check:65535);}}
static void query(struct input*out){unsigned char*f=out->bytes;memset(f,0,128);f[6]=2;f[11]=2;if(!ipv6){f[0]=1;f[2]=0x5e;f[5]=1;f[12]=8;f[14]=0x45;f[17]=28;f[22]=1;f[23]=2;f[26]=10;f[29]=2;f[30]=224;f[33]=1;u16(f+24,checksum(f+14,20));f[34]=0x11;f[35]=10;u16(f+36,checksum(f+34,8));out->length=42;}else{f[0]=f[1]=0x33;f[5]=1;f[12]=0x86;f[13]=0xdd;f[14]=0x60;f[19]=32;f[20]=0;f[21]=1;f[22]=0xfe;f[23]=0x80;f[37]=2;f[38]=0xff;f[39]=2;f[53]=1;f[54]=58;f[56]=5;f[57]=2;f[60]=1;f[62]=130;u16(f+66,1000);unsigned char pseudo[64]={0};memcpy(pseudo,f+22,32);pseudo[35]=24;pseudo[39]=58;memcpy(pseudo+40,f+62,24);u16(f+64,checksum(pseudo,64));out->length=86;}}
static void receive(void*arg,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*addr,u16_t port){(void)arg;(void)pcb;(void)addr;(void)port;unsigned char b[5];R(p->tot_len==5&&pbuf_copy_partial(p,b,5,0)==5&&memcmp(b,"group",5)==0);received++;pbuf_free(p);}
static void setup(void*unused){(void)unused;unsigned char mac[]={2,0,0,0,0,1};interface=lwip_host_interface_add("10.0.0.1","255.255.255.0","0.0.0.0","fd00::1",mac);R(interface);lwip_host_interface_up(interface);}
static void receiver_start(void*unused){(void)unused;receiver=udp_new_ip_type(ipv6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);R(receiver);R(udp_bind(receiver,IP_ANY_TYPE,28600)==0);udp_recv(receiver,receive,NULL);}
static void input(void*arg){struct input*p=arg;R(lwip_host_frame_input(interface,p->bytes,p->length)==0);}
static void empty(void*unused){(void)unused;}
static void inject(struct input*p){R(lwip_host_call(input,p)==0);R(lwip_host_call(empty,NULL)==0);lwip_host_poll();R(lwip_host_call(empty,NULL)==0);}
static void member(void*arg){int join=(int)(size_t)arg;ip_addr_t group;R(ipaddr_aton(ipv6?"ff02::1234":"239.1.2.3",&group));err_t result=ipv6?(join?mld6_joingroup_netif(interface,ip_2_ip6(&group)):mld6_leavegroup_netif(interface,ip_2_ip6(&group))):(join?igmp_joingroup_netif(interface,ip_2_ip4(&group)):igmp_leavegroup_netif(interface,ip_2_ip4(&group)));R(result==0);}
static void ticks(void*arg){unsigned n=(unsigned)(size_t)arg;for(unsigned i=0;i<n;i++){if(ipv6)mld6_tmr();else igmp_tmr();}}
static void receiver_stop(void*unused){(void)unused;udp_remove(receiver);}
static void cleanup(void*unused){(void)unused;lwip_host_interface_remove(interface);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);for(ipv6=0;ipv6<=1;ipv6++){reports=leaves=received=0;R(lwip_host_call(receiver_start,NULL)==0);struct input in;datagram(&in);inject(&in);R(received==0);R(lwip_host_call(member,(void*)1)==0);R(lwip_host_call(ticks,(void*)110)==0);R(reports>0);datagram(&in);inject(&in);R(received==1);unsigned before=reports;query(&in);inject(&in);R(lwip_host_call(ticks,(void*)15)==0);R(reports>before);R(lwip_host_call(member,NULL)==0);R(leaves>0);datagram(&in);inject(&in);R(received==1);R(lwip_host_call(receiver_stop,NULL)==0);printf("{\"case\":\"%s-join-query-leave-admission\",\"reports\":%u,\"leaves\":%u}\n",ipv6?"MLD6":"IGMP",reports,leaves);}R(lwip_host_call(cleanup,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
return 0;}
