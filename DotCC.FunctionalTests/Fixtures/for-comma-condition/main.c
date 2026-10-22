#include <stdio.h>

int main(void)
{
    int probes = 0, left = 0, right = 0, total = 0, j = 0;
    for (int i = 0; ++probes, left = i + 10, right = i * 2, i < 4; ++i)
    {
        if (i == 1) continue;
        total += right;
    }
    printf("%d %d %d %d\n", probes, left, right, total);
    for (; ++probes, ++j, j < 3; ) total += j;
    for (j = 0; ++probes, j < 2; ++j) ++total;
    printf("%d %d %d\n", probes, j, total);
    return 0;
}
