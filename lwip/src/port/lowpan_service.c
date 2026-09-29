/* Owning link lifetime boundary. Include original algorithms; no protocol replacement.
 * The first include resolves to the same staged lowpan6.c as product/native inputs.
 * All entry points require a quiescent core boundary. */
#include "netif/lowpan6.c"
#include "netif/zepif.c"
#include "netif/lowpan6_ble.h"
#include <stdlib.h>

struct lowpan_service {
  struct lowpan_service *next;
  struct netif interface;
  ip_addr_t local,remote;
  unsigned kind,stopped;
};
static struct lowpan_service *lowpan_services;
static unsigned lowpan_timer_users;

static struct lowpan_service *lowpan_find(void *handle) {
  struct lowpan_service *item;
  for(item=lowpan_services;item;item=item->next)if(item==handle)return item;
  return NULL;
}
static int lowpan_timer_acquire(void) {
  if(!lowpan_timer_users) {
    /* An unregistered raw zepif already owns this timer: do not adopt it. */
    if(zep_lowpan_timer_running)return ERR_USE;
    zep_lowpan_timer_running=1;
    sys_timeout(LOWPAN6_TMR_INTERVAL,zep_lowpan_timer,NULL);
  }
  lowpan_timer_users++;return ERR_OK;
}
static void lowpan_timer_release(void) {
  LWIP_ASSERT("balanced lowpan service timer",lowpan_timer_users);
  if(--lowpan_timer_users)return;
  zep_lowpan_timer_running=0;sys_untimeout(zep_lowpan_timer,NULL);
  /* No registered interface or borrowed raw user remains. The original
   * global reassembly list may now be discarded without harming a live user. */
  while(lowpan6_data.reass_list) {
    struct lowpan6_reass_helper *item=lowpan6_data.reass_list;
    lowpan6_data.reass_list=item->next_packet;free_reass_datagram(item);
  }
}
/* kind0 owns a direct IEEE802.15.4 netif, kind1 a ZEP netif,
 * kind2 retains the timer for caller-managed raw users; kind3 is RFC7668 BLE
 * with separate original context/address state and no radio reassembly timer. */
void *lwip_owned_lowpan_new(unsigned kind,const struct zepif_init *configuration) {
  LWIP_ASSERT_CORE_LOCKED();if(kind>3)return NULL;
  struct lowpan_service *item=calloc(1,sizeof(*item));if(!item)return NULL;
  if(kind!=3&&lowpan_timer_acquire()!=ERR_OK){free(item);return NULL;}
  item->kind=kind;
  if(kind!=2) {
    struct zepif_init init={0};if(configuration)init=*configuration;
    if(init.zep_src_ip_addr){item->local=*init.zep_src_ip_addr;init.zep_src_ip_addr=&item->local;}
    if(init.zep_dst_ip_addr){item->remote=*init.zep_dst_ip_addr;init.zep_dst_ip_addr=&item->remote;}
    if(!netif_add(&item->interface,NULL,NULL,NULL,kind==1?&init:NULL,kind==1?zepif_init:kind==3?rfc7668_if_init:lowpan6_if_init,kind==3?rfc7668_input:lowpan6_input)) {
      if(kind!=3)lowpan_timer_release();free(item);return NULL;
    }
  }
  item->next=lowpan_services;lowpan_services=item;return item;
}
struct netif *lwip_owned_lowpan_netif(void *handle) {
  LWIP_ASSERT_CORE_LOCKED();struct lowpan_service *item=lowpan_find(handle);
  return item&&!item->stopped&&item->kind!=2?&item->interface:NULL;
}
int lwip_owned_lowpan_stop(void *handle) {
  LWIP_ASSERT_CORE_LOCKED();struct lowpan_service **link=&lowpan_services;
  while(*link&&*link!=handle)link=&(*link)->next;
  if(!*link)return ERR_ARG;struct lowpan_service *item=*link;if(item->stopped)return ERR_OK;
  if(item->kind!=2) {
    netif_set_down(&item->interface);
#if ENABLE_LOOPBACK
    netif_poll(&item->interface);
#endif
    if(item->kind==1) {
      struct zepif_state *state=item->interface.state;
      udp_recv(state->pcb,NULL,NULL);udp_remove(state->pcb);mem_free(state);item->interface.state=NULL;
    }
    netif_remove(&item->interface);
  }
  if(item->kind!=3)lowpan_timer_release();item->stopped=1;return ERR_OK;
}

/* Caller must run a FIFO core barrier after stop before freeing netif backing. */
int lwip_owned_lowpan_free(void *handle) {
  LWIP_ASSERT_CORE_LOCKED();struct lowpan_service **link=&lowpan_services;
  while(*link&&*link!=handle)link=&(*link)->next;
  if(!*link)return ERR_ARG;struct lowpan_service *item=*link;
  if(!item->stopped)return ERR_USE;*link=item->next;free(item);return ERR_OK;
}
