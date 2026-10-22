#ifndef _FCNTL_H
#define _FCNTL_H

#include <sys/types.h>
int posix_fallocate(int fd, off_t offset, off_t len);

/* Linux descriptor/status flag values. F_SETFL controls socket blocking and
   file append behavior; unsupported nonblocking console input returns ENOTSUP. */

#define FD_CLOEXEC 1
#define F_GETFD 1
#define F_SETFD 2
#define F_GETFL 3
#define F_SETFL 4

/* Linux x64 LP64 process-associated advisory record locks. off_t is signed
   64-bit and pid_t is signed 32-bit. The aggregate is 32 bytes/alignment 8,
   with fields at 0, 2, 8, 16, 24. This is not a 32-bit host ABI declaration. */
#define F_GETLK  5
#define F_SETLK  6
#define F_SETLKW 7
#define F_RDLCK  0
#define F_WRLCK  1
#define F_UNLCK  2

struct flock {
    short l_type;
    short l_whence;
    off_t l_start;
    off_t l_len;
    pid_t l_pid;
};

#define O_ACCMODE  0x3
#define O_CLOEXEC  0x80000
#define O_RDONLY   0x0
#define O_WRONLY   0x1
#define O_RDWR     0x2
#define O_CREAT    0x40
#define O_EXCL     0x80
#define O_TRUNC    0x200
#define O_APPEND   0x400
#define O_NONBLOCK 0x800
#define O_NOFOLLOW 0x20000

int fcntl(int fd, int cmd, ...);

/* open() returns a dotcc fd (a FileSlot index, same space as fileno); the
   O_* access/creation flags above map to .NET FileMode/FileAccess in
   DotCC.Libc.PosixFsLib. The mode argument is accepted (best-effort chmod). */
int open(const char *path, int flags, ...);

#endif /* _FCNTL_H */
