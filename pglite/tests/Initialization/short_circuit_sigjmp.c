/* pg_get_line_append arms its interrupt target only when a context exists. */
#include <setjmp.h>

static int guarded_jump(int enabled)
{
    sigjmp_buf target;
    if (enabled && sigsetjmp(target, 1) != 0)
        return 42;
    if (enabled)
        siglongjmp(target, 7);
    return 3;
}

int main(void) { return guarded_jump(0) == 3 && guarded_jump(1) == 42 ? 0 : 1; }
