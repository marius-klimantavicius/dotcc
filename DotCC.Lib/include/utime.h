#ifndef _DOTCC_UTIME_H
#define _DOTCC_UTIME_H
#include <time.h>
/* Linux LP64 time_t pair; no native wchar operands. */
struct utimbuf { time_t actime; time_t modtime; };
int utime(const char *path, const struct utimbuf *times);
#endif
