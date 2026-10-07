#ifndef __EDG_POSIX_SYS_UIO_H
#define __EDG_POSIX_SYS_UIO_H
#include <sys/types.h>

/* Portable LP64 record. No native operating-system structure is exposed. */
struct iovec {
    void *iov_base;
    size_t iov_len;
};

#ifdef __cplusplus
extern "C" {
#endif
ssize_t readv(int, const struct iovec *, int);
ssize_t writev(int, const struct iovec *, int);
#ifdef __cplusplus
}
#endif
#endif
