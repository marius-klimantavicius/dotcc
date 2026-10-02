/* Portable calendar boundary; algorithms and tables are pinned musl sources. */
#ifndef EDG_CALENDAR_COMPAT_H
#define EDG_CALENDAR_COMPAT_H
#include <stddef.h>
#include <locale.h>
#include <langinfo.h>
#include <time.h>
#ifdef __cplusplus
#define restrict __restrict
extern "C" {
#endif
int __edg_locale_validate(locale_t);
int __edg_locale_categories(locale_t);
char *__edg_calendar_utc(void);
char *__edg_calendar_zone(const char *);
char *__nl_langinfo_l(nl_item, locale_t);
char *__nl_langinfo(nl_item);
size_t __strftime_l(char *, size_t, const char *, const struct tm *, locale_t);
struct tm *__gmtime_r(const time_t *, struct tm *);
char *__asctime_r(const struct tm *, char *);
#ifdef __cplusplus
}
#endif
#define __tm_gmtoff tm_gmtoff
#define __tm_zone tm_zone
#define __utc (__edg_calendar_utc())
/* Private sentinels only: public _l wrappers still validate explicit handles. */
#define CURRENT_LOCALE ((locale_t)0)
#define C_LOCALE ((locale_t)-2)
/* Both admitted locales use musl's unchanged C time/message data. */
#define LCTRANS(text, category, locale) (text)
#endif
