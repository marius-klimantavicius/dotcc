#ifndef LWIP_OWNED_LOWPAN_SERVICE_H
#define LWIP_OWNED_LOWPAN_SERVICE_H
#include "netif/zepif.h"
void *lwip_owned_lowpan_new(unsigned kind,const struct zepif_init *configuration);
struct netif *lwip_owned_lowpan_netif(void *handle);
int lwip_owned_lowpan_stop(void *handle);
int lwip_owned_lowpan_free(void *handle);
#endif
