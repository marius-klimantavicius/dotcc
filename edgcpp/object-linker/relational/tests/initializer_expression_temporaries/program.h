#pragma once
struct Value {
    int value;
    const Value* self;
    Value(int, bool);
    Value(const Value&) = delete;
    ~Value() noexcept;
};
struct Result {
    int value;
    const Result* self;
    Result(int);
    Result(const Result&) = delete;
    ~Result() noexcept;
};
extern int trace;
void append(int);
Value make_value(bool);
Result combine(const Value&, bool);
Result wrapper(bool, bool);
int check_temporaries();
