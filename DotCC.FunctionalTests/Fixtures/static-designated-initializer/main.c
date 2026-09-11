#include <stdio.h>
struct point { const char *name; int count; int untouched; };
static void first(void)
{
    static struct point point = {.name = "first", .count = 7};
    printf("%s count=%d untouched=%d\n", point.name, point.count++, point.untouched);
}
static void second(void)
{
    static struct point point = {.name = "second"};
    printf("%s count=%d untouched=%d\n", point.name, point.count++, point.untouched);
}
int main(void) { first(); first(); second(); first(); return 0; }
