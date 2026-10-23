#include <stdio.h>
static unsigned char global[] = { 5, 8 }, second[] = { [3] = 9 };
int main(void)
{
    unsigned char actual[sizeof(unsigned short)], expected[] = { 0xa5, 0xa5 };
    int first[] = { 3, 4 }, later = 6, last[] = { [2] = 7, 8 };
    static int saved = 9, values[] = { 11, 12 };
    actual[0] = expected[1];
    printf("%d %d %d %d %d %d %d %d\n", actual[0], (int)sizeof(expected), (int)sizeof(first), later, (int)sizeof(last), last[3], second[3], values[1]);
    return global[1] != 8 || saved != 9;
}
