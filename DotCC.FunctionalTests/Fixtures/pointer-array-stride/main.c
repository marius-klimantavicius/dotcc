#include <stdio.h>
#include <stddef.h>
typedef int Row[3];
struct Pair { int key, value; };
struct Cursor { Row *row; };
static struct Cursor cursor;
static int calls;
static Row *global;
static struct Cursor *get_cursor(void) { calls++; return &cursor; }
int main(void) {
    int matrix[5][3];
    int numbers[3];
    struct Pair pairs[2];
    int index = 0;
    Row *row = &matrix[index++];
    Row *old;
    Row **global_slot = &global;
    printf("%d %d %d %d %d\n",
        (char *)(&numbers + 1) - (char *)&numbers == sizeof(numbers),
        (char *)(&matrix[1] + 1) - (char *)&matrix[1] == sizeof(matrix[1]),
        (char *)(&pairs + 1) - (char *)&pairs == sizeof(pairs),
        (char *)(row + 1) == (char *)&matrix[1][0], index);
    printf("%d %d %d %ld %ld %d\n", row + 2 == &matrix[2],
        2 + row == &matrix[2], &matrix[3] - 2 == &matrix[1],
        (long)(&matrix[4] - row), (long)(row - &matrix[4]), matrix + 2 == &matrix[2]);
    row += 2;
    old = row++;
    printf("%d %d ", old == &matrix[2], row == &matrix[3]);
    old = row--;
    printf("%d %d ", old == &matrix[3], row == &matrix[2]);
    old = ++row;
    printf("%d %d ", old == &matrix[3], row == &matrix[3]);
    old = --row;
    row -= 2;
    printf("%d %d\n", old == &matrix[2], row == matrix);
    for (row = matrix; row != &matrix[4]; row++) { }
    row--;
    printf("%d ", row == &matrix[3]);
    cursor.row = matrix;
    old = get_cursor()->row++;
    printf("%d %d %d ", old == matrix, cursor.row == &matrix[1], calls);
    get_cursor()->row += 2;
    printf("%d %d\n", cursor.row == &matrix[3], calls);
    global = matrix;
    global += 2;
    old = global++;
    printf("%d %d %d\n", old == &matrix[2], global == &matrix[3], *global_slot == global);
    return 0;
}
