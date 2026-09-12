/* C11 alignment matters to CXPLAT_LOCK and CXPLAT_EVENT. */
#include <stdio.h>
#include <stdalign.h>
struct Aligned { alignas(16) unsigned char value; };
int main(void) {
    printf("%lu %lu\n", (unsigned long)sizeof(struct Aligned),
           (unsigned long)alignof(struct Aligned));
    return 0;
}
