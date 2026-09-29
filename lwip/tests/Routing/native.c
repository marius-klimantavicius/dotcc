#include "lwip/netif.h"
#include "lwip/tcpip.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);extern void lwip_host_poll(void);
extern void*lwip_host_interface_add(const char*,const char*,const char*,const char*,const unsigned char*);extern void lwip_host_interface_up(void*);extern void lwip_host_interface_remove(void*);extern int lwip_host_frame_input(void*,const unsigned char*,unsigned);
static FILE*pcap;
static void capture(const unsigned char*bytes,unsigned length){if(pcap){unsigned header[4]={0,0,length,length};R(fwrite(header,sizeof(header),1,pcap)==1);R(fwrite(bytes,length,1,pcap)==1);}}
static struct netif*interfaces[3];static unsigned char frames[64][1600];static unsigned lengths[64],ports[64],count;static atomic_int ready;
int lwip_host_frame_send(void*interface,const unsigned char*bytes,unsigned length){R(count<64&&length<=1600);unsigned port=0;for(unsigned i=0;i<3;i++)if(interfaces[i]==interface)port=i+1;if(!port)return -12;capture(bytes,length);ports[count]=port;lengths[count]=length;memcpy(frames[count++],bytes,length);return 0;}
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static void empty(void*arg){(void)arg;}
static void mac(unsigned char*p,unsigned subnet,int router){unsigned char v[]={2,0,0,0,subnet,router?1:2};memcpy(p,v,6);}
static void ip(unsigned char*p,unsigned subnet,int router){unsigned char v[]={10,subnet,0,router?1:2};memcpy(p,v,4);}
static unsigned short checksum(const unsigned char*p,unsigned length){unsigned sum=0;for(unsigned i=0;i<length;i+=2)sum+=(p[i]<<8)|(i+1<length?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return ~sum;}
static void arp(unsigned char*p,unsigned subnet){memset(p,0,42);memset(p,255,6);mac(p+6,subnet,0);p[12]=8;p[13]=6;p[15]=1;p[16]=8;p[18]=6;p[19]=4;p[21]=1;mac(p+22,subnet,0);ip(p+28,subnet,0);ip(p+38,subnet,1);}
static void protocol_datagram(unsigned char*p,unsigned destination,unsigned ttl,unsigned protocol){memset(p,0,66);mac(p,1,1);mac(p+6,1,0);p[12]=8;p[14]=0x45;p[17]=52;p[18]=0x12;p[19]=0x34;p[22]=ttl;p[23]=protocol;ip(p+26,1,0);ip(p+30,destination,0);unsigned sum=checksum(p+14,20);p[24]=sum>>8;p[25]=sum;p[34]=29001>>8;p[35]=29001&255;p[36]=29002>>8;p[37]=29002&255;p[39]=32;for(unsigned i=42;i<66;i++)p[i]=i*17;if(protocol==6){memset(p+38,0,16);p[46]=0x50;p[47]=0x18;p[48]=0x10;}else if(protocol==1){memset(p+34,0,8);p[34]=8;p[38]=0x12;p[39]=0x34;p[41]=1;}unsigned check;if(protocol==1){check=checksum(p+34,32);p[36]=check>>8;p[37]=check;}else{unsigned char pseudo[44]={0};memcpy(pseudo,p+26,8);pseudo[9]=protocol;pseudo[11]=32;memcpy(pseudo+12,p+34,32);check=checksum(pseudo,44);unsigned offset=protocol==17?40:50;p[offset]=check>>8;p[offset+1]=check;}}
static void datagram(unsigned char*p,unsigned destination,unsigned ttl){protocol_datagram(p,destination,ttl,17);}
static void setup(void*arg){(void)arg;for(unsigned i=0;i<3;i++){char address[20],address6[20];snprintf(address,sizeof(address),"10.%u.0.1",i+1);snprintf(address6,sizeof(address6),"fd%02u::1",i+1);unsigned char hardware[6];mac(hardware,i+1,1);interfaces[i]=lwip_host_interface_add(address,"255.255.255.0","0.0.0.0",address6,hardware);R(interfaces[i]);lwip_host_interface_up(interfaces[i]);}}
struct input{unsigned port,length;unsigned char bytes[66];};static void input(void*argument){struct input*i=argument;R(lwip_host_frame_input(interfaces[i->port-1],i->bytes,i->length)==0);}
static void inject(struct input*i){capture(i->bytes,i->length);R(lwip_host_call(input,i)==0);R(lwip_host_call(empty,NULL)==0);lwip_host_poll();R(lwip_host_call(empty,NULL)==0);}
static void down(void*arg){(void)arg;netif_set_link_down(interfaces[1]);netif_set_default(interfaces[2]);}
static void up(void*arg){(void)arg;netif_set_link_up(interfaces[1]);}
static void cleanup(void*arg){(void)arg;for(unsigned i=0;i<3;i++)lwip_host_interface_remove(interfaces[i]);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);const char*pcap_path=getenv("LWIP_ROUTING_PCAP");if(pcap_path){pcap=fopen(pcap_path,"wb");R(pcap);unsigned header[]={0xa1b2c3d4,0x00040002,0,0,65535,1};R(fwrite(header,sizeof(header),1,pcap)==1);}lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);struct input in;for(unsigned port=1;port<=3;port++){in.port=port;in.length=42;arp(in.bytes,port);inject(&in);}count=0;in.port=1;in.length=66;datagram(in.bytes,2,64);inject(&in);R(count==1&&ports[0]==2&&frames[0][22]==63);R(checksum(frames[0]+14,20)==0);R(memcmp(frames[0]+34,in.bytes+34,32)==0);count=0;
for(unsigned protocol=1;protocol<=6;protocol+=5){protocol_datagram(in.bytes,2,64,protocol);inject(&in);R(count==1&&ports[0]==2&&checksum(frames[0]+14,20)==0);R(memcmp(frames[0]+34,in.bytes+34,32)==0);count=0;}
unsigned char whole[66];datagram(whole,2,64);for(unsigned part=0;part<2;part++){memcpy(in.bytes,whole,34);memcpy(in.bytes+34,whole+34+part*16,16);in.length=50;in.bytes[17]=36;in.bytes[20]=part?0:0x20;in.bytes[21]=part?2:0;in.bytes[24]=in.bytes[25]=0;unsigned check=checksum(in.bytes+14,20);in.bytes[24]=check>>8;in.bytes[25]=check;inject(&in);R(count==1&&ports[0]==2&&checksum(frames[0]+14,20)==0);R(memcmp(frames[0]+34,in.bytes+34,16)==0);count=0;}in.length=66;datagram(in.bytes,2,1);inject(&in);R(count==1&&ports[0]==1&&frames[0][23]==1&&frames[0][34]==11&&frames[0][35]==0);R(checksum(frames[0]+34,lengths[0]-34)==0);count=0;datagram(in.bytes,1,64);inject(&in);R(count==0);R(lwip_host_call(down,NULL)==0);datagram(in.bytes,2,64);inject(&in);for(unsigned i=0;i<count;i++)R(ports[i]!=2);count=0;R(lwip_host_call(up,NULL)==0);in.port=2;in.length=42;arp(in.bytes,2);inject(&in);count=0;in.port=1;in.length=66;datagram(in.bytes,2,64);inject(&in);R(count==1&&ports[0]==2);R(lwip_host_call(cleanup,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
if(pcap)fclose(pcap);
printf("{\"case\":\"routing-ipv4\",\"interfaces\":3,\"ttl\":63,\"icmp_expired\":11,\"link_recovered\":true}\n");return 0;}
