#include <stdio.h>

struct cell { int number; const char *label; };
static struct cell table[2][3] = {
    [0][1] = {2, "old"}, [1][2] = {7, "last"}, [0][1] = {3, "three"}
};
static int sparse[2][3] = { [1][1] = 4, 5, [0] = {1, 2} };
static int replaced[2][3] = { [1][2] = 9, [1] = {6} };

int main(void)
{
    int local[2][2][2] = { [1][0][1] = 8, 9, [0][1] = {2, 3} };
    for (int row = 0; row < 2; ++row)
        for (int col = 0; col < 3; ++col)
            printf("%d:%s%c", table[row][col].number,
                   table[row][col].label ? table[row][col].label : "-", col == 2 ? '\n' : ' ');
    for (int row = 0; row < 2; ++row)
        printf("%d %d %d\n", sparse[row][0], sparse[row][1], sparse[row][2]);
    printf("%d %d %d\n", replaced[1][0], replaced[1][1], replaced[1][2]);
    printf("%d %d %d %d %d\n", local[1][0][0], local[1][0][1], local[1][1][0], local[0][1][0], local[0][1][1]);
    return 0;
}
