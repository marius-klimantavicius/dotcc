#ifndef DOTCC_PINTA_NATIVE_PREINCLUDE_H
#define DOTCC_PINTA_NATIVE_PREINCLUDE_H
#include "pinta-preinclude.h"
#include <stdarg.h>
#include <stdlib.h>
/* Never call the host's 32-bit wchar libc with -fshort-wchar strings. */
size_t pinta_test_wcslen(const wchar_t *text);
int pinta_test_swprintf(wchar_t *output, size_t capacity, const wchar_t *format, ...);
int pinta_test_wcscmp(const wchar_t *left, const wchar_t *right);
int pinta_test_wcstombs_s(size_t *count, char *out, size_t capacity, const wchar_t *text, size_t maximum);
wint_t pinta_test_putwchar(wchar_t value);
#define putwchar pinta_test_putwchar
#define swprintf pinta_test_swprintf
#define wcslen pinta_test_wcslen
#define wcscmp pinta_test_wcscmp
#define wcstombs_s pinta_test_wcstombs_s
#endif
