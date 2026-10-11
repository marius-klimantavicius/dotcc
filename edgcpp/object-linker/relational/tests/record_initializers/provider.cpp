#include "program.h"
const Derived fixed_value = {{7}, 9};
Pair make_pair() { return {4, 5}; }
Pair forward_pair() { return make_pair(); }
int local_records() {
    Pair partial = {2};
    if (partial.first != 2 || partial.second != 0) return 1;
    Pair designated = {.second = 7};
    if (designated.first != 0 || designated.second != 7) return 2;
    Nested nested = {{3, 4}, {5}};
    if (nested.pair.first != 3 || nested.pair.second != 4 || nested.values[0] != 5 || nested.values[1] != 0 || nested.values[2] != 0) return 3;
    Derived derived = {{11, 12}, 13};
    if (derived.first != 11 || derived.second != 12 || derived.third != 13) return 4;
    Constructed constructed;
    if (constructed.first != 6 || constructed.second != 0 || constructed.third != 8) return 5;
    return 0;
}
