/* Native-only process entrypoint; qualification.c is the actual translated Unit. */
#include <stdio.h>
#include <stdlib.h>
int ZqRun(unsigned group);
int main(int argc, char** argv)
{
    char* end;
    unsigned long group;
    if (argc != 2) { fputs("Expected numeric qualification group\n", stderr); return 2; }
    group = strtoul(argv[1], &end, 10);
    if (!argv[1][0] || *end || group > 5) { fputs("Invalid qualification group\n", stderr); return 2; }
    return ZqRun((unsigned)group);
}
