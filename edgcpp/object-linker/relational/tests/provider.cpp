#include "program.h"
int initial_bonus = 5;
int accumulate(const Point *point, int count)
{
    int result = 0;
    for (int i = 0; i < count; ++i) {
        if (i == 2) continue;
        result += combine(point->total(), i);
    }
    return result + initial_bonus;
}
