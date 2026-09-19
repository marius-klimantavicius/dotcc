#ifndef BLINK_CAMPAIGN_HOST_RESOURCES_H
#define BLINK_CAMPAIGN_HOST_RESOURCES_H
#include <sys/resource.h>
#include <sys/times.h>
/* Private owner policy; the supplied resource header provides typed aliases.
 * Child usage is zero in this childless namespace; self accounting is unsupported. */
int getrlimit(int, struct rlimit *);
int setrlimit(int, const struct rlimit *);
int getpriority(int, unsigned int);
int setpriority(int, unsigned int, int);
#endif
