#include <stdio.h>
static int plus(int value) { return value + 1; }
static int twice(int value) { return value * 2; }
static int (*methods[][2])(int) = {
    [0] = { &plus, &twice },
    [2] = { &twice, &plus },
};
int main(void)
{
    int (*local[][2])(int) = { { &twice, &plus } };
    int sparse[][2] = { [1][1] = 7, 8 };
    int nested[][2][2] = { [1][0] = { 3 }, [0][1][1] = 4 };
    printf("%d %d %d %d %d\n", (int)(sizeof methods / sizeof methods[0]),
           methods[0][0](3), methods[2][0](4), methods[1][0] == 0, local[0][1](5));
    printf("%d %d %d %d %d\n", (int)(sizeof sparse / sizeof sparse[0]), sparse[2][0],
           (int)(sizeof nested / sizeof nested[0]), nested[1][0][0], nested[0][1][1]);
    return 0;
}
