#ifndef DOTCC_UNIX_BINDINGS_H
#define DOTCC_UNIX_BINDINGS_H
#define open uvfs_open
#define close uvfs_close
#define access uvfs_access
#define getcwd uvfs_getcwd
#define stat uvfs_stat
#define fstat uvfs_fstat
#define lstat uvfs_lstat
#define ftruncate uvfs_ftruncate
#define read uvfs_read
#define write uvfs_write
#define pread uvfs_pread
#define pwrite uvfs_pwrite
#define lseek uvfs_lseek
#define fchmod uvfs_fchmod
#define fchown uvfs_fchown
#define geteuid uvfs_geteuid
#define getpid uvfs_getpid
#define unlink uvfs_unlink
#define mkdir uvfs_mkdir
#define rmdir uvfs_rmdir
#define readlink uvfs_readlink
#define fsync uvfs_fsync
#define fdatasync uvfs_fdatasync
#define getpagesize uvfs_getpagesize
#define fcntl uvfs_fcntl
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <fcntl.h>
typedef unsigned long dev_t;
struct flock { short l_type; short l_whence; long l_start; long l_len; int l_pid; };
#define F_GETLK 5
#define F_SETLK 6
#define F_SETLKW 7
#define F_RDLCK 0
#define F_WRLCK 1
#define F_UNLCK 2
#define FD_CLOEXEC 1
#define O_CLOEXEC 02000000
#define O_NOFOLLOW 0400000
#define O_DIRECTORY 0200000
#define _SC_PAGESIZE 30
#define ENOLCK 37
#define sysconf uvfs_sysconf
#define utimes uvfs_utimes
long uvfs_sysconf(int);
int uvfs_utimes(const char*, const void*);
int uvfs_fchmod(int, unsigned int);
int uvfs_fchown(int, unsigned int, unsigned int);
unsigned int uvfs_geteuid(void);
long uvfs_pread(int, void*, unsigned long, long);
long uvfs_pwrite(int, const void*, unsigned long, long);
int uvfs_fdatasync(int);
int uvfs_getpagesize(void);
#endif
