/* Private output-sink ABI for the selected musl printf core. */
#ifndef EDG_MUSL_FORMAT_SINK_H
#define EDG_MUSL_FORMAT_SINK_H
#include <stdarg.h>
#include <stddef.h>
#include <limits.h>
struct edg_format_sink {
    int (*write)(void *, const char *, size_t);
    void *context;
    int error;
};
static int edg_sink_error(struct edg_format_sink *f) { return f->error; }
static size_t edg_sink_write(const void *bytes, size_t count, struct edg_format_sink *f) {
    if(f->write(f->context, bytes, count)<0) { f->error=1; return 0; }
    return count;
}
int __edg_musl_vformat(struct edg_format_sink *, const char *, va_list);
int __edg_musl_vsnprintf(char *, size_t, const char *, va_list);
#endif
