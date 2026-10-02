/* Public formatting entry points around the pinned algorithm. The private
   FILE lock import invokes its callback under managed try/finally. */
#include "format_sink.h"
#include "format_file.h"
#include <stdio.h>
#include <limits.h>

static int edg_file_format_write(void *context, const char *bytes, size_t count)
{
    return fwrite(bytes, 1, count, (FILE *)context) == count ? 0 : -1;
}

static int edg_file_format_body(void *context, const char *format, va_list arguments)
{
    return __edg_musl_vformat((struct edg_format_sink *)context, format, arguments);
}

int vfprintf(FILE *file, const char *format, va_list arguments)
{
    struct edg_format_sink sink = { edg_file_format_write, file, 0 };
    return __edg_with_file_lock(file, edg_file_format_body, &sink, format, arguments);
}

int fprintf(FILE *file, const char *format, ...)
{
    va_list arguments;
    va_start(arguments, format);
    int result = vfprintf(file, format, arguments);
    va_end(arguments);
    return result;
}

int vprintf(const char *format, va_list arguments)
{
    return vfprintf(stdout, format, arguments);
}

int printf(const char *format, ...)
{
    va_list arguments;
    va_start(arguments, format);
    int result = vfprintf(stdout, format, arguments);
    va_end(arguments);
    return result;
}

int vsnprintf(char *destination, size_t capacity, const char *format, va_list arguments)
{
    return __edg_musl_vsnprintf(destination, capacity, format, arguments);
}

int snprintf(char *destination, size_t capacity, const char *format, ...)
{
    va_list arguments;
    va_start(arguments, format);
    int result = __edg_musl_vsnprintf(destination, capacity, format, arguments);
    va_end(arguments);
    return result;
}

int vsprintf(char *destination, const char *format, va_list arguments)
{
    return __edg_musl_vsnprintf(destination, INT_MAX, format, arguments);
}

int sprintf(char *destination, const char *format, ...)
{
    va_list arguments;
    va_start(arguments, format);
    int result = __edg_musl_vsnprintf(destination, INT_MAX, format, arguments);
    va_end(arguments);
    return result;
}
