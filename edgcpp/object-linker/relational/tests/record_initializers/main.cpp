#include "program.h"
int main() {
    if (Hash{}(11) != 33) return 1;
    if (fixed_value.first != 7 || fixed_value.second != 0 || fixed_value.third != 9) return 2;
    Pair value = forward_pair();
    if (value.first != 4 || value.second != 5) return 3;
    if (local_records()) return 4;
    return 0;
}
