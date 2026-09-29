#include "lwip/init.h"
#include "lwip/tcp.h"
#include "lwip/netif.h"
#include "lwip/ip_addr.h"
#include "addons/tcp_md5/tcp_md5.h"
#include "addons/tcp_isn/tcp_isn.h"
#include "addons/ipv6_static_routing/ip6_route_table.h"
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#define REQUIRE(x) do { if (!(x)) { fprintf(stderr,"FAIL %s:%d %s\n",__FILE__,__LINE__,#x); exit(92); } } while (0)
static unsigned destroyed;
u32_t sys_now(void) { return 1000; }
static void destroy(u8_t id,void *arg) { REQUIRE(id==1 && arg==&destroyed); destroyed++; }
static const struct tcp_ext_arg_callbacks callbacks={destroy,NULL};
int main(int argc,char **argv)
{
  if(argc>1 && !strcmp(argv[1],"--negative")) REQUIRE(0);
  printf("{\"abi\":{\"netif\":%zu,\"tcp_pcb\":%zu,\"tcp_ext_args_offset\":%zu,\"tcp_ext_arg_callbacks\":%zu,\"ip6_prefix\":%zu,\"ip6_route_entry\":%zu,\"tcp_md5sig\":%zu},",
         sizeof(struct netif),sizeof(struct tcp_pcb),offsetof(struct tcp_pcb,ext_args),sizeof(struct tcp_ext_arg_callbacks),sizeof(struct ip6_prefix),sizeof(struct ip6_route_entry),sizeof(struct tcp_md5sig));
  lwip_init();tcp_md5_init();
  u8_t id=tcp_ext_arg_alloc_id();REQUIRE(id==1);
  struct tcp_pcb *pcb=tcp_new();REQUIRE(pcb);
  tcp_ext_arg_set_callbacks(pcb,id,&callbacks);tcp_ext_arg_set(pcb,id,&destroyed);
  REQUIRE(tcp_md5_get_additional_option_length(pcb,4)==4);tcp_abort(pcb);REQUIRE(destroyed==1);
  struct netif iface={0}; struct ip6_prefix broad={0},specific={0},bad={0};
  REQUIRE(ip6addr_aton("2001:db8::",&broad.addr));broad.prefix_len=32;specific=broad;specific.prefix_len=64;
  ip6_addr_t target;REQUIRE(ip6addr_aton("2001:db8::1234",&target));s8_t idx=-1;
  REQUIRE(ip6_add_route_entry(&broad,&iface,NULL,&idx)==ERR_OK);
  REQUIRE(ip6_add_route_entry(&specific,&iface,&broad.addr,&idx)==ERR_OK);
  REQUIRE(ip6_find_route_entry(&target)==0 && ip6_static_route(NULL,&target)==&iface);
  REQUIRE(ip6_get_gateway(&iface,&target)==&broad.addr);
  bad=broad;bad.prefix_len=33;REQUIRE(ip6_add_route_entry(&bad,&iface,NULL,NULL)==ERR_ARG);
  ip6_remove_route_entry(&specific);REQUIRE(ip6_find_route_entry(&target)==0);
  ip6_remove_route_entry(&broad);REQUIRE(ip6_find_route_entry(&target)==0 && ip6_static_route(NULL,&target)==NULL);
  u8_t secret[16];for(unsigned i=0;i<16;i++)secret[i]=(u8_t)(i*17);
  ip_addr_t local,remote;REQUIRE(ipaddr_aton("192.0.2.1",&local));REQUIRE(ipaddr_aton("198.51.100.2",&remote));
  lwip_init_tcp_isn(7,secret);u32_t v4=lwip_hook_tcp_isn(&local,1234,&remote,4321);
  REQUIRE(ipaddr_aton("2001:db8::1",&local));REQUIRE(ipaddr_aton("2001:db8::2",&remote));
  u32_t v6=lwip_hook_tcp_isn(&local,1234,&remote,4321);
  printf("\"isn4\":%u,\"isn6\":%u,\"destroyed\":%u,\"route_cases\":7}\n",v4,v6,destroyed);
  return 0;
}
