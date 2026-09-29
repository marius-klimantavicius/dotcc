#include "lwip/netif.h"
#include "lwip/tcpip.h"
#include "lwip/ip6.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern int lwip_host_call(void(*)(void*),void*);extern void native_join_workers(void);extern void lwip_host_poll(void);
extern void*lwip_host_interface_add(const char*,const char*,const char*,const char*,const unsigned char*);extern void lwip_host_interface_up(void*);extern void lwip_host_interface_remove(void*);extern int lwip_host_frame_input(void*,const unsigned char*,unsigned);
static struct netif*interfaces[3];static unsigned char frames[128][2048];static unsigned ports[128],lengths[128],count;static atomic_int ready;
int lwip_host_frame_send(void*n,const unsigned char*b,unsigned len){unsigned port=0;for(unsigned i=0;i<3;i++)if(interfaces[i]==n)port=i+1;if(!port)return -12;R(count<128&&len<=2048);ports[count]=port;lengths[count]=len;memcpy(frames[count++],b,len);return 0;}
static void initialized(void*unused){(void)unused;atomic_store(&ready,1);}
static unsigned short checksum(const unsigned char*p,unsigned n){unsigned sum=0;for(unsigned i=0;i<n;i+=2)sum+=(p[i]<<8)|(i+1<n?p[i+1]:0);while(sum>>16)sum=(sum&65535)+(sum>>16);return ~sum;}
static void u16(unsigned char*p,unsigned n){p[0]=n>>8;p[1]=n;}
static void u32(unsigned char*p,unsigned n){p[0]=n>>24;p[1]=n>>16;p[2]=n>>8;p[3]=n;}
static void ip(unsigned char*p,unsigned subnet,unsigned host){memset(p,0,16);p[0]=0xfd;p[1]=subnet;p[15]=host;}
static void mac(unsigned char*p,unsigned subnet,unsigned host){memset(p,0,6);p[0]=2;p[4]=subnet;p[5]=host;}
static void checksum6(unsigned char*f,unsigned offset){unsigned n=(f[18]<<8)|f[19];unsigned char pseudo[2048]={0};R(n+40<=2048);memcpy(pseudo,f+22,32);u32(pseudo+32,n);pseudo[39]=f[20];memcpy(pseudo+40,f+54,n);unsigned sum=checksum(pseudo,n+40);u16(f+54+offset,sum?sum:65535);}
static int valid_checksum6(const unsigned char*f){unsigned n=(f[18]<<8)|f[19];unsigned char pseudo[2048]={0};R(n+40<=2048);memcpy(pseudo,f+22,32);u32(pseudo+32,n);pseudo[39]=f[20];memcpy(pseudo+40,f+54,n);return checksum(pseudo,n+40)==0;}
struct input{unsigned port,length;unsigned char bytes[2048];};
static void packet(struct input*out,unsigned destination,unsigned hop,unsigned protocol,unsigned length){R(length>=32&&length+54<=2048);unsigned char*f=out->bytes;memset(f,0,length+54);out->port=1;out->length=length+54;mac(f,1,1);mac(f+6,1,2);f[12]=0x86;f[13]=0xdd;f[14]=0x60;u16(f+18,length);f[20]=protocol;f[21]=hop;ip(f+22,1,2);ip(f+38,destination,2);for(unsigned i=8;i<length;i++)f[54+i]=i*17;if(protocol==17){u16(f+54,29001);u16(f+56,29002);u16(f+58,length);checksum6(f,6);}else if(protocol==6){memset(f+54,0,20);u16(f+54,29001);u16(f+56,29002);f[66]=0x50;f[67]=0x18;f[68]=0x10;checksum6(f,16);}else{f[54]=128;f[58]=0x12;f[59]=0x34;f[61]=1;checksum6(f,2);}}
static void neighbor(struct input*out,unsigned subnet){unsigned char*f=out->bytes;memset(f,0,86);out->port=subnet;out->length=86;f[0]=f[1]=0x33;f[2]=0xff;f[5]=1;mac(f+6,subnet,2);f[12]=0x86;f[13]=0xdd;f[14]=0x60;f[19]=32;f[20]=58;f[21]=255;ip(f+22,subnet,2);f[38]=0xff;f[39]=2;f[49]=1;f[50]=0xff;f[53]=1;f[54]=135;ip(f+62,subnet,1);f[78]=1;f[79]=1;mac(f+80,subnet,2);checksum6(f,2);}
static void setup(void*unused){(void)unused;for(unsigned i=0;i<3;i++){char a[24],v6[24];snprintf(a,sizeof(a),"10.%u.0.1",i+1);snprintf(v6,sizeof(v6),"fd%02u::1",i+1);unsigned char hardware[6];mac(hardware,i+1,1);interfaces[i]=lwip_host_interface_add(a,"255.255.255.0","0.0.0.0",v6,hardware);R(interfaces[i]);lwip_host_interface_up(interfaces[i]);}}
static void input(void*arg){struct input*p=arg;R(lwip_host_frame_input(interfaces[p->port-1],p->bytes,p->length)==0);}
static void empty(void*unused){(void)unused;}
static void inject(struct input*p){R(lwip_host_call(input,p)==0);R(lwip_host_call(empty,NULL)==0);lwip_host_poll();R(lwip_host_call(empty,NULL)==0);}
static void mtu(void*unused){(void)unused;interfaces[1]->mtu=1280;}
static void down(void*unused){(void)unused;netif_set_link_down(interfaces[1]);netif_set_default(interfaces[2]);}
static void up(void*unused){(void)unused;netif_set_link_up(interfaces[1]);}
static void no_default(void*unused){(void)unused;netif_set_default(NULL);}
static void cleanup(void*unused){(void)unused;for(unsigned i=0;i<3;i++)lwip_host_interface_remove(interfaces[i]);}
int main(int argc,char**argv){int hop_test=argc>1&&(!strcmp(argv[1],"--zero-hop")||!strncmp(argv[1],"--hop=",6));unsigned tested_hop=hop_test&&!strncmp(argv[1],"--hop=",6)?(unsigned)atoi(argv[1]+6):0;if(argc>1&&!hop_test)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);struct input in;for(unsigned i=1;i<=3;i++){neighbor(&in,i);inject(&in);}count=0;
if(hop_test){packet(&in,2,tested_hop,17,32);inject(&in);fprintf(stderr,"hop-test output count=%u port=%u protocol=%u hop=%u\n",count,count?ports[0]:0,count?frames[0][20]:0,count?frames[0][21]:0);R(count==1);if(tested_hop<=1){R(ports[0]==1&&frames[0][20]==58&&frames[0][54]==3);}else{R(ports[0]==2&&frames[0][20]==17&&frames[0][21]==tested_hop-1);}R(valid_checksum6(frames[0]));printf("{\"case\":\"forward-hop-limit\",\"input\":%u,\"output_port\":%u,\"output_protocol\":%u,\"output_hop\":%u}\n",tested_hop,ports[0],frames[0][20],frames[0][21]);return 0;}
for(unsigned protocol=6;protocol<=58;protocol=protocol==6?17:protocol==17?58:59){packet(&in,2,64,protocol,32);inject(&in);R(count==1&&ports[0]==2&&frames[0][21]==63);R(valid_checksum6(frames[0]));R(!memcmp(frames[0]+54,in.bytes+54,32));count=0;}
unsigned char payload[32];packet(&in,2,64,17,32);memcpy(payload,in.bytes+54,32);for(unsigned part=0;part<2;part++){packet(&in,2,64,17,32);in.bytes[20]=44;u16(in.bytes+18,24);in.length=78;memset(in.bytes+54,0,8);in.bytes[54]=17;u16(in.bytes+56,part?16:1);u32(in.bytes+58,0x12345678);memcpy(in.bytes+62,payload+16*part,16);inject(&in);R(count==1&&ports[0]==2&&frames[0][21]==63);R(!memcmp(frames[0]+54,in.bytes+54,24));count=0;}
packet(&in,2,1,17,32);inject(&in);R(count==1&&ports[0]==1&&frames[0][20]==58&&frames[0][54]==3&&frames[0][55]==0&&valid_checksum6(frames[0]));count=0;R(lwip_host_call(mtu,NULL)==0);packet(&in,2,64,17,1320);inject(&in);R(count==1&&ports[0]==1&&frames[0][54]==2&&frames[0][55]==0);R(frames[0][58]==0&&frames[0][59]==0&&frames[0][60]==5&&frames[0][61]==0&&valid_checksum6(frames[0]));count=0;packet(&in,1,64,17,32);in.bytes[53]=3;inject(&in);R(count==0);packet(&in,2,64,17,32);memset(in.bytes+22,0,16);in.bytes[22]=0xfe;in.bytes[23]=0x80;in.bytes[37]=2;inject(&in);R(count==0);packet(&in,2,64,17,32);memset(in.bytes+38,0,16);in.bytes[38]=0xfe;in.bytes[39]=0x80;in.bytes[53]=2;inject(&in);R(count==0);R(lwip_host_call(down,NULL)==0);packet(&in,2,64,17,32);inject(&in);for(unsigned i=0;i<count;i++)R(ports[i]!=2);count=0;R(lwip_host_call(up,NULL)==0);neighbor(&in,2);inject(&in);count=0;packet(&in,2,64,17,32);inject(&in);R(count==1&&ports[0]==2);count=0;R(lwip_host_call(no_default,NULL)==0);packet(&in,99,64,17,32);inject(&in);R(count==1&&ports[0]==1&&frames[0][54]==1&&frames[0][55]==0&&valid_checksum6(frames[0]));R(lwip_host_call(cleanup,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
puts("{\"case\":\"ipv6-forwarding-hop-mtu-fragments-zones-link-recovery\"}");return 0;}
