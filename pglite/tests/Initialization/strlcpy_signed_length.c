/* Native/emitted reduction: a source-defined strlcpy must retain the C
 * size_t parameter conversions for signed caller lengths after object link. */
#ifdef DPG_CONFIGURED_HEADERS
#include "postgres_fe.h"
#else
#include <stddef.h>
#include <string.h>
extern size_t strlcpy(char *, const char *, size_t);
#endif

int main(void)
{
    char output[8];
    int small = 4;
    long large = 8;
    if (strlcpy(output, "abcdef", small) != 6 || strcmp(output, "abc") != 0)
        return 1;
    if (strlcpy(output, "abcdef", large) != 6 || strcmp(output, "abcdef") != 0)
        return 2;
    return 0;
}
