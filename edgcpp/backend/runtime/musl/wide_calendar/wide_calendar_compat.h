#ifndef EDG_WIDE_CALENDAR_COMPAT_H
#define EDG_WIDE_CALENDAR_COMPAT_H
#include <wchar.h>
#include <stdlib.h>
#include "../calendar/calendar_compat.h"
#ifdef __cplusplus
extern "C" {
#endif
size_t __wcsftime_l(wchar_t *, size_t, const wchar_t *, const struct tm *, locale_t);
#ifdef __cplusplus
}
#endif
#endif
