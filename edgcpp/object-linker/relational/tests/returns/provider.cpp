#include "program.h"

int trace = 0;
void mark(int value) { trace = trace * 10 + value; }

ScopeGuard::ScopeGuard() { mark(1); }
ScopeGuard::~ScopeGuard() { mark(3); }

Result::Result(int input, bool fail) : self(this), value(input) {
    mark(2);
    if (fail) throw input;
}
Result::~Result() { mark(4); }
bool Result::stable() const { return self == this; }

Scalar::Scalar(int input) : value(input) {}

Result make_result(int value, bool fail) {
    ScopeGuard local;
    return Result(value, fail);
}

Scalar make_scalar(int value) { return {value}; }

Result forward_result(int value, bool fail) { return make_result(value, fail); }
