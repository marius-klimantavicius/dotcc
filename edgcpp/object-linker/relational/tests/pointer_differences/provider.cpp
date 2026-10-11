#include "program.h"
Difference char_distance(const char *left, const char *right) { return left - right; }
Difference int_distance(const int *left, const int *right) { return left - right; }
int *selected_left(int *value, int *calls) { ++*calls; return value; }
int *selected_right(int *value, int *calls) { ++*calls; return value; }
Difference *selected_destination(Difference *value, int *left_calls, int *right_calls, int *state) {
    *state = *left_calls == 1 && *right_calls == 1 ? 7 : 9;
    return value;
}
