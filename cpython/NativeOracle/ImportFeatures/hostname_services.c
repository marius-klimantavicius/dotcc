/* Prepared policy-facing shared service fixture. No interpreter or replacement
 * service. The launcher supplies the independently read real /proc hostname. */
#include "pyconfig.h"
#include <stddef.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <wchar.h>
#ifdef IMPORT_NATIVE_ORACLE
extern int *__errno_location(void);
#define HOST_ERRNO (*__errno_location())
#else
#define HOST_ERRNO errno
#endif

int main(int argc, char **argv)
{
    if (sizeof(size_t) != 8 || sizeof(unsigned long) != 8 || sizeof(int) != 4 ||
        sizeof(wchar_t) != 2 || (wchar_t)-1 < 0) return 21;
#ifdef HAVE_GETHOSTNAME
    int (*query)(char *, size_t) = gethostname;
    unsigned char storage[1030];
    if (argc != 2 || !argv[1][0]) return 22;
    size_t length = strlen(argv[1]);
    if (length >= 1023) return 23;
    memset(storage, 0xa5, sizeof storage);
    HOST_ERRNO = EDOM;
    /* Exact pin wrapper capacity/expression: implicit int-to-size_t conversion. */
    if (gethostname((char *)storage + 2, (int)1024 - 1) != 0 || HOST_ERRNO != EDOM) return 24;
    if (memcmp(storage + 2, argv[1], length) || storage[2 + length] != 0) return 25;
    if (storage[0] != 0xa5 || storage[1] != 0xa5 || storage[3 + length] != 0xa5) return 26;
    memset(storage, 0xa5, sizeof storage);
    HOST_ERRNO = EDOM;
    if (query((char *)storage + 2, length + 1) || HOST_ERRNO != EDOM ||
        memcmp(storage + 2, argv[1], length) || storage[2 + length] != 0 || storage[3 + length] != 0xa5) return 27;
    memset(storage, 0xa5, sizeof storage);
    if (query((char *)storage + 2, length) != -1 || HOST_ERRNO != ENAMETOOLONG ||
        memcmp(storage + 2, argv[1], length) || storage[2 + length] != 0xa5) return 28;
    if (query((char *)storage + 2, 0) != -1 || HOST_ERRNO != ENAMETOOLONG || storage[1] != 0xa5) return 29;
    HOST_ERRNO = EDOM;
    if (query((char *)storage + 2, (size_t)2147483713UL) || HOST_ERRNO != EDOM ||
        memcmp(storage + 2, argv[1], length) || storage[2 + length] != 0) return 30;
    printf("hostname selected=1 failures=0\n");
#else
    /* An unselected fixture must not introduce an unconditional hostname call. */
    printf("hostname selected=0 failures=0\n");
#endif
    return 0;
}
