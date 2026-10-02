#include "calendar_compat.h"
#ifdef __cplusplus
extern "C" {
#endif
int __month_to_secs(int, int);
long long __year_to_secs(long long, int *);
long long __tm_to_secs(const struct tm *);
const char *__tm_to_tzname(const struct tm *);
int __secs_to_tm(long long, struct tm *);
const char *__strftime_fmt_1(char (*)[100], size_t *, int, const struct tm *, locale_t, int);
#ifdef __cplusplus
}
#endif
