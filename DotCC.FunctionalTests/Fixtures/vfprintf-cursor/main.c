#include <stdio.h>
#include <stdarg.h>
#include <string.h>
#include <errno.h>

static int file_format(FILE *stream, const char *format, ...) {
    va_list arguments;
    va_start(arguments, format);
    int result = vfprintf(stream, format, arguments);
    va_end(arguments);
    return result;
}

static int stdout_format(const char *format, ...) {
    va_list arguments;
    va_start(arguments, format);
    int result = vprintf(format, arguments);
    va_end(arguments);
    return result;
}

static int buffer_format(char *buffer, const char *format, ...) {
    va_list arguments;
    va_start(arguments, format);
    int result = vsprintf(buffer, format, arguments);
    va_end(arguments);
    return result;
}

static int copy_after_skip(FILE *stream, const char *format, ...) {
    va_list arguments, copy;
    va_start(arguments, format);
    int skipped = va_arg(arguments, int);
    va_copy(copy, arguments);
    int first = vfprintf(stream, format, arguments);
    int second = vfprintf(stream, format, copy);
    va_end(copy);
    va_end(arguments);
    return first + second + skipped;
}

int main(void) {
    FILE *stream = tmpfile();
    if (!stream) return 1;
    int count = -1;
    int result = file_format(stream, "[%*.*s]|%lld|%zu%n|%c|%s", -5, 3, "abcdef",
                             -9000000000LL, (size_t)42, &count, 0, "\xc3\xa9");
    fflush(stream);
    long size = ftell(stream);
    rewind(stream);
    unsigned char bytes[128];
    int got = fread(bytes, 1, sizeof(bytes), stream);
    printf("file=%d bytes=%d size=%ld count=%d zero=%d tail=%u,%u\n",
           result, got, size, count, bytes[count + 1], bytes[got - 2], bytes[got - 1]);
    fclose(stream);

    stream = tmpfile();
    result = copy_after_skip(stream, "%s:%d|", 10, "next", 42);
    rewind(stream);
    got = fread(bytes, 1, sizeof(bytes) - 1, stream);
    bytes[got] = 0;
    printf("copy=%s result=%d\n", bytes, result);
    fclose(stream);

    char buffer[64];
    result = buffer_format(buffer, "%s:%d:%c", "ok", 7, 255);
    printf("buffer=%d last=%u terminator=%d\n", result, (unsigned char)buffer[result - 1], buffer[result]);
    result = stdout_format("stdout=%d %s\n", 42, "ok");
    printf("stdout-count=%d\n", result);

    char filename[L_tmpnam];
    if (!tmpnam(filename)) return 2;
    stream = fopen(filename, "w");
    if (!stream) return 3;
    fclose(stream);
    stream = fopen(filename, "r");
    if (!stream) return 4;
    errno = 0;
    result = file_format(stream, "%s:%d", "unwritable", 42);
    printf("readonly=%d error=%d errno=%d\n", result < 0, ferror(stream) != 0, errno != 0);
    fclose(stream);
    remove(filename);
    return 0;
}
