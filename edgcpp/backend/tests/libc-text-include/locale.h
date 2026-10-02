/* Bounded C/POSIX and C.UTF-8 provider; other locale categories are pending. */
#ifndef __EDG_DOT_LIBC_LOCALE_H
#define __EDG_DOT_LIBC_LOCALE_H
#define LC_CTYPE 0
#define LC_ALL 6
#ifdef __cplusplus
extern "C" {
#endif
char *setlocale(int, const char *);
#ifdef __cplusplus
}
#endif
#endif
