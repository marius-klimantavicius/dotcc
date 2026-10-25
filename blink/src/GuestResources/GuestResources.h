#ifndef BLINK_CAMPAIGN_GUEST_RESOURCES_H
#define BLINK_CAMPAIGN_GUEST_RESOURCES_H
#include <stddef.h>
struct System;
/* Single-worker initialization, after NewSystem and before NewMachine, loader,
 * guest descriptor installation or page-table allocation. */
int BlinkHostInitializeResourceLimits(struct System *, size_t);
/* Explicit virtual reservation budget; backing retains its independent owner
 * limit. The default entry above keeps the original equal-budget policy. */
int BlinkHostInitializeResourceLimitsWithVirtual(struct System *, size_t, size_t);
/* Managed callback: obtains capacity from the bound InstanceIo. */
int BlinkHostInitializeBoundResourceLimits(struct System *);
#endif
