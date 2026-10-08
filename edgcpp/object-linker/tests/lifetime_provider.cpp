#include "lifetime_fixture.h"
void cleanup_one(int** value) { **value = **value * 10 + 1; }
void cleanup_two(int** value) { **value = **value * 10 + 2; }
int cleanup_three(void* value) {
    int** pointer = (int**)value;
    **pointer = **pointer * 10 + 3;
    return 123;
}
void cleanup_array(int* (*value)[2]) {
    int* trace = (*value)[0];
    *trace = *trace * 10 + *(*value)[1];
}
void cleanup_scalar(unsigned long* value) {
    int* trace = (int*)*value;
    *trace = *trace * 10 + 4;
}
void cleanup_write_only(int* value) { *value = 7; }
int lifetime_divide(int divisor) { return 8 / divisor; }
int* lifetime_pointer_initializer(int** input) {
    lifetime_divide(*input[1]);
    return input[0];
}
