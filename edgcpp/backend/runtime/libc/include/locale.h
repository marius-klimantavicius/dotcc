/* Locale declarations. The current setlocale binding supports only CTYPE/ALL
   with C/POSIX or C.UTF-8. POSIX locale handles remain unimplemented/unbound. */
#ifndef __EDG_DOT_LIBC_LOCALE_H
#define __EDG_DOT_LIBC_LOCALE_H
#define LC_CTYPE 0
#define LC_NUMERIC 1
#define LC_TIME 2
#define LC_COLLATE 3
#define LC_MONETARY 4
#define LC_MESSAGES 5
#define LC_ALL 6
#define LC_CTYPE_MASK (1<<LC_CTYPE)
#define LC_NUMERIC_MASK (1<<LC_NUMERIC)
#define LC_TIME_MASK (1<<LC_TIME)
#define LC_COLLATE_MASK (1<<LC_COLLATE)
#define LC_MONETARY_MASK (1<<LC_MONETARY)
#define LC_MESSAGES_MASK (1<<LC_MESSAGES)
#define LC_ALL_MASK 63
/* Opaque translated locale identity, never a native OS locale pointer. */
typedef struct __edg_locale *locale_t;
#define LC_GLOBAL_LOCALE ((locale_t)-1L)
#ifdef __cplusplus
extern "C" {
#endif
char *setlocale(int, const char *);
locale_t newlocale(int, const char *, locale_t);
locale_t duplocale(locale_t);
void freelocale(locale_t);
locale_t uselocale(locale_t);
#ifdef __cplusplus
}
#endif
#endif
