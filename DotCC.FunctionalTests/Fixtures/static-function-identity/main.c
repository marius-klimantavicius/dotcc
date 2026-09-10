#include <stdio.h>
typedef int (*Callback)(int);
static int same(int value) { return value + 1; }
Callback other_same(void);
int main(void) {
    Callback first = same;
    Callback second = other_same();
    printf("%d %d %d\n", first != second, first(41), second(41));
    return 0;
}
