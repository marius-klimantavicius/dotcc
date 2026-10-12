#include "program.h"
int check_case(int mode) {
    trace = 0;
    int caught = 0;
    try {
        Result result = wrapper(mode == 1, mode == 2);
        if (result.self != &result || result.value != 18 || trace != 1243) return 1;
    } catch (int error) {
        caught = error;
    }
    if (mode == 0) return caught == 0 && trace == 12435 ? 0 : 2;
    if (mode == 1) return caught == 7 && trace == 1 ? 0 : 3;
    return caught == 8 && trace == 123 ? 0 : 4;
}
int check_temporaries() {
    for (int mode = 0; mode < 3; ++mode) {
        int error = check_case(mode);
        if (error != 0) return error;
    }
    return 0;
}
int main() { return check_temporaries(); }
