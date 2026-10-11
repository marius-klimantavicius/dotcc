#include "program.h"
int *shared_pointer = nullptr;
const unsigned char *shift_bytes(const unsigned char *value, long offset, bool subtract) {
    if (subtract) value -= offset;
    else value += offset;
    return value;
}
int *shift_words(int *value, long offset, bool subtract) {
    if (subtract) value -= offset;
    else value += offset;
    return value;
}
int **selected_target(int **value, int *trace, int *calls) {
    *trace = *trace * 10 + 2;
    ++*calls;
    return value;
}
long selected_offset(long value, int *trace, int *calls) {
    *trace = *trace * 10 + 1;
    ++*calls;
    return value;
}
