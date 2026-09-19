#ifndef BLINK_PRIVATE_NAMESPACE_H
#define BLINK_PRIVATE_NAMESPACE_H
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <stdio.h>
#include <fcntl.h>
#ifndef AT_REMOVEDIR
#define AT_REMOVEDIR 512
#endif
#undef mkdir
#define mkdir blink_host_mkdir
#define mkdirat blink_host_mkdirat
#define unlink blink_host_unlink
#define unlinkat blink_host_unlinkat
#define rmdir blink_host_rmdir
#define rename blink_host_rename
#define renameat blink_host_renameat
int mkdir(const char *, unsigned int);
int mkdirat(int, const char *, unsigned int);
int unlink(const char *);
int unlinkat(int, const char *, int);
int rmdir(const char *);
int rename(const char *, const char *);
int renameat(int, const char *, int, const char *);
#endif
