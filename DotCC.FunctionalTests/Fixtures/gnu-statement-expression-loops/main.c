#include <stdio.h>

int main(void)
{
    int visits = 0;
    int eager = ({
        int value = 0;
        do {
            ++value;
            if (value == 2) continue;
            ++visits;
            if (value == 4) break;
        } while (value < 10);
        value;
    });
    int lazy = 1 && ({
        int total = 0;
        for (int i = 0; i < 5; ++i) {
            if (i == 1) continue;
            total += i;
            if (i == 3) break;
        }
        while (total < 7) { ++total; }
        total == 7;
    });
    int skipped = 0 && ({ do { visits += 100; } while (0); 1; });
    printf("%d %d %d %d\n", eager, visits, lazy, skipped);
    return 0;
}
