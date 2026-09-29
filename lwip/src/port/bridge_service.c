/* Authored lifecycle boundary: original forwarding and FDB algorithms remain intact.
 * Call only on the core after stopping ingress and draining queued input callbacks. */
#include "netif/bridgeif.c"
#include "netif/bridgeif_fdb.c"
#include "netif/ethernet.h"
#include <stdlib.h>
#include "lwip/tcpip.h"
#if !NO_SYS
static void bridge_barrier_callback(void *argument) {sys_sem_signal((sys_sem_t*)argument);}
#endif
/* Caller must not hold the core lock or run on the core worker. */
int lwip_owned_bridge_barrier(void) {
#if NO_SYS
  return ERR_OK;
#else
  sys_sem_t done;err_t result=sys_sem_new(&done,0);if(result!=ERR_OK)return result;
  result=tcpip_callback(bridge_barrier_callback,&done);
  if(result==ERR_OK)sys_sem_wait(&done);sys_sem_free(&done);return result;
#endif
}

struct saved_port {struct netif *netif;netif_input_fn input;void *client;u8_t etharp;};
struct owned_bridge {
  struct owned_bridge *next;
  struct netif interface;
  struct saved_port ports[BRIDGEIF_MAX_PORTS];
  u8_t count,closed;
};
static struct owned_bridge *owned_bridges;
static struct owned_bridge *find_bridge(void *handle) {
  struct owned_bridge *item;
  for(item=owned_bridges;item;item=item->next)if(item==handle)return item;
  return NULL;
}
void *lwip_owned_bridge_new(const unsigned char *mac,unsigned ports,unsigned dynamic,unsigned fixed) {
  LWIP_ASSERT_CORE_LOCKED();
  if(!mac||!ports||ports>BRIDGEIF_MAX_PORTS||dynamic>65535||fixed>65535)return NULL;
  struct owned_bridge *item=calloc(1,sizeof(*item));if(!item)return NULL;
  bridgeif_initdata_t init;memcpy(init.ethaddr.addr,mac,6);init.max_ports=(u8_t)ports;
  init.max_fdb_dynamic_entries=(u16_t)dynamic;init.max_fdb_static_entries=(u16_t)fixed;
  if(!netif_add(&item->interface,NULL,NULL,NULL,&init,bridgeif_init,ethernet_input)){free(item);return NULL;}
  item->next=owned_bridges;owned_bridges=item;netif_set_up(&item->interface);return item;
}
struct netif *lwip_owned_bridge_netif(void *handle) {
  LWIP_ASSERT_CORE_LOCKED();struct owned_bridge *item=find_bridge(handle);
  return item&&!item->closed?&item->interface:NULL;
}
int lwip_owned_bridge_add(void *handle,struct netif *port) {
  LWIP_ASSERT_CORE_LOCKED();struct owned_bridge *item=find_bridge(handle);struct netif *local;
  if(!item||item->closed||!port||port==&item->interface)return ERR_ARG;
  for(local=netif_list;local&&local!=port;local=local->next){}
  if(!local)return ERR_ARG;
  for(unsigned i=0;i<item->count;i++)if(item->ports[i].netif==port)return ERR_USE;
  bridgeif_private_t *br=item->interface.state;
  if(item->count>=br->max_ports)return ERR_VAL;
  struct saved_port saved={port,port->input,netif_get_client_data(port,bridgeif_netif_client_id),(u8_t)(port->flags&NETIF_FLAG_ETHARP)};
  if(saved.client)return ERR_USE;
  err_t result=bridgeif_add_port(&item->interface,port);
  if(result==ERR_OK)item->ports[item->count++]=saved;
  return result;
}
int lwip_owned_bridge_stop(void *handle) {
  LWIP_ASSERT_CORE_LOCKED();struct owned_bridge *item=find_bridge(handle);
  if(!item)return ERR_ARG;if(item->closed)return ERR_OK;
  bridgeif_private_t *br=item->interface.state;
  /* Validate every attachment before mutating any of them, allowing repair/retry. */
  for(unsigned i=0;i<item->count;i++) {
    struct saved_port *saved=&item->ports[i];
#if BRIDGEIF_PORT_NETIFS_OUTPUT_DIRECT
    if(saved->netif->input!=bridgeif_input)return ERR_USE;
#else
    if(saved->netif->input!=bridgeif_tcpip_input)return ERR_USE;
#endif
    if(netif_get_client_data(saved->netif,bridgeif_netif_client_id)!=&br->ports[i])return ERR_USE;
  }
  netif_set_down(&item->interface);
#if ENABLE_LOOPBACK
  netif_poll(&item->interface);
#endif
  netif_remove(&item->interface);
  for(unsigned i=0;i<item->count;i++) {
    struct saved_port *saved=&item->ports[i];saved->netif->input=saved->input;
    netif_set_client_data(saved->netif,bridgeif_netif_client_id,saved->client);
    saved->netif->flags=(u8_t)((saved->netif->flags&~NETIF_FLAG_ETHARP)|saved->etharp);
  }
  sys_untimeout(bridgeif_age_tmr,br->fdbd);mem_free(br->fdbd);mem_free(br);
  item->interface.state=NULL;item->closed=1;return ERR_OK;
}
int lwip_owned_bridge_free(void *handle) {
  LWIP_ASSERT_CORE_LOCKED();struct owned_bridge **link=&owned_bridges;
  while(*link&&*link!=handle)link=&(*link)->next;
  if(!*link)return ERR_ARG;struct owned_bridge *item=*link;if(!item->closed)return ERR_USE;
  *link=item->next;free(item);return ERR_OK;
}
