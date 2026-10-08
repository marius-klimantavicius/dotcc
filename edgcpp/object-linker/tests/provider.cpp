#include "fixture.h"

static int helper(int value) { return value + DOTOBJ_BASE; }

int from_a(int value) {
    int result = helper(value);
    if (result > 5) return result + 2;
    return result - 2;
}

int bump(int event) {
    int required = event + 1;
    return required;
}
