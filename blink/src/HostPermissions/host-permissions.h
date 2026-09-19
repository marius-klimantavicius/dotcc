#ifndef BLINK_HOST_PERMISSIONS_H
#define BLINK_HOST_PERMISSIONS_H
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>
#undef fchown
#undef fchownat
#undef umask
#define fchown blink_host_fchown
#define fchownat blink_host_fchownat
#define umask blink_host_umask
int fchown(int, uid_t, gid_t);
int fchownat(int, const char *, uid_t, gid_t, int);
mode_t umask(mode_t);
#endif
