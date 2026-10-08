#include <stdio.h>
#include <stdarg.h>
#include <locale.h>
#include <wchar.h>
#include <string.h>
#include <limits.h>

typedef int (*narrow_callback)(void *, const char *, va_list);
typedef int (*wide_callback)(void *, const wchar_t *, va_list);

#ifdef NATIVE_ORACLE
/* Native locking oracle; the managed provider reuses LibcFiles instead. */
static int narrow_session(FILE *file, narrow_callback callback, void *context, const char *format, va_list args) {
    flockfile(file); int result = callback(context, format, args); funlockfile(file); return result;
}
static int wide_session(FILE *file, wide_callback callback, void *context, const wchar_t *format, va_list args) {
    flockfile(file); int result = callback(context, format, args); funlockfile(file); return result;
}
#define __edg_with_file_lock narrow_session
#define __edg_with_file_read_lock narrow_session
#define __edg_with_file_wide_lock wide_session
#define __edg_with_file_wide_read_lock wide_session
#else
extern int __edg_with_file_lock(FILE *, narrow_callback, void *, const char *, va_list);
extern int __edg_with_file_read_lock(FILE *, narrow_callback, void *, const char *, va_list);
extern int __edg_with_file_wide_lock(FILE *, wide_callback, void *, const wchar_t *, va_list);
extern int __edg_with_file_wide_read_lock(FILE *, wide_callback, void *, const wchar_t *, va_list);
#endif

static int narrow_write(void *context, const char *format, va_list args) {
    int value = va_arg(args, int);
    return value == 27 && fputs(format, (FILE *)context) >= 0 ? value : -1;
}
static int narrow_read(void *context, const char *format, va_list args) {
    int value = va_arg(args, int);
    return value == 37 && fgetc((FILE *)context) == format[0] ? value : -1;
}
static int wide_write(void *context, const wchar_t *format, va_list args) {
    int value = va_arg(args, int);
    return value == 47 && fputwc(format[0], (FILE *)context) == (wint_t)format[0] ? value : -1;
}
static int wide_read(void *context, const wchar_t *format, va_list args) {
    int value = va_arg(args, int);
    return value == 57 && fgetwc((FILE *)context) == (wint_t)format[0] ? value : -1;
}
static int session(int mode, FILE *file, ...) {
    va_list args;
    va_start(args, file);
    int result;
    switch (mode) {
        case 0: result = __edg_with_file_lock(file, narrow_write, file, "locked", args); break;
        case 1: result = __edg_with_file_read_lock(file, narrow_read, file, "locked", args); break;
        case 2: result = __edg_with_file_wide_lock(file, wide_write, file, L"wide", args); break;
        default: result = __edg_with_file_wide_read_lock(file, wide_read, file, L"wide", args); break;
    }
    va_end(args);
    return result;
}
int main(void) {
    locale_t locale = newlocale(LC_ALL_MASK, "C", 0);
    if (!locale) return 1;
    locale_t copy = duplocale(locale);
    if (!copy) return 2;
    locale_t previous = uselocale(copy);
    if (!previous) return 3;
    struct lconv *info = localeconv();
    if (!info || strcmp(info->decimal_point, ".") || info->int_frac_digits != CHAR_MAX) return 4;
    if (uselocale(previous) != copy) return 5;
    freelocale(copy);
    freelocale(locale);
    FILE *file = tmpfile();
    if (!file || session(0, file, 27) != 27 || fseek(file, 0, SEEK_SET) || session(1, file, 37) != 37 || fclose(file)) return 6;
    file = tmpfile();
    if (!file || session(2, file, 47) != 47 || fseek(file, 0, SEEK_SET) || session(3, file, 57) != 57 || fclose(file)) return 7;
    return 0;
}
