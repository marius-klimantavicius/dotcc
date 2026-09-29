/* Authored port entrypoints. Protocol execution remains upstream lwIP. */
#include "lwip/init.h"
#include "lwip/tcpip.h"
#include "lwip/timeouts.h"
void lwip_host_initialize(void (*ready)(void *), void *argument)
{
#if NO_SYS
  lwip_init();
  ready(argument);
#else
  tcpip_init(ready, argument);
#endif
}
extern void lwip_host_request_worker_stop(void);
#if !NO_SYS
static void lwip_host_stop_callback(void *argument)
{
  LWIP_UNUSED_ARG(argument);
  lwip_host_request_worker_stop();
}
#endif
int lwip_host_stop_worker(void)
{
#if NO_SYS
  return 0;
#else
  return tcpip_callback(lwip_host_stop_callback, NULL);
#endif
}
#include "lwip/netif.h"
#include "lwip/etharp.h"
#include "lwip/ethip6.h"
#include "netif/ethernet.h"
#include <stdlib.h>
#include <string.h>
extern int lwip_host_frame_send(void *interface, const unsigned char *bytes, unsigned int length);
static err_t lwip_host_link_output(struct netif *interface,struct pbuf *p)
{
  unsigned char *bytes=malloc(p->tot_len);if(!bytes)return ERR_MEM;
  pbuf_copy_partial(p,bytes,p->tot_len,0);
  int result=lwip_host_frame_send(interface,bytes,p->tot_len);free(bytes);return (err_t)result;
}
static err_t lwip_host_netif_initialize(struct netif *interface)
{
  interface->name[0]='m';interface->name[1]='n';interface->mtu=1500;
  interface->hwaddr_len=6;interface->flags=NETIF_FLAG_BROADCAST|NETIF_FLAG_ETHARP|NETIF_FLAG_ETHERNET|NETIF_FLAG_IGMP|NETIF_FLAG_MLD6;
  interface->output=etharp_output;interface->output_ip6=ethip6_output;interface->linkoutput=lwip_host_link_output;return ERR_OK;
}
void *lwip_host_interface_add(const char *ipv4,const char *netmask,const char *gateway,const char *ipv6,const unsigned char *mac)
{
  struct netif *interface=calloc(1,sizeof(*interface));ip4_addr_t ip,mask,gw;
  if(!interface||!ip4addr_aton(ipv4,&ip)||!ip4addr_aton(netmask,&mask)||!ip4addr_aton(gateway,&gw)){free(interface);return NULL;}
  memcpy(interface->hwaddr,mac,6);
  if(!netif_add(interface,&ip,&mask,&gw,NULL,lwip_host_netif_initialize,
#if NO_SYS
  ethernet_input
#else
  tcpip_input
#endif
  )){free(interface);return NULL;}
  netif_set_default(interface);
  if(ipv6&&ipv6[0]){ip6_addr_t address;if(!ip6addr_aton(ipv6,&address)){netif_remove(interface);free(interface);return NULL;}netif_ip6_addr_set(interface,0,&address);netif_ip6_addr_set_state(interface,0,IP6_ADDR_PREFERRED);}
  return interface;
}
void lwip_host_interface_up(void *handle){struct netif*n=handle;netif_set_up(n);netif_set_link_up(n);}
/* The owning facade excludes new ingress, queues a FIFO barrier, stops on the
 * core, queues another FIFO barrier, then frees. Backing survives queued polls. */
void lwip_host_interface_stop(void *handle){struct netif*n=handle;netif_set_link_down(n);netif_set_down(n);
#if ENABLE_LOOPBACK
netif_poll(n);
#endif
netif_remove(n);}
void lwip_host_interface_free(void *handle){free(handle);}
/* Borrowed compatibility entrypoint: caller must already have excluded and
 * drained every queued input/poll callback and any pending loopback pbufs. */
void lwip_host_interface_remove(void *handle){struct netif*n=handle;netif_set_link_down(n);netif_set_down(n);netif_remove(n);free(n);}
#if !NO_SYS
static void lwip_host_barrier_callback(void *argument){sys_sem_signal((sys_sem_t*)argument);}
#endif
/* Must run outside the core lock/worker; callback_wait is not FIFO in locking mode. */
int lwip_host_barrier(void)
{
#if NO_SYS
  return ERR_OK;
#else
  sys_sem_t done;err_t result=sys_sem_new(&done,0);if(result!=ERR_OK)return result;
  result=tcpip_callback(lwip_host_barrier_callback,&done);
  if(result==ERR_OK)sys_sem_wait(&done);sys_sem_free(&done);return result;
#endif
}
int lwip_host_frame_input(void *handle,const unsigned char *bytes,unsigned int length)
{
  struct netif *interface=handle;if(length>65535)return ERR_VAL;
  struct pbuf *p=pbuf_alloc(PBUF_RAW,(u16_t)length,PBUF_POOL);if(!p)return ERR_MEM;
  err_t result=pbuf_take(p,bytes,(u16_t)length);if(result==ERR_OK)result=interface->input(p,interface);if(result!=ERR_OK)pbuf_free(p);return result;
}
int lwip_host_call(void (*callback)(void *),void *argument)
{
#if NO_SYS
  callback(argument);return 0;
#else
  return tcpip_callback_wait(callback,argument);
#endif
}
void lwip_host_poll(void)
{
#if NO_SYS
  netif_poll_all();sys_check_timeouts();
#endif
}
