#ifndef BLINK_CAMPAIGN_HOST_ACCESS_H
#define BLINK_CAMPAIGN_HOST_ACCESS_H
#include <unistd.h>
#include <fcntl.h>
#define access blink_host_access
#define faccessat blink_host_faccessat
int access(const char *, int);
int faccessat(int, const char *, int, int);
#endif
