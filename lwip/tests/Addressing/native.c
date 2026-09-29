#include "lwip/dns.h"
#include "lwip/udp.h"
#include "lwip/tcpip.h"
#include <stdatomic.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#define R(x) do{if(!(x)){fprintf(stderr,"FAIL %d %s\n",__LINE__,#x);exit(92);}}while(0)
extern void lwip_host_initialize(void(*)(void*),void*);
extern int lwip_host_stop_worker(void);
extern int lwip_host_call(void(*)(void*),void*);
extern void native_join_workers(void);
extern void lwip_host_poll(void);
static atomic_int ready,complete;static int missing,queries;static ip_addr_t answer;static struct udp_pcb*server;
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static void found(const char*name,const ip_addr_t*address,void*arg){(void)name;(void)arg;missing=address==NULL;if(address)answer=*address;atomic_store(&complete,1);}
static void query(void*arg,struct udp_pcb*pcb,struct pbuf*packet,const ip_addr_t*address,u16_t port){
 (void)arg;unsigned char response[512];unsigned length=packet->tot_len;R(length<450);R(pbuf_copy_partial(packet,response,length,0)==length);pbuf_free(packet);R(length>=17&&response[4]==0&&response[5]==1);unsigned cursor=12;while(cursor<length&&response[cursor]){unsigned n=response[cursor++];R(n<=63&&cursor+n<length);cursor+=n;}R(cursor+5==length);cursor++;unsigned type=((unsigned)response[cursor]<<8)|response[cursor+1];R(type==1||type==28);int absent=response[12]==7&&memcmp(response+13,"missing",7)==0;response[2]=0x81;response[3]=absent?0x83:0x80;response[6]=0;response[7]=absent?0:1;
 if(!absent){unsigned size=type==1?4:16;unsigned char tail[12]={0xc0,0x0c,0,0,0,1,0,0,0,60,0,0};tail[3]=type;tail[11]=size;memcpy(response+length,tail,12);length+=12;memset(response+length,0,size);if(type==1){unsigned char ip[4]={192,0,2,123};memcpy(response+length,ip,4);}else{response[length]=0x20;response[length+1]=1;response[length+2]=0x0d;response[length+3]=0xb8;response[length+15]=0x7b;}length+=size;}
 struct pbuf*output=pbuf_alloc(PBUF_TRANSPORT,length,PBUF_RAM);R(output);R(pbuf_take(output,response,length)==0);R(udp_sendto(pcb,output,address,port)==0);pbuf_free(output);queries++;
}
static void setup(void*unused){(void)unused;ip_addr_t address;R(ipaddr_aton("127.0.0.1",&address));server=udp_new();R(server);R(udp_bind(server,&address,53)==0);udp_recv(server,query,NULL);dns_setserver(0,&address);}
struct request {const char*name;const char*expected;u8_t type;};
static void submit(void*argument){struct request*r=argument;ip_addr_t out;R(dns_gethostbyname_addrtype(r->name,&out,found,NULL,r->type)==ERR_INPROGRESS);}
static void cached(void*argument){struct request*r=argument;ip_addr_t out,wanted;R(ipaddr_aton(r->expected,&wanted));R(ip_addr_cmp(&answer,&wanted));R(dns_gethostbyname_addrtype(r->name,&out,NULL,NULL,r->type)==ERR_OK);R(ip_addr_cmp(&out,&wanted));}
static void cleanup(void*unused){(void)unused;udp_remove(server);}
int main(int argc,char**argv){(void)argv;if(argc>1)R(0);lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}R(lwip_host_call(setup,NULL)==0);struct request cases[]={{"v4.example.test","192.0.2.123",LWIP_DNS_ADDRTYPE_IPV4},{"v6.example.test","2001:db8::7b",LWIP_DNS_ADDRTYPE_IPV6},{"missing.example.test",NULL,LWIP_DNS_ADDRTYPE_IPV4}};for(unsigned i=0;i<3;i++){atomic_store(&complete,0);R(lwip_host_call(submit,&cases[i])==0);unsigned attempts=0;while(!atomic_load(&complete)){R(++attempts<10000);lwip_host_poll();struct timespec t={0,1000000};nanosleep(&t,NULL);}R(missing==(cases[i].expected==NULL));if(cases[i].expected)R(lwip_host_call(cached,&cases[i])==0);R(queries==(int)i+1);printf("{\"case\":\"dns-%s\",\"queries\":%d,\"missing\":%d}\n",cases[i].name,queries,missing);}R(lwip_host_call(cleanup,NULL)==0);R(lwip_host_stop_worker()==0);
#if !NO_SYS
native_join_workers();
#endif
return 0;}
