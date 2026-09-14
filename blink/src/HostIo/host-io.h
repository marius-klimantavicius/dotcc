#ifndef BLINK_CAMPAIGN_HOST_IO_H
#define BLINK_CAMPAIGN_HOST_IO_H
#include <sys/types.h>
#include <sys/uio.h>
#include <fcntl.h>
#include <unistd.h>
int blink_io_open(const char *, int);
int blink_io_open_at(int, const char *, int);
int blink_io_control(int, int, int);
int blink_io_close(int);
int blink_io_dup(int);
off_t blink_io_seek(int, off_t, int);
ssize_t blink_io_read(int, void *, size_t);
ssize_t blink_io_pread(int, void *, size_t, off_t);
int blink_io_read_at_length(int, off_t *);
ssize_t blink_io_write(int, const void *, size_t);
ssize_t blink_io_readv(int, const struct iovec *, int);
ssize_t blink_io_writev(int, const struct iovec *, int);
/* This opt-in boundary overrides only explicitly implemented file/stream calls. */
#undef open
/* Optional mode is evaluated by the normal C variadic call. Creation retains
 * the documented fixed private mode0600; chmod/umask are not implemented. */
static inline int blink_io_open_mode(const char *path, int flags, ...) {
  return blink_io_open(path, flags);
}
#define open blink_io_open_mode
#define close blink_io_close
#define dup blink_io_dup
#define lseek blink_io_seek
#define read blink_io_read
#define pread blink_io_pread
#define write blink_io_write
#undef readv
#undef writev
#define readv blink_io_readv
#define writev blink_io_writev
#endif
