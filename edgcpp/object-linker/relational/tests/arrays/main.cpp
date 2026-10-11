#include "program.h"
int main() {
    for (int i = 0; i < 10; ++i) {
        if (digits[2 * i] != '0' || digits[2 * i + 1] != '0' + i) return 1;
    }
    if (shared_values[0] != 11 || shared_values[1] != 12 || shared_values[2] != 0 || shared_values[3] != 0) return 2;
    int *first = shared_data();
    int *second = shared_data();
    if (first != second) return 3;
    shared_values[1] += 3;
    if (first[1] != 15) return 4;
    if (local_arrays()) return 5;
    int *zero = zero_data();
    if (zero[0] != 0 || zero[1] != 0 || zero[2] != 0) return 6;
    int cleanup = 0;
    array_cleanup(&cleanup, false);
    if (cleanup != 40) return 7;
    cleanup = 0;
    try { array_cleanup(&cleanup, true); return 8; }
    catch (int error) { if (error != 7 || cleanup != 40) return 9; }
    return 0;
}
