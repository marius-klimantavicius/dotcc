/* EDG dot-gen portable LP64 ABI: initial audited libc subset. */
#ifndef __EDG_DOT_LIBC_ERRNO_H
#define __EDG_DOT_LIBC_ERRNO_H
#ifdef __cplusplus
extern "C" {
#endif
int *__errno_location(void);
#define errno (*__errno_location())
#define EDOM 33
#define ERANGE 34
#define EINVAL 22
#define ENOMEM 12
#ifdef __cplusplus
}
#endif
#endif
