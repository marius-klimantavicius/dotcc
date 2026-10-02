/* Hidden restartable state is per translated owner/thread, separately per API. */
#ifndef EDG_MB_COMPAT_H
#define EDG_MB_COMPAT_H
#include <wchar.h>
#include <stdlib.h>
#ifdef __cplusplus
#define restrict __restrict
#define EDG_MB_TLS thread_local
#else
#define EDG_MB_TLS _Thread_local
#endif
#endif
