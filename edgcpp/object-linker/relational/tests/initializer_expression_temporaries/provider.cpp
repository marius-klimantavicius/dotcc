#include "program.h"
int trace = 0;
void append(int event) { trace = trace * 10 + event; }
Value::Value(int input, bool fail) : value(input), self(this) {
    append(1);
    if (fail) throw 7;
}
Value::~Value() noexcept { append(3); }
Result::Result(int input) : value(input), self(this) { append(4); }
Result::~Result() noexcept { append(5); }
Value make_value(bool fail) { return Value(17, fail); }
Result combine(const Value& source, bool fail) {
    append(2);
    if (source.self != &source) throw 9;
    if (fail) throw 8;
    return Result(source.value + 1);
}
Result wrapper(bool fail_temporary, bool fail_call) {
    return combine(make_value(fail_temporary), fail_call);
}
