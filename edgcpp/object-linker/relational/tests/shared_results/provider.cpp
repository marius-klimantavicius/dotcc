#include "program.h"

long long trace;
int alive, constructions, copies, destructions, selections, copy_failure;
void mark(int digit) { trace = trace * 10 + digit; }
void reset() {
    trace = 0;
    alive = constructions = copies = destructions = selections = copy_failure = 0;
}

Guard::Guard(int input) : event(input) { mark(event); }
Guard::~Guard() noexcept { mark(event + 2); }

Value::Value(int input, bool fail) : self(this), value(input) {
    ++constructions;
    mark(2);
    if (fail) throw input;
    ++alive;
}
Value::Value(const Value &other) : self(this), value(other.value) {
    ++copies;
    mark(5);
    if (copy_failure) throw 17;
    ++alive;
}
Value::~Value() noexcept { mark(4); ++destructions; --alive; }
bool Value::stable() const { return self == this; }

bool selected(bool value) { ++selections; mark(6); return value; }
Value make_value(int value, bool fail) {
    Guard local(1);
    return Value(value, fail);
}

Value choose_result(bool fresh, bool fail, bool fail_copy) {
    Value local(5, false);
    copy_failure = fail_copy;
    return selected(fresh) ? make_value(9, fail) : local;
}
Value choose_comma(bool fresh, bool fail, bool fail_copy) {
    Value local(5, false);
    copy_failure = fail_copy;
    return (mark(7), selected(fresh) ? make_value(9, fail) : local);
}
Value choose_nested(bool outer, bool inner, bool fail) {
    Value local(5, false);
    copy_failure = false;
    return selected(outer) ? (selected(inner) ? make_value(9, fail) : local) : local;
}
int local_result(bool fresh, bool fail, bool fail_copy) {
    Value local(5, false);
    copy_failure = fail_copy;
    {
        Value result = selected(fresh) ? make_value(9, fail) : local;
        if (!result.stable() || result.self != &result || result.value != (fresh ? 9 : 5)) return 1;
        mark(7);
    }
    return 0;
}
