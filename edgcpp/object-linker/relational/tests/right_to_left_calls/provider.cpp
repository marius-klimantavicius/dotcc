#include "program.h"
Accumulator accumulator = {0};
int trace = 0;
bool fail_input = false;
bool fail_receiver = false;
bool fail_call = false;
void append(int event) { trace = trace * 10 + event; }
Input::Input(int input) : value(input), self(this) {
    append(1);
    if (fail_input) throw 7;
}
Input::~Input() noexcept { append(4); }
Input make_input() { return Input(7); }
Accumulator& get_accumulator() {
    append(2);
    if (fail_receiver) throw 8;
    return accumulator;
}
Accumulator& Accumulator::operator+=(const Input& input) {
    append(3);
    if (this != &accumulator || input.self != &input) throw 99;
    if (fail_call) throw 9;
    value += input.value;
    return *this;
}
Accumulator& perform() { return get_accumulator() += make_input(); }
