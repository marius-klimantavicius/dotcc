/* Retain the selected libc++/managed libc stdio headers, then declare the
   POSIX stream/descriptor bridge used by the original EDG sources. */
#ifndef EDG_SELF_STDIO_H
#define EDG_SELF_STDIO_H
#include_next <stdio.h>
#ifdef __cplusplus
extern "C" {
#endif
int fileno(FILE *);
FILE *fdopen(int, const char *);
#ifdef __cplusplus
}
#endif
#endif
