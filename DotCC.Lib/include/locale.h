#ifndef _LOCALE_H
#define _LOCALE_H

/* dotcc's <locale.h> (C90 7.4) — locale control. Backed by
   DotCC.Libc/LocaleLib.cs.

   dotcc supports "C" (== "POSIX") formatting — the one guaranteed at
   program startup and the only set of conventions portable across hosts.
   setlocale() accepts NULL (query) / "" / "C" / "POSIX" and returns "C"; any
   other formatting locale name is reported unsupported (NULL). LC_CTYPE and
   LC_ALL additionally accept C.UTF-8/C.utf8, selecting strict UTF-8 for the
   stateless multibyte conversions; C/POSIX use ASCII. Selection is scoped to
   the bound runtime and shared by its workers. localeconv() reports the
   "C" locale's conventions (decimal_point ".", every other string empty, the
   numeric members CHAR_MAX). Numeric/monetary conventions remain C.

   `struct lconv` is the runtime Libc.lconv struct — declared ONLY there (same
   pattern as <time.h>'s struct tm): dotcc parses `struct lconv` via the usual
   `struct ID` rule and emits the bare tag `lconv`, which resolves to Libc.lconv
   through `using static Libc;`. A `struct lconv { … };` body here would make the
   emitter produce a second, colliding top-level `lconv`. (And `lconv` must NOT
   be seeded as a type name — that would break the `struct ID` parse.) Its
   members, for reference, are the C90 set: the string members `decimal_point`,
   `thousands_sep`, `grouping`, `int_curr_symbol`, `currency_symbol`,
   `mon_decimal_point`, `mon_thousands_sep`, `mon_grouping`, `positive_sign`,
   `negative_sign`, then the numeric `char` members `int_frac_digits`,
   `frac_digits`, `p_cs_precedes`, `p_sep_by_space`, `n_cs_precedes`,
   `n_sep_by_space`, `p_sign_posn`, `n_sign_posn` (plus the C99 `int_*` set). */

#ifndef NULL
#define NULL ((void*)0)
#endif

/* C/POSIX categories, using Linux values. Formatting conventions are C;
   LC_CTYPE/LC_ALL select the global character encoding. */
#define LC_ALL      6
#define LC_COLLATE  3
#define LC_CTYPE    0
#define LC_MONETARY 4
#define LC_NUMERIC  1
#define LC_TIME     2
#define LC_MESSAGES 5

/* POSIX locale objects are opaque pointer-sized identities owned by the bound
   runtime. They must only be passed to these functions, never dereferenced.
   C/POSIX and C.UTF-8/C.utf8 are supported; other names fail with ENOENT.
   Empty names deterministically select C, matching dotcc's setlocale policy. */
typedef void *locale_t;
#define LC_CTYPE_MASK    (1 << LC_CTYPE)
#define LC_NUMERIC_MASK  (1 << LC_NUMERIC)
#define LC_TIME_MASK     (1 << LC_TIME)
#define LC_COLLATE_MASK  (1 << LC_COLLATE)
#define LC_MONETARY_MASK (1 << LC_MONETARY)
#define LC_MESSAGES_MASK (1 << LC_MESSAGES)
/* Linux category-mask ABI, including its C-only extension categories. */
#define LC_ALL_MASK 0x1fbf
#define LC_GLOBAL_LOCALE ((locale_t)-1)

locale_t newlocale(int category_mask, const char *locale, locale_t base);
locale_t duplocale(locale_t locale);
locale_t uselocale(locale_t locale);
void freelocale(locale_t locale);

/* Explicit-locale host services. C/C.UTF-8 retain C formatting and byte
   collation; narrow classification is ASCII. Wide scalar classification and
   simple casing in C.UTF-8 use an immutable Linux libc Unicode service.
   C.UTF-8 handle creation fails with ENOTSUP when that service is unavailable. */
#include <stddef.h>
int strcoll_l(const char *a, const char *b, locale_t locale);
size_t strxfrm_l(char *destination, const char *source, size_t n, locale_t locale);
/* Uses CalendarLib's C formatting subset; unsupported directives return zero
   with ENOTSUP, without changing the destination. */
size_t strftime_l(char *destination, size_t n, const char *format,
                  const struct tm *time, locale_t locale);
char *nl_langinfo_l(int item, locale_t locale);
int isalnum_l(int c, locale_t locale);
int isalpha_l(int c, locale_t locale);
int isblank_l(int c, locale_t locale);
int iscntrl_l(int c, locale_t locale);
int isdigit_l(int c, locale_t locale);
int isgraph_l(int c, locale_t locale);
int islower_l(int c, locale_t locale);
int isprint_l(int c, locale_t locale);
int ispunct_l(int c, locale_t locale);
int isspace_l(int c, locale_t locale);
int isupper_l(int c, locale_t locale);
int isxdigit_l(int c, locale_t locale);
int tolower_l(int c, locale_t locale);
int toupper_l(int c, locale_t locale);
int iswalnum_l(int c, locale_t locale);
int iswalpha_l(int c, locale_t locale);
int iswblank_l(int c, locale_t locale);
int iswcntrl_l(int c, locale_t locale);
int iswdigit_l(int c, locale_t locale);
int iswgraph_l(int c, locale_t locale);
int iswlower_l(int c, locale_t locale);
int iswprint_l(int c, locale_t locale);
int iswpunct_l(int c, locale_t locale);
int iswspace_l(int c, locale_t locale);
int iswupper_l(int c, locale_t locale);
int iswxdigit_l(int c, locale_t locale);
int towlower_l(int c, locale_t locale);
int towupper_l(int c, locale_t locale);

char *setlocale(int category, const char *locale);
struct lconv *localeconv(void);

#endif
