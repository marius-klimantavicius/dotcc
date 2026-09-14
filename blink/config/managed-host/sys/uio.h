#ifndef BLINK_MANAGED_SYS_UIO_H
#define BLINK_MANAGED_SYS_UIO_H
#include <sys/types.h>
#include "abi.h"
#define iovec blink_host_iovec
#define readv blink_host_readv
#define writev blink_host_writev
#define preadv blink_host_preadv
#define pwritev blink_host_pwritev
ssize_t readv(int, const struct iovec *, int);
ssize_t writev(int, const struct iovec *, int);
ssize_t preadv(int, const struct iovec *, int, off_t);
ssize_t pwritev(int, const struct iovec *, int, off_t);
#endif
