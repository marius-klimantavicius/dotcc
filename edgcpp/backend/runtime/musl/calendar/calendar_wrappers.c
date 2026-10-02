#include "calendar_compat.h"
#include "time_impl.h"

/* Explicit wrappers replace native linker aliases. The opaque _l argument is
   validated before internal CURRENT_LOCALE/C_LOCALE conventions are used. */
size_t strftime_l(char *s, size_t n, const char *format, const struct tm *tm, locale_t locale)
{ if (!__edg_locale_validate(locale)) return 0; return __strftime_l(s,n,format,tm,locale); }
char *nl_langinfo_l(nl_item item, locale_t locale)
{ if (!__edg_locale_validate(locale)) return (char *)""; return __nl_langinfo_l(item,locale); }
char *nl_langinfo(nl_item item) { return __nl_langinfo(item); }
struct tm *gmtime_r(const time_t *t, struct tm *tm) { return __gmtime_r(t,tm); }
char *asctime_r(const struct tm *tm, char *buffer) { return __asctime_r(tm,buffer); }
const char *__tm_to_tzname(const struct tm *tm) { return __edg_calendar_zone(tm->tm_zone); }
