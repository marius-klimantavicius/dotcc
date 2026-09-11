#include <stdio.h>
#include <stdarg.h>

struct logger {
    void (*cb)(const char *, ...) __attribute__((format(printf, 1, 2)));
};

__attribute__((format(printf, 1, 2)))
static void output(const char *format, ...)
{
    va_list args;
    va_start(args, format);
    printf("callback=%d\n", va_arg(args, int));
    va_end(args);
}

int main(void)
{
    struct logger logger = {output};
    logger.cb("callback=%d\n", 42);
    return 0;
}
