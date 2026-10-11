#include "program.h"
int main()
{
    Point point;
    point.x = 3;
    point.y = 4;
    int expected = combine(7, 0) + combine(7, 1) + combine(7, 3) + initial_bonus;
    if (accumulate(&point, 4) != expected) return 1;
    int i = 3;
    while (i > 0) --i;
    return i;
}
