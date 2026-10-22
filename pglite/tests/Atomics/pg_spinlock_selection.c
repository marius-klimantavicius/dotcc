/* Actual-header slock_t selection/layout reduction, shared with full input. */
#include "c.h"
#include "storage/s_lock.h"

size_t dpga_spinlock_width(void) { return sizeof(slock_t); }

#ifdef DPGA_NATIVE_MAIN
#include <stdio.h>
#undef printf
int main(void)
{
    printf("%zu\n", dpga_spinlock_width());
    return sizeof(slock_t) == 1 ? 0 : 1;
}
#endif
