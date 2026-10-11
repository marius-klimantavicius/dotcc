#include "program.h"
int main() {
    int values[3] = {1, 2, 3};
    int sum = 0;
    for (int offset = 2; int &value : values) { value += offset; }
    for (int value : values) { if (value == 4) continue; sum += value; }
    if (sum != 8 || values[1] != 4) return 1;
    Range range{values, 3};
    expected_alive = 1;
    int bodies = 0;
    for (Watch outer{20}; Watch item : range) {
        if (alive != 2) return 2;
        ++bodies;
        if (item.value == 3) continue;
        if (item.value == 5) break;
    }
    if (alive != 0 || bodies != 3 || bad_steps != 0 || begins != 1 || ends != 1) return 3;
    expected_alive = 0;
    if (early_return(range) != 3 || alive != 0) return 4;
    try { for (Watch item : range) { throw item.value; } } catch (int value) {
        if (value != 3 || alive != 0) return 5;
    }
    values[1] = -1;
    bodies = 0;
    try { for (Watch item : range) { ++bodies; } return 6; } catch (int value) {
        if (value != 17 || alive != 0 || bodies != 1 || bad_steps != 0) return 7;
    }
    int nested = 0;
    for (int value : values) {
        for (int inner : values) { if (inner < 0) continue; ++nested; break; }
        if (value < 0) break;
    }
    if (nested != 2 || alive != 0) return 8;
    return 0;
}
