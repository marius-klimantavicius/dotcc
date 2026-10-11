#include "program.h"
int *shared_data() { return shared_values; }
int *zero_data() { return zero_values; }
const char digits[20] = {
    '0', '0', '0', '1', '0', '2', '0', '3', '0', '4',
    '0', '5', '0', '6', '0', '7', '0', '8', '0', '9'
};
int shared_values[4] = {11, 12};
int zero_values[3];
int sum(int *values, int count) {
    int result = 0;
    for (int i = 0; i < count; ++i) result += values[i];
    return result;
}
int local_arrays() {
    int partial[5] = {3, 4};
    if (sum(partial, 5) != 7 || partial[4] != 0) return 1;
    partial[4] = 9;
    if (sum(partial, 5) != 16) return 2;
    int nested[2][3] = {{1}, {2, 3}};
    if (nested[0][0] != 1 || nested[0][2] != 0 || nested[1][0] != 2 || nested[1][1] != 3 || nested[1][2] != 0) return 3;
    int zeros[4] = {};
    if (sum(zeros, 4) != 0) return 4;
    int total = 0;
    for (int i = 0; i < 3; ++i) {
        int iteration[2] = {1};
        total += iteration[1];
        iteration[1] = 99;
    }
    if (total != 0) return 5;
    return 0;
}

struct ArrayObserver {
    int *values;
    int *result;
    ArrayObserver(int *v, int *r) : values(v), result(r) {}
    ~ArrayObserver() { *result = values[0] + values[1]; }
};
void array_cleanup(int *result, bool fail) {
    int values[2] = {10, 20};
    ArrayObserver observer(values, result);
    values[1] = 30;
    if (fail) throw 7;
}
