/* Authored C17 facade: pass this translated owner's live environment vector. */
#include <stdlib.h>
#ifdef __cplusplus
extern "C" {
#endif
int __edg_system(const char *, char **);
int system(const char *command)
{
    return __edg_system(command, environ);
}
#ifdef __cplusplus
}
#endif
