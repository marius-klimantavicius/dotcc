/* Apply only to native fixture CALL SITES, never to the original source wrapper.
 * Managed generation routes the original tftp_put entry through a typed override.
 * Native comparison invokes exactly the same small authored entry helper.
 */
#ifndef DOTCC_UDP_NATIVE_PRODUCT_ADAPTER_H
#define DOTCC_UDP_NATIVE_PRODUCT_ADAPTER_H
#define DOTCC_UDP_PRODUCT_BOUNDARIES 1
#include "udp_app_services.h"
#define tftp_put lwip_host_tftp_put
#endif
