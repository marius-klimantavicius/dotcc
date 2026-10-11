#include "program.h"

int exercise(bool fail) {
    Result result = forward_result(7, fail);
    if (!result.stable()) return 1;
    if (result.self != &result) return 2;
    if (result.value != 7) return 3;
    Scalar scalar = make_scalar(9);
    if (scalar.value != 9) return 4;
    return 0;
}

int main() {
    trace = 0;
    if (exercise(false) != 0) return 1;
    // Returned object is constructed in its final storage before local cleanup.
    if (trace != 1234) return 2;
    trace = 0;
    try {
        exercise(true);
        return 3;
    } catch (int &failure) {
        if (failure != 7) return 4;
        mark(5);
    }
    // Failed result construction destroys the callee local, not the result.
    if (trace != 1235) return 5;
    return 0;
}
