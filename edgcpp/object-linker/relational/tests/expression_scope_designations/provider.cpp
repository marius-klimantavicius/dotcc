#include "program.h"
int trace = 0;
int cleaned = 0;
bool fail_receiver = false;
bool fail_call = false;
bool fail_payload = false;
Accumulator accumulator = {0};
void append(int event) { trace = trace * 10 + event; }
Input::Input(int value, bool fail) : value(value), self(this) {
    append(1);
    if (fail) throw 7;
}
Input::~Input() noexcept {
    append(4);
    ++cleaned;
    accumulator.value = accumulator.value + 100;
}
Accumulator &get_accumulator() {
    append(2);
    if (fail_receiver) throw 8;
    return accumulator;
}
Accumulator &Accumulator::operator+=(const Input &input) {
    append(3);
    if (this != &accumulator || input.self != &input) throw 99;
    if (fail_call) throw 9;
    value += input.value;
    return *this;
}
Error::Error(const Input &input) : value(input.value) {
    append(5);
    if (input.self != &input) throw 99;
    if (fail_payload) throw 10;
}
void discard_record(bool fail_input) {
    get_accumulator() += Input(7, fail_input);
}
Accumulator *scoped_address() {
    return &(get_accumulator() += Input(7, false));
}
void throw_scoped(bool fail_input) {
    throw Error(Input(17, fail_input));
}
