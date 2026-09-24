#define _DEFAULT_SOURCE 1
#include <sys/param.h>
#include <syslog.h>
#include <stdio.h>

#if !defined(BYTE_ORDER) || (BYTE_ORDER != BIG_ENDIAN && BYTE_ORDER != LITTLE_ENDIAN)
#error Undefined or invalid BYTE_ORDER
#endif

int main(void) {
    unsigned int marker = 1;
    int little = *(unsigned char *)&marker;
    int priority = LOG_MAKEPRI(LOG_LOCAL3, LOG_WARNING);
    printf("%d %d %d %d\n", (BYTE_ORDER == LITTLE_ENDIAN) == little,
           NBBY, MIN(8, 5), MAX(3, 7));
    printf("%d %d %d %d\n", LOG_FAC(priority), LOG_PRI(priority),
           LOG_MASK(LOG_ERR), LOG_UPTO(LOG_NOTICE));
    return 0;
}
