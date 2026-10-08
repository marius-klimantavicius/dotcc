#include "lifetime_fixture.h"
#define CLEANUP(function) __attribute__((cleanup(function)))

int return_event(int* trace) { *trace = *trace * 10 + 9; return *trace; }
int lifetime_return(int* trace, int early) {
    int* first CLEANUP(cleanup_one) = trace;
    if (early) return return_event(trace);
    int* second CLEANUP(cleanup_two) = trace;
    return return_event(trace);
}
void lifetime_nested(int* trace) {
    int* first CLEANUP(cleanup_one) = trace;
    {
        int* second CLEANUP(cleanup_two) = trace;
        int* third CLEANUP(cleanup_three) = trace;
    }
    *trace = *trace * 10 + 8;
}
void lifetime_loops(int* trace) {
    for (int i = 0; i < 3; ++i) {
        int* guard CLEANUP(cleanup_one) = trace;
        if (i == 0) continue;
        {
            int* inner CLEANUP(cleanup_two) = trace;
            if (i == 1) continue;
            break;
        }
    }
}
void lifetime_for_initializer(int* trace) {
    for (int* guard CLEANUP(cleanup_three) = trace; *trace < 2; ++*trace) {
        if (*trace == 0) continue;
        break;
    }
}
void lifetime_for_uninitialized(int* trace) {
    for (int* guard CLEANUP(cleanup_two); *trace == 0;) {
        guard = trace;
        *trace = 1;
        break;
    }
}
void lifetime_switch(int* trace, int selector) {
    switch (selector) {
    case 1: {
        int* guard CLEANUP(cleanup_one) = trace;
        break;
    }
    case 2: {
        int* guard CLEANUP(cleanup_two) = trace;
        return;
    }
    default: break;
    }
}
void lifetime_array_scalar(int* trace) {
    int digit = 5;
    int* values[2] CLEANUP(cleanup_array);
    values[0] = trace;
    values[1] = &digit;
    unsigned long encoded CLEANUP(cleanup_scalar) = (unsigned long)trace;
}
void lifetime_uninitialized(int* trace) {
    int unused CLEANUP(cleanup_write_only);
    int* guard CLEANUP(cleanup_two);
    guard = trace;
}
void lifetime_constant_array(int* trace) {
    int digit = 6;
    int* values[2] CLEANUP(cleanup_array) = {0, 0};
    values[0] = trace;
    values[1] = &digit;
}
int lifetime_failure(int* trace, int divisor, int fail_initializer) {
    int* first CLEANUP(cleanup_one) = trace;
    int amount = lifetime_divide(1);
    int initializer_divisor = fail_initializer ? divisor : 1;
    int* input[2];
    input[0] = trace;
    input[1] = &initializer_divisor;
    int* second CLEANUP(cleanup_two) = lifetime_pointer_initializer(input);
    return amount + lifetime_divide(divisor);
}
int main(void) {
    int trace = 0;
    if (lifetime_return(&trace, 1) != 9 || trace != 91) return 1;
    trace = 0;
    if (lifetime_return(&trace, 0) != 9 || trace != 921) return 2;
    trace = 0; lifetime_nested(&trace);
    if (trace != 3281) return 3;
    trace = 0; lifetime_loops(&trace);
    if (trace != 12121) return 4;
    trace = 0; lifetime_for_initializer(&trace);
    if (trace != 13) return 5;
    trace = 0; lifetime_switch(&trace, 1);
    if (trace != 1) return 6;
    trace = 0; lifetime_switch(&trace, 2);
    if (trace != 2) return 7;
    trace = 0; lifetime_switch(&trace, 0);
    if (trace != 0) return 8;
    trace = 0; lifetime_array_scalar(&trace);
    if (trace != 45) return 9;
    trace = 0; lifetime_uninitialized(&trace);
    if (trace != 2) return 10;
    trace = 0;
    if (lifetime_failure(&trace, 2, 1) != 12 || trace != 21) return 11;
    trace = 0;
    if (lifetime_failure(&trace, 2, 0) != 12 || trace != 21) return 12;
    trace = 0; lifetime_for_uninitialized(&trace);
    if (trace != 12) return 13;
    trace = 0; lifetime_constant_array(&trace);
    if (trace != 6) return 14;
    return 0;
}
