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
/* Authored test diagnostics; engine-only/product builds do not define these. */
void pinta_test_trace_step(void *thread, unsigned code);
void pinta_test_trace_type(void *object);
void pinta_test_trace_load(int exception, void *domain);
void pinta_test_trace_gc(void *parent, void *child, unsigned index, unsigned field);
void pinta_test_trace_scratch(void *start, void *end, unsigned count);
#define PINTA_TEST_TRACE_STEP(thread, code) pinta_test_trace_step(thread, code)
#define PINTA_TEST_TRACE_TYPE(object) pinta_test_trace_type(object)
#define PINTA_TEST_TRACE_LOAD(exception, domain) pinta_test_trace_load(exception, domain)
#define PINTA_TEST_TRACE_GC(parent, child, index, field) pinta_test_trace_gc(parent, child, index, field)
#define PINTA_TEST_TRACE_SCRATCH(start, end, count) pinta_test_trace_scratch(start, end, count)
#endif
