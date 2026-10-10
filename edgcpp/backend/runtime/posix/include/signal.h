/* Portable POSIX signal import declarations. Runtime binding is required. */
#ifndef EDG_POSIX_SIGNAL_H
#define EDG_POSIX_SIGNAL_H
#include <sys/types.h>
#include "../../libc/include/signal.h"
/* Portable profile value, not a host OS ABI constant. */
#define SIGKILL 9
#ifdef __cplusplus
extern "C" {
#endif
int kill(pid_t, int);
#ifdef __cplusplus
}
#endif
#endif
