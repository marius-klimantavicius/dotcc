#ifndef BLINK_MANAGED_HOST_SYS_IOCTL_H
#define BLINK_MANAGED_HOST_SYS_IOCTL_H
#include "ioctl-constants.h"
/* Native-measured Linux LP64 terminal storage; no terminal implementation. */
struct winsize {
  unsigned short ws_row, ws_col, ws_xpixel, ws_ypixel;
};
#define ioctl blink_host_ioctl
int ioctl(int, unsigned long, ...);
#endif
