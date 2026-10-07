#ifndef EDG_POSIX_SYS_IOCTL_H
#define EDG_POSIX_SYS_IOCTL_H
struct winsize {
    unsigned short ws_row, ws_col, ws_xpixel, ws_ypixel;
};
#define TIOCGWINSZ 0x5413UL
#ifdef __cplusplus
extern "C" {
#endif
int ioctl(int, unsigned long, ...);
#ifdef __cplusplus
}
#endif
#endif
