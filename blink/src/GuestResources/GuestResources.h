#ifndef BLINK_CAMPAIGN_GUEST_RESOURCES_H
#define BLINK_CAMPAIGN_GUEST_RESOURCES_H
#include <stddef.h>
struct System;
/* Single-worker initialization, after NewSystem and before NewMachine, loader,
 * guest descriptor installation or page-table allocation. */
int BlinkHostInitializeResourceLimits(struct System *, size_t);
/* Managed callback: obtains capacity from the bound InstanceIo. */
int BlinkHostInitializeBoundResourceLimits(struct System *);
#endif
