#ifndef BLINK_MANAGED_HOST_FCNTL_H
#define BLINK_MANAGED_HOST_FCNTL_H
#include "abi.h"
#include "fcntl-constants.h"
#include "limits-constants.h"
/* Native-checked C host storage. Guest records/constants remain blink/linux.h.
 * Operations are isolated declarations; no shared fcntl success stub is bound. */
#define flock blink_host_flock
#define fcntl blink_host_fcntl
#define open blink_host_open
#define openat blink_host_openat
int fcntl(int, int, ...);
int open(const char *, int, ...);
int openat(int, const char *, int, ...);
#endif
