#ifndef _DOTCC_SYS_IOCTL_H
#define _DOTCC_SYS_IOCTL_H

/* Linux terminal window-size ABI. These are declarations only; unsupported
   ioctl requests must not be reported as successful host operations. */
struct winsize {
    unsigned short ws_row;
    unsigned short ws_col;
    unsigned short ws_xpixel;
    unsigned short ws_ypixel;
};
#define TIOCGWINSZ 0x5413
#define TIOCSWINSZ 0x5414

/* Linux socket control request values. */
#define FIONREAD 0x541B
#define TIOCINQ FIONREAD
#define FIONBIO 0x5421
int ioctl(int fd, unsigned long request, ...);
#endif
