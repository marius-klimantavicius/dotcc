#ifndef BLINK_CAMPAIGN_HOST_SIGNAL_ACTIONS_H
#define BLINK_CAMPAIGN_HOST_SIGNAL_ACTIONS_H
#include <signal.h>
/* C tags and functions have distinct namespaces. Preserve the measured tag
 * while selecting a distinct CLR-callable implementation for ordinary calls.
 * Bare sigaction function designators are not supported by this profile. */
int blink_host_register_sigaction(int, const struct sigaction *, struct sigaction *);
#define blink_host_sigaction(...) blink_host_register_sigaction(__VA_ARGS__)
/* Registration state only: no OS handlers or asynchronous dispatcher. */
int BlinkHostSignalActionsBegin(void);
void BlinkHostSignalActionsEnd(void);
#endif
