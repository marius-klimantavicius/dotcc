/* Actual PostgreSQL locale storage plus real libc host-service contracts. */
#include "postgres.h"
#include "utils/pg_locale.h"
#include <langinfo.h>
#include <time.h>
#include <errno.h>
#include <wctype.h>

/* Storage for an actual declared but otherwise unused header inline. */
MemoryContext CurrentMemoryContext = NULL;

size_t dpl_layout(unsigned int item)
{
    switch (item)
    {
        case 0: return sizeof(locale_t);
        case 1: return _Alignof(locale_t);
        case 2: return sizeof(struct pg_locale_struct);
        case 3: return _Alignof(struct pg_locale_struct);
        case 4: return offsetof(struct pg_locale_struct, collate);
        case 5: return offsetof(struct pg_locale_struct, info);
        case 6: return sizeof(((struct pg_locale_struct *) 0)->info.lt);
        case 7: return offsetof(struct pg_locale_struct, info.lt);
        default: return 0;
    }
}

#define CHECK(condition) do { if (!(condition)) return __LINE__; } while (0)
int dpl_contracts(void)
{
    locale_t c, utf8, copy, previous;
    wchar_t wide[3];
    char bytes[8], transformed[5];
    struct tm time = {0};
    c = newlocale(LC_ALL_MASK, "C", (locale_t) 0);
    CHECK(c != (locale_t) 0);
    utf8 = newlocale(LC_ALL_MASK, "C.UTF-8", (locale_t) 0);
    CHECK(utf8 != (locale_t) 0);
    copy = duplocale(utf8);
    CHECK(copy != (locale_t) 0);
    CHECK(strcmp(nl_langinfo_l(CODESET, c), "ANSI_X3.4-1968") == 0);
    CHECK(strcmp(nl_langinfo_l(CODESET, utf8), "UTF-8") == 0);
    CHECK(strcmp(nl_langinfo_l(RADIXCHAR, utf8), ".") == 0);
    CHECK(strcmp(nl_langinfo_l(DAY_1, utf8), "Sunday") == 0);
    CHECK(strcoll_l("a", "b", utf8) < 0);
    CHECK(strcoll_l("\303\251", "z", utf8) > 0);
    CHECK(strxfrm_l(NULL, "abc", 0, utf8) == 3);
    transformed[4] = 42;
    CHECK(strxfrm_l(transformed, "abc", 4, utf8) == 3);
    CHECK(strcmp(transformed, "abc") == 0 && transformed[4] == 42);
    CHECK(isalpha_l('A', utf8) != 0 && isalpha_l(0xe9, utf8) == 0);
    CHECK(tolower_l('A', utf8) == 'a' && toupper_l('z', c) == 'Z');
    CHECK(iswalpha_l(0xe9, utf8) != 0 && iswalpha_l(0xe9, c) == 0);
    CHECK(towlower_l(0x10400, utf8) == 0x10428);
    CHECK(towupper_l(0x10428, utf8) == 0x10400);
    CHECK(setlocale(LC_ALL, "C") != NULL);
    previous = uselocale(utf8);
    CHECK(previous == LC_GLOBAL_LOCALE);
    CHECK(strcmp(setlocale(LC_CTYPE, NULL), "C") == 0);
    CHECK(mbstowcs(wide, "\303\251", 3) == 1 && wide[0] == 0xe9 && wide[1] == 0);
    CHECK(wcstombs(bytes, wide, 8) == 2 && strcmp(bytes, "\303\251") == 0);
    CHECK(uselocale(previous) == utf8);
    errno = 0;
    CHECK(mbstowcs(wide, "\303\251", 3) == (size_t) -1 && errno == EILSEQ);
    time.tm_year = 124; time.tm_mon = 0; time.tm_mday = 7; time.tm_wday = 0;
    CHECK(strftime_l(bytes, sizeof(bytes), "%a", &time, copy) == 3);
    CHECK(strcmp(bytes, "Sun") == 0);
    errno = 0;
    CHECK(newlocale(LC_CTYPE_MASK, "dotcc-nonexistent-locale", c) == (locale_t) 0 && errno == ENOENT);
    CHECK(strcmp(nl_langinfo_l(CODESET, c), "ANSI_X3.4-1968") == 0);
    freelocale(c); freelocale(utf8);
    CHECK(strcmp(nl_langinfo_l(CODESET, copy), "UTF-8") == 0);
    freelocale(copy);
    return 0;
}

unsigned int dpl_classes(int c, locale_t locale)
{
    return ((iswalnum_l(c, locale) != 0) << 0)
        | ((iswalpha_l(c, locale) != 0) << 1)
        | ((iswblank_l(c, locale) != 0) << 2)
        | ((iswcntrl_l(c, locale) != 0) << 3)
        | ((iswdigit_l(c, locale) != 0) << 4)
        | ((iswgraph_l(c, locale) != 0) << 5)
        | ((iswlower_l(c, locale) != 0) << 6)
        | ((iswprint_l(c, locale) != 0) << 7)
        | ((iswpunct_l(c, locale) != 0) << 8)
        | ((iswspace_l(c, locale) != 0) << 9)
        | ((iswupper_l(c, locale) != 0) << 10)
        | ((iswxdigit_l(c, locale) != 0) << 11);
}

/* Enumerates every Unicode scalar, including non-BMP values, without passing
 * a wchar_t* through the host boundary. Version comes from the OS C.UTF-8 data. */
unsigned long long dpl_unicode_digest(void)
{
    locale_t locale = newlocale(LC_CTYPE_MASK, "C.UTF-8", (locale_t) 0);
    unsigned long long digest = 14695981039346656037ULL;
    if (!locale) return 0;
    for (int c = 0; c <= 0x10ffff; c++)
    {
        if (c >= 0xd800 && c <= 0xdfff) continue;
        digest = (digest ^ dpl_classes(c, locale)) * 1099511628211ULL;
        digest = (digest ^ (unsigned int) towlower_l(c, locale)) * 1099511628211ULL;
        digest = (digest ^ (unsigned int) towupper_l(c, locale)) * 1099511628211ULL;
    }
    freelocale(locale);
    return digest;
}

size_t dpl_supplementary_units(void)
{
    locale_t locale = newlocale(LC_CTYPE_MASK, "C.UTF-8", (locale_t) 0);
    locale_t previous;
    size_t count;
    if (!locale) return (size_t) -1;
    previous = uselocale(locale);
    count = mbstowcs(NULL, "\360\237\232\200", 0);
    uselocale(previous); freelocale(locale);
    return count;
}

#ifdef DPL_NATIVE_MAIN
#undef printf
#undef fprintf
int main(void)
{
    int status = dpl_contracts();
    if (status != 0) { fprintf(stderr, "locale contract failed at line %d\n", status); return 1; }
    printf("{\"layout\":[");
    for (unsigned int i = 0; i < 8; i++) printf("%s%zu", i ? "," : "", dpl_layout(i));
    printf("],\"unicode_digest\":%llu,\"wchar_bytes\":%zu,\"supplementary_units\":%zu,\"behavior\":true}\n",
           dpl_unicode_digest(), sizeof(wchar_t), dpl_supplementary_units());
    return 0;
}
#endif
