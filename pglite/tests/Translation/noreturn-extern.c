/* PostgreSQL c.h declaration ordering, reduced without upstream headers. */
_Noreturn extern void stop(void);
int choose(int x) { if (x) stop(); else return 42; }
_Noreturn extern void stop(void) { for (;;) {} }
int main(void) { return choose(0) - 42; }
