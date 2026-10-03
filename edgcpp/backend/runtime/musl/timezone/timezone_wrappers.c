#include "timezone_compat.h"
struct tm *localtime_r(const time_t *t, struct tm *tm) { return __localtime_r(t, tm); }
