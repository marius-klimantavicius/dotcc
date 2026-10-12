#include "program.h"
int check_case(int mode) {
    completed = 0;
    destroyed = 0;
    order_error = false;
    receiver_constructed = 0;
    receiver_destroyed = 0;
    result_constructed = 0;
    result_destroyed = 0;
    fail_id = mode == 1 || mode == 2 ? mode : 0;
    fail_call = mode == 3;
    int caught = 0;
    try {
        Result result = evaluate();
        if (result.self != &result || result.value != 3 || mode != 0) return 1;
    } catch (int error) {
        caught = error;
    }
    if (order_error || receiver_constructed != 1 || receiver_destroyed != 1) return 2;
    int expected = completed == 12 ? 21 : completed == 21 ? 12 : completed;
    if (destroyed != expected) return 3;
    if (mode == 0) return caught == 0 && (completed == 12 || completed == 21) && result_constructed == 1 && result_destroyed == 1 ? 0 : 4;
    if (result_constructed != 0 || result_destroyed != 0) return 5;
    if (mode == 1) return caught == 21 && (completed == 0 || completed == 2) ? 0 : 6;
    if (mode == 2) return caught == 22 && (completed == 0 || completed == 1) ? 0 : 7;
    return caught == 30 && (completed == 12 || completed == 21) ? 0 : 8;
}
int check_unordered_temporaries() {
    for (int mode = 0; mode < 4; ++mode) {
        int error = check_case(mode);
        if (error != 0) return error;
    }
    return 0;
}
int main() { return check_unordered_temporaries(); }
