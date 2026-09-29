/* Unregistered fixture boundary: force-include for the original sntp.c only.
 * Include this after selecting the profile; do not patch upstream sntp.c.
 * NTP timestamp words stay in host byte order, with the original wire-era
 * seconds and unsigned fraction. This does not set the operating system clock.
 */
#ifndef DOTCC_UDP_APPLICATION_SNTP_CLOCK_H
#define DOTCC_UDP_APPLICATION_SNTP_CLOCK_H
#include <stdint.h>
void dotcc_udp_sntp_notify(uint32_t seconds, uint32_t fraction);
uint32_t dotcc_udp_sntp_notification_count(void);
uint32_t dotcc_udp_sntp_notification_seconds(void);
uint32_t dotcc_udp_sntp_notification_fraction(void);
#define SNTP_SET_SYSTEM_TIME_NTP(seconds, fraction) \
  dotcc_udp_sntp_notify((uint32_t)(seconds), (uint32_t)(fraction))
#endif
