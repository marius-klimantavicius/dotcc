#ifndef EDG_STRPTIME_COMPAT_H
#define EDG_STRPTIME_COMPAT_H
#include <limits.h>
#include <time.h>
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
char *strptime(const char *, const char *, struct tm *);
#ifdef __cplusplus
}
#endif
#define __tm_gmtoff tm_gmtoff
#endif
