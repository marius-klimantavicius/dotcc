/* Reduction of postgres.c's next failure in the pre-spinlock compiler snapshot.
 * Preserve PostgreSQL's actual StaticAssertExpr macro and its do/while body.
 */
#include "c.h"

int dpgs_static_assert_expr(int value)
{
    return (StaticAssertExpr(sizeof(uint64) == 8, "uint64 width"), value + 1);
}

#ifdef DPGS_NATIVE_MAIN
#include <stdio.h>
#undef printf
int main(void)
{
    int value = dpgs_static_assert_expr(41);
    printf("%d\n", value);
    return value == 42 ? 0 : 1;
}
#endif
