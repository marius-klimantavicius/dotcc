#ifndef BLINK_CAMPAIGN_HOST_VARIABLES_H
#define BLINK_CAMPAIGN_HOST_VARIABLES_H
#include <stdlib.h>
#define getenv blink_host_getenv
char *getenv(const char *);
#endif
