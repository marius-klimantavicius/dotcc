#include "program.h"
int completed = 0;
int destroyed = 0;
int fail_id = 0;
bool fail_call = false;
bool order_error = false;
int receiver_constructed = 0;
int receiver_destroyed = 0;
int result_constructed = 0;
int result_destroyed = 0;
Value::Value(int input) : id(input), self(this) {
    if (fail_id == id) throw 20 + id;
    completed = completed * 10 + id;
}
Value::~Value() noexcept { destroyed = destroyed * 10 + id; }
Result::Result(int input) : value(input), self(this) { ++result_constructed; }
Result::~Result() noexcept { ++result_destroyed; }
Receiver::Receiver() { ++receiver_constructed; }
Receiver::~Receiver() noexcept {
    int expected = completed == 12 ? 21 : completed == 21 ? 12 : completed;
    if (destroyed != expected) order_error = true;
    ++receiver_destroyed;
}
Value make_value(int id) { return Value(id); }
Receiver make_receiver() { return Receiver(); }
Result Receiver::combine(const Value& left, const Value& right) {
    if (left.self != &left || right.self != &right) throw 99;
    if (fail_call) throw 30;
    return Result(left.id + right.id);
}
Result evaluate() { return (make_receiver(), Receiver::combine(make_value(1), make_value(2))); }
