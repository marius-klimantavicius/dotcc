/* Test-only clock notification sink. Under instance translation these globals
 * belong to the particular translated owner, just like the original SNTP state.
 * The native oracle links this same observer in its isolated process.
 */
#include "sntp-clock.h"
static uint32_t count, last_seconds, last_fraction;
void dotcc_udp_sntp_notify(uint32_t seconds, uint32_t fraction)
{
  last_seconds = seconds;
  last_fraction = fraction;
  count++;
}
uint32_t dotcc_udp_sntp_notification_count(void) { return count; }
uint32_t dotcc_udp_sntp_notification_seconds(void) { return last_seconds; }
uint32_t dotcc_udp_sntp_notification_fraction(void) { return last_fraction; }

/* Matching native implementation of the real product port's notification ABI.
 * Product managed fixtures observe LwipSntpClient instead of these counters. */
void lwip_host_sntp_time(unsigned int seconds,unsigned int fraction)
{ dotcc_udp_sntp_notify((uint32_t)seconds,(uint32_t)fraction); }
