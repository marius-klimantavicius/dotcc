#ifndef _FCNTL_H
#define _FCNTL_H

/* Linux descriptor/status flag values. F_SETFL controls socket blocking and
   file append behavior; unsupported nonblocking console input returns ENOTSUP. */

#define FD_CLOEXEC 1
#define F_GETFD 1
#define F_SETFD 2
#define F_GETFL 3
#define F_SETFL 4

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

int fcntl(int fd, int cmd, ...);

/* open() returns a dotcc fd (a FileSlot index, same space as fileno); the
   O_* access/creation flags above map to .NET FileMode/FileAccess in
   DotCC.Libc.PosixFsLib. The mode argument is accepted (best-effort chmod). */
int open(const char *path, int flags, ...);

#endif /* _FCNTL_H */
