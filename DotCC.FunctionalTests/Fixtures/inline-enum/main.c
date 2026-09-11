#include <stdio.h>
struct state { enum phase { NONE, READY = 7, DONE } phase; int value; };
int main(void)
{
    struct state s = {READY, 42};
    enum phase p = DONE;
    enum { FULL, PSK, PSK_DHE } mode = PSK_DHE;
    printf("phase=%d done=%d mode=%d value=%d\n", s.phase, p, mode, s.value);
    return 0;
}
