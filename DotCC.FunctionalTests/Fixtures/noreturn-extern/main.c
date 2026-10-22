/* PostgreSQL c.h declaration ordering, reduced without upstream headers. */
_Noreturn extern void stop(void);
int choose(int x) { if (x) stop(); else return 42; }
_Noreturn extern void stop(void) { for (;;) {} }
#include <stdio.h>
int main(void) { printf("value=%d\n", choose(0)); return 0; }
