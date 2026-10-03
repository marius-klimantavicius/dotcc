#include "timezone_compat.h"
#include "time_impl.h"
#include <errno.h>

time_t mktime(struct tm *tm)
{
	struct tm converted;
	long opp;
	long long t = __tm_to_secs(tm);

	__secs_to_zone(t, 1, &converted.tm_isdst, &converted.__tm_gmtoff, &opp, &converted.__tm_zone);

	if (tm->tm_isdst>=0 && converted.tm_isdst!=tm->tm_isdst)
		t -= opp - converted.__tm_gmtoff;

	t -= converted.__tm_gmtoff;
	if ((time_t)t != t) goto error;

	__secs_to_zone(t, 0, &converted.tm_isdst, &converted.__tm_gmtoff, &opp, &converted.__tm_zone);

	if (__secs_to_tm(t + converted.__tm_gmtoff, &converted) < 0) goto error;

	*tm = converted;
	return t;

error:
	errno = EOVERFLOW;
	return -1;
}
