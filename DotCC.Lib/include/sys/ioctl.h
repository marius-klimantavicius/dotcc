#ifndef _DOTCC_SYS_IOCTL_H
#define _DOTCC_SYS_IOCTL_H
/* Linux socket control request values. */
#define FIONREAD 0x541B
#define TIOCINQ FIONREAD
#define FIONBIO 0x5421
int ioctl(int fd, unsigned long request, ...);
#endif
