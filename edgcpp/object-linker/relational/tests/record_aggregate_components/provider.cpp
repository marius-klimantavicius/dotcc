#include "program.h"

int trace = 0;
int selections = 0;

int mark(int event, int value) {
    trace = trace * 10 + event;
    return value;
}

int *mark_owner(int event, int *owner) {
    trace = trace * 10 + event;
    return owner;
}

TestValue &selected(TestValue &value) {
    mark(2, 0);
    ++selections;
    return value;
}

TestValue make_test(int *alias, int expected, unsigned mask) {
    return TestValue{alias, mark(4, expected), mask};
}

Backoff from_reference(int *owner, TestValue &test) {
    Backoff backoff = {mark_owner(1, owner), test, mark(3, 29)};
    return backoff;
}
