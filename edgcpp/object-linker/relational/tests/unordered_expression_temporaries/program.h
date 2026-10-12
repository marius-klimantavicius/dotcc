#pragma once
struct Value {
    int id;
    const Value* self;
    Value(int);
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
struct Receiver {
    Receiver();
    Receiver(const Receiver&) = delete;
    ~Receiver() noexcept;
    static Result combine(const Value&, const Value&);
};
extern int completed;
extern int destroyed;
extern int fail_id;
extern bool fail_call;
extern bool order_error;
extern int receiver_constructed;
extern int receiver_destroyed;
extern int result_constructed;
extern int result_destroyed;
Value make_value(int);
Receiver make_receiver();
Result evaluate();
int check_unordered_temporaries();
