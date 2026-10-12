#include "program.h"
int check_case(int mode) {
    trace = 0;
    accumulator.value = 0;
    fail_input = mode == 1;
    fail_receiver = mode == 2;
    fail_call = mode == 3;
    int caught = 0;
    try {
        Accumulator& result = perform();
        if (&result != &accumulator || result.value != 7 || trace != 1234) return 1;
    } catch (int error) {
        caught = error;
    }
    if (mode == 0) return caught == 0 && accumulator.value == 7 && trace == 1234 ? 0 : 2;
    if (accumulator.value != 0) return 3;
    if (mode == 1) return caught == 7 && trace == 1 ? 0 : 4;
    if (mode == 2) return caught == 8 && trace == 124 ? 0 : 5;
    return caught == 9 && trace == 1234 ? 0 : 6;
}
int check_calls() {
    for (int mode = 0; mode < 4; ++mode) {
        int error = check_case(mode);
        if (error != 0) return error;
    }
    return 0;
}
int main() { return check_calls(); }
