/* Dot-gen private buffer sink. Formatting algorithms are in the pinned musl
   core; this adapter owns neither the caller's buffer nor its argument pack. */
#include "format_sink.h"
#include <string.h>

struct edg_print_buffer {
    char *next;
    size_t capacity;
};

static int edg_print_buffer_write(void *context, const char *bytes, size_t count)
{
    struct edg_print_buffer *buffer = context;
    size_t copied = count < buffer->capacity ? count : buffer->capacity;
    if (copied) {
        memcpy(buffer->next, bytes, copied);
        buffer->next += copied;
        buffer->capacity -= copied;
    }
    if (buffer->next) *buffer->next = 0;
    /* Truncation does not change the required output count or report failure. */
    return 0;
}

int __edg_musl_vsnprintf(char *destination, size_t capacity, const char *format, va_list arguments)
{
    struct edg_print_buffer buffer = { capacity ? destination : 0, capacity ? capacity - 1 : 0 };
    struct edg_format_sink sink = { edg_print_buffer_write, &buffer, 0 };
    if (capacity) *destination = 0;
    return __edg_musl_vformat(&sink, format, arguments);
}
