#include "lwip/init.h"
#include "lwip/tcp.h"
#include "lwip/udp.h"
#include "lwip/netif.h"
#include "lwip/timeouts.h"
#include "lwip/sockets.h"
#include "lwip/netbuf.h"
#include "lwip/inet_chksum.h"
#include "lwip/prot/ethernet.h"
#include "lwip/prot/ip4.h"
#include "lwip/prot/ip6.h"
#include "lwip/prot/tcp.h"
#include "lwip/prot/udp.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stddef.h>
#if !NO_SYS
#include <pthread.h>
#include <stdatomic.h>
#endif
#define REQUIRE(v) do{if(!(v)){fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#v);exit(92);}}while(0)
#define TOTAL (256*1024)
static unsigned char data[4096];
static unsigned pattern(unsigned i){return (i*31u+(i>>8))&255;}
static void abi(void)
{
  printf("{\"abi\":{\"pbuf\":%zu,\"pbuf_payload\":%zu,\"netif\":%zu,\"tcp_pcb\":%zu,\"ip_addr\":%zu,\"eth_hdr\":%zu,\"ip_hdr\":%zu,\"ip6_hdr\":%zu,\"tcp_hdr\":%zu,\"udp_hdr\":%zu",sizeof(struct pbuf),offsetof(struct pbuf,payload),sizeof(struct netif),sizeof(struct tcp_pcb),sizeof(ip_addr_t),sizeof(struct eth_hdr),sizeof(struct ip_hdr),sizeof(struct ip6_hdr),sizeof(struct tcp_hdr),sizeof(struct udp_hdr));
#if !NO_SYS
  printf(",\"sockaddr_in\":%zu,\"sockaddr_in6\":%zu,\"sockaddr_storage\":%zu,\"fd_set\":%zu,\"netbuf\":%zu,\"netbuf_addr\":%zu,\"netbuf_port\":%zu,\"sockaddr_in_addr\":%zu,\"sockaddr_in6_addr\":%zu",sizeof(struct sockaddr_in),sizeof(struct sockaddr_in6),sizeof(struct sockaddr_storage),sizeof(fd_set),sizeof(struct netbuf),offsetof(struct netbuf,addr),offsetof(struct netbuf,port),offsetof(struct sockaddr_in,sin_addr),offsetof(struct sockaddr_in6,sin6_addr));
#endif
  printf("}}\n");
  unsigned char bytes[18];for(unsigned i=0;i<18;i++)bytes[i]=(unsigned char)(i*17+3);
  struct pbuf tail={0},head={0};head.next=&tail;head.payload=bytes+1;head.len=7;head.tot_len=15;tail.payload=bytes+8;tail.len=tail.tot_len=8;
  printf("{\"checksums\":{\"odd_address_odd_length\":%u,\"odd_chain\":%u}}\n",inet_chksum(bytes+1,15),inet_chksum_pbuf(&head));
}

#if NO_SYS
static unsigned sent,received,acknowledged,udp_received;static int connected;
static err_t recv_tcp(void*arg,struct tcp_pcb*pcb,struct pbuf*p,err_t err){(void)arg;REQUIRE(err==ERR_OK);if(!p)return tcp_close(pcb);for(struct pbuf*q=p;q;q=q->next)for(unsigned i=0;i<q->len;i++)REQUIRE(((unsigned char*)q->payload)[i]==pattern(received++));tcp_recved(pcb,p->tot_len);pbuf_free(p);return ERR_OK;}
static err_t accepted(void*arg,struct tcp_pcb*pcb,err_t err){(void)arg;REQUIRE(err==ERR_OK);tcp_recv(pcb,recv_tcp);return ERR_OK;}
static err_t sent_tcp(void*arg,struct tcp_pcb*pcb,u16_t n){(void)arg;(void)pcb;acknowledged+=n;return ERR_OK;}
static err_t connected_tcp(void*arg,struct tcp_pcb*pcb,err_t err){(void)arg;REQUIRE(err==ERR_OK);connected=1;tcp_sent(pcb,sent_tcp);return ERR_OK;}
static void recv_udp(void*arg,struct udp_pcb*pcb,struct pbuf*p,const ip_addr_t*addr,u16_t port){(void)arg;(void)pcb;(void)addr;(void)port;REQUIRE(p->tot_len==123);unsigned char b[123];REQUIRE(pbuf_copy_partial(p,b,123,0)==123);for(unsigned i=0;i<123;i++)REQUIRE(b[i]==pattern(i));udp_received++;pbuf_free(p);}
static void pump(void){netif_poll_all();sys_check_timeouts();}
static void raw_case(int v6){ip_addr_t addr;REQUIRE(ipaddr_aton(v6?"::1":"127.0.0.1",&addr));sent=received=acknowledged=udp_received=0;connected=0;struct tcp_pcb*listener=tcp_new_ip_type(v6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);REQUIRE(listener);REQUIRE(tcp_bind(listener,&addr,12345)==ERR_OK);listener=tcp_listen(listener);REQUIRE(listener);tcp_accept(listener,accepted);struct tcp_pcb*client=tcp_new_ip_type(v6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);REQUIRE(tcp_connect(client,&addr,12345,connected_tcp)==ERR_OK);u32_t end=sys_now()+10000;while((!connected||acknowledged<TOTAL)&& (int)(end-sys_now())>0){pump();if(connected&&sent<TOTAL){unsigned n=TOTAL-sent;if(n>sizeof(data))n=sizeof(data);if(n>tcp_sndbuf(client))n=tcp_sndbuf(client);if(n){for(unsigned i=0;i<n;i++)data[i]=(unsigned char)pattern(sent+i);err_t e=tcp_write(client,data,(u16_t)n,TCP_WRITE_FLAG_COPY);REQUIRE(e==ERR_OK||e==ERR_MEM);if(e==ERR_OK){sent+=n;tcp_output(client);}}}}REQUIRE(received==TOTAL&&acknowledged==TOTAL);REQUIRE(tcp_close(client)==ERR_OK);for(int i=0;i<100;i++)pump();REQUIRE(tcp_close(listener)==ERR_OK);struct udp_pcb*u=udp_new_ip_type(v6?IPADDR_TYPE_V6:IPADDR_TYPE_V4);REQUIRE(u);REQUIRE(udp_bind(u,&addr,12346)==ERR_OK);udp_recv(u,recv_udp,NULL);struct pbuf*p=pbuf_alloc(PBUF_TRANSPORT,123,PBUF_RAM);REQUIRE(p);for(unsigned i=0;i<123;i++)data[i]=(unsigned char)pattern(i);REQUIRE(pbuf_take(p,data,123)==ERR_OK);REQUIRE(udp_sendto(u,p,&addr,12346)==ERR_OK);pbuf_free(p);for(int i=0;i<100&&!udp_received;i++)pump();REQUIRE(udp_received==1);udp_remove(u);printf("{\"case\":\"raw-ipv%d\",\"tcp_bytes\":%u,\"udp_bytes\":123}\n",v6?6:4,received);}
#else
extern void lwip_host_initialize(void(*)(void*),void*);extern int lwip_host_stop_worker(void);extern void native_join_workers(void);
static atomic_int ready;
static void initialized(void*arg){(void)arg;atomic_store(&ready,1);}
static void make_address(struct sockaddr_storage*s,int v6,int port){memset(s,0,sizeof(*s));if(v6){struct sockaddr_in6*a=(void*)s;a->sin6_family=AF_INET6;a->sin6_port=lwip_htons(port);a->sin6_addr.s6_addr[15]=1;}else{struct sockaddr_in*a=(void*)s;a->sin_family=AF_INET;a->sin_port=lwip_htons(port);a->sin_addr.s_addr=lwip_htonl(0x7f000001);}}
static void*server(void*arg){int listener=*(int*)arg;lwip_socket_thread_init();int accepted=lwip_accept(listener,NULL,NULL);REQUIRE(accepted>=0);unsigned received=0;unsigned char buffer[4096];while(received<TOTAL){int n=lwip_recv(accepted,buffer,sizeof(buffer),0);REQUIRE(n>0);for(int i=0;i<n;i++)REQUIRE(buffer[i]==pattern(received++));}REQUIRE(lwip_shutdown(accepted,SHUT_WR)==0);REQUIRE(lwip_close(accepted)==0);lwip_socket_thread_cleanup();return NULL;}
static void socket_case(int v6){int family=v6?AF_INET6:AF_INET;struct sockaddr_storage address;make_address(&address,v6,12345);socklen_t length=v6?sizeof(struct sockaddr_in6):sizeof(struct sockaddr_in);int listener=lwip_socket(family,SOCK_STREAM,0);REQUIRE(listener>=0);REQUIRE(lwip_bind(listener,(struct sockaddr*)&address,length)==0);REQUIRE(lwip_listen(listener,2)==0);pthread_t peer;REQUIRE(pthread_create(&peer,NULL,server,&listener)==0);int client=lwip_socket(family,SOCK_STREAM,0);REQUIRE(client>=0);REQUIRE(lwip_connect(client,(struct sockaddr*)&address,length)==0);unsigned sent=0;while(sent<TOTAL){unsigned n=TOTAL-sent;if(n>sizeof(data))n=sizeof(data);for(unsigned i=0;i<n;i++)data[i]=(unsigned char)pattern(sent+i);int done=lwip_send(client,data,n,0);REQUIRE(done>0);sent+=(unsigned)done;}REQUIRE(lwip_recv(client,data,sizeof(data),0)==0);REQUIRE(lwip_close(client)==0);pthread_join(peer,NULL);REQUIRE(lwip_close(listener)==0);int udp=lwip_socket(family,SOCK_DGRAM,0);REQUIRE(udp>=0);make_address(&address,v6,12346);REQUIRE(lwip_bind(udp,(struct sockaddr*)&address,length)==0);for(unsigned i=0;i<123;i++)data[i]=(unsigned char)pattern(i);REQUIRE(lwip_sendto(udp,data,123,0,(struct sockaddr*)&address,length)==123);memset(data,0,123);REQUIRE(lwip_recv(udp,data,sizeof(data),0)==123);for(unsigned i=0;i<123;i++)REQUIRE(data[i]==pattern(i));REQUIRE(lwip_close(udp)==0);printf("{\"case\":\"socket-ipv%d\",\"tcp_bytes\":%u,\"udp_bytes\":123}\n",v6?6:4,sent);}
#endif
int main(int argc,char**argv){if(argc>1&&!strcmp(argv[1],"--negative")){REQUIRE(0);}abi();
#if NO_SYS
lwip_init();raw_case(0);raw_case(1);
#else
lwip_host_initialize(initialized,NULL);while(!atomic_load(&ready)){struct timespec t={0,1000000};nanosleep(&t,NULL);}lwip_socket_thread_init();socket_case(0);socket_case(1);lwip_socket_thread_cleanup();REQUIRE(lwip_host_stop_worker()==ERR_OK);native_join_workers();
#endif
return 0;}
