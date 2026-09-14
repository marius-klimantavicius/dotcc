#ifndef BLINK_MANAGED_POLL_H
#define BLINK_MANAGED_POLL_H
#include "abi.h"
typedef uint64_t nfds_t;
#define pollfd blink_host_pollfd
#define POLLIN 0x001
#define POLLPRI 0x002
#define POLLOUT 0x004
#define POLLERR 0x008
#define POLLHUP 0x010
#define POLLNVAL 0x020
#define POLLRDNORM 0x040
#define POLLRDBAND 0x080
#define POLLWRNORM 0x100
#define POLLWRBAND 0x200
#define poll blink_host_poll
int poll(struct pollfd *, nfds_t, int);
#endif
