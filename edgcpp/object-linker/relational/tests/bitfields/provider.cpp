#include "program.h"

int effects = 0;
int trace = 0;

Packed* selected(Packed* value) {
    ++effects;
    trace = trace * 10 + 2;
    return value;
}

int selected_value() {
    trace = trace * 10 + 1;
    return -9;
}

void ordered_store(Packed* value) {
    selected(value)->signed_value = selected_value();
}

void update(LongShape* long_value, ShortShape* short_value, Packed* value) {
    long_value->__is_long_ = 0;
    long_value->__cap_ = 0x7123456789abcdefUL;
    short_value->__is_long_ = 1;
    short_value->__size_ = 109;
    value->signed_value = -11;
    value->low = 5;
    value->crossing = 257;
    value->enabled = true;
}
