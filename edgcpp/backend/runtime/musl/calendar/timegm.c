#include "calendar_compat.h"
#define _GNU_SOURCE
#include "time_impl.h"
#include <errno.h>

time_t timegm(struct tm *tm)
{
	struct tm converted;
	long long t = __tm_to_secs(tm);
	if (__secs_to_tm(t, &converted) < 0) {
		errno = EOVERFLOW;
		return -1;
	}
	*tm = converted;
	tm->tm_isdst = 0;
	tm->__tm_gmtoff = 0;
	tm->__tm_zone = __utc;
	return t;
}
