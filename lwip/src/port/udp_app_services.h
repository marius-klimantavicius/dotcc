#ifndef DOTCC_LWIP_UDP_APP_SERVICES_H
#define DOTCC_LWIP_UDP_APP_SERVICES_H
#include "lwip/apps/tftp_client.h"
#include "lwip/apps/tftp_server.h"

/* Raw NTP wire-era seconds and fraction, in host byte order. No Unix conversion. */
void lwip_host_sntp_time(unsigned int seconds,unsigned int fraction);
#ifndef SNTP_SET_SYSTEM_TIME_NTP
#define SNTP_SET_SYSTEM_TIME_NTP(seconds,fraction) \
  lwip_host_sntp_time((unsigned int)(seconds),(unsigned int)(fraction))
#endif

err_t lwip_host_tftp_put(void *handle,const ip_addr_t *addr,u16_t port,const char *fname,enum tftp_transfer_mode mode);
err_t lwip_owned_tftp_start(u8_t mode,const struct tftp_context *context);
void lwip_owned_tftp_stop(void);
/* Only meaningful during the original context.close callback:
 * 0=timer/other, 3=DATA dispatch, 4=ACK dispatch, 5=ERROR dispatch, 256=explicit stop.
 * The observer classifies closure; it never accepts/rejects/modifies a packet. */
unsigned int lwip_owned_tftp_close_context(void);
#endif
