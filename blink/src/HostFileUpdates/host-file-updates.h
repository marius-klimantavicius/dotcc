#ifndef BLINK_PRIVATE_FILE_UPDATES_H
#define BLINK_PRIVATE_FILE_UPDATES_H
#include <sys/types.h>
#include <unistd.h>
#define pwrite blink_host_pwrite
#define ftruncate blink_host_ftruncate
#define truncate blink_host_truncate
#define fsync blink_host_fsync
#define fdatasync blink_host_fdatasync
ssize_t pwrite(int,const void *,size_t,off_t);
int ftruncate(int,off_t);
int truncate(const char *,off_t);
int fsync(int);
int fdatasync(int);
#endif
