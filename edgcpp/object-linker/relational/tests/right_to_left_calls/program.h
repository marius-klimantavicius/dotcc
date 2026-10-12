#pragma once
struct Input {
    int value;
    const Input* self;
    Input(int);
    Input(const Input&) = delete;
    ~Input() noexcept;
};
struct Accumulator {
    int value;
    Accumulator& operator+=(const Input&);
};
extern Accumulator accumulator;
extern int trace;
extern bool fail_input;
extern bool fail_receiver;
extern bool fail_call;
Input make_input();
Accumulator& get_accumulator();
Accumulator& perform();
int check_calls();
