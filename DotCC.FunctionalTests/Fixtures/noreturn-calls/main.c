#include <stdio.h>
#include <stdlib.h>

_Noreturn void die(void) { exit(90); }
int leaf(int x) { if (x) return x; abort(); }
int branches(int x) { if (x) return 2; else exit(91); }
int switches(int x) { switch (x) { case 0: return 3; default: die(); } }
int commas(int x) { if (x) return 4; (void)(puts("unexpected"), die()); }
int conditional(int x) { if (x) return 5; x ? die() : abort(); }
int braceless(int x) { if (x) die(); else return 6; }

int main(int argc, char **argv)
{
    if (argc > 1) return branches(0);
    printf("%d %d %d %d %d %d\n", leaf(1), branches(1), switches(0), commas(1), conditional(1), braceless(0));
    return 0;
}
