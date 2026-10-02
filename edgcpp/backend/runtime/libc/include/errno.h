/* EDG dot-gen portable LP64 ABI: initial audited libc subset. */
#ifndef __EDG_DOT_LIBC_ERRNO_H
#define __EDG_DOT_LIBC_ERRNO_H
#ifdef __cplusplus
extern "C" {
#endif
int *__errno_location(void);
#define errno (*__errno_location())
/* Stable translated errno ABI, normalized by the runtime on each host. */
#define EPERM 1
#define ENOENT 2
#define EINTR 4
#define EIO 5
#define EBADF 9
#define EAGAIN 11
#define EACCES 13
#define EEXIST 17
#define EXDEV 18
#define ENOTDIR 20
#define EISDIR 21
#define EMFILE 24
#define EFBIG 27
#define ENOSPC 28
#define ESPIPE 29
#define EROFS 30
#define EPIPE 32
#define EDOM 33
#define ENAMETOOLONG 36
#define ENOTEMPTY 39
#define ELOOP 40
#define EOVERFLOW 75
#define EILSEQ 84
#define ENOTSUP 95
#define EOPNOTSUPP ENOTSUP
#define EWOULDBLOCK EAGAIN
#define ERANGE 34
#define EINVAL 22
#define ENOMEM 12
#ifdef __cplusplus
}
#endif
#endif
