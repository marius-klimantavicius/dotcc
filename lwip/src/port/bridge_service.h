#ifndef DOTCC_LWIP_BRIDGE_SERVICE_H
#define DOTCC_LWIP_BRIDGE_SERVICE_H
#include "lwip/netif.h"
/* Core-context only. Stop ingress and drain queued port callbacks before stop.
 * Handles belong to the current translated owner. Free requires successful stop. */
/* Non-core FIFO barrier; call before stop and between stop/free. */
int lwip_owned_bridge_barrier(void);
void *lwip_owned_bridge_new(const unsigned char *mac,unsigned ports,unsigned dynamic,unsigned fixed);
struct netif *lwip_owned_bridge_netif(void *handle);
int lwip_owned_bridge_add(void *handle,struct netif *port);
int lwip_owned_bridge_stop(void *handle);
int lwip_owned_bridge_free(void *handle);
#endif
