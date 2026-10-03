/* Authored portable facade: resolve TLS errno before any stream operation. */
#include <stdio.h>
#include <errno.h>
#include <string.h>
#ifdef __cplusplus
extern "C" {
#endif
void __edg_perror(FILE *, const char *, const char *);
void perror(const char *message)
{
    const char *text = strerror(errno);
    __edg_perror(stderr, message, text);
}
#ifdef __cplusplus
}
#endif
