#include <stdio.h>

static char *place;
typedef int (*action_t)(int);
static action_t action;
static int plus_five(int value) { return value + 5; }

int main(void)
{
    char text[] = "AZ";
    int letter = *(place = text + 1);
    int result = (action = plus_five)(5);
    int first = (place = text)[0];
    place = text;
    int next = *(place += 1);
    printf("%d %d %d %d\n", letter, result, first, next);
    return 0;
}
