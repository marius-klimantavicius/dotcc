#include <stdio.h>
struct point { int x, y; };
struct methods { int (*apply)(int); int count; struct point point; };
int plus(int x) { return x + 10; }
static struct methods methods[] = {
    [2].apply = plus,
    [2].count = 7,
    [0].count = 9,
    [2].point.x = 4,
    [2].point.y = 5,
    [1] = {plus, 3, {8, 9}},
    [1].count = 6,
    [2].count = 11
};
static struct point replaced[1] = {[0].x = 8, [0] = {.y = 2}};
int main(void) {
    printf("%d %d %d %d %d %d %d %d %d\n", methods[2].apply(2), methods[2].count,
           methods[0].count, methods[0].point.y, methods[2].point.x,
           methods[2].point.y, methods[1].count, replaced[0].x, replaced[0].y);
    return 0;
}
