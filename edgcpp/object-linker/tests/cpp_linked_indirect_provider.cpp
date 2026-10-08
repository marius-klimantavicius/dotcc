#include "cpp_linked_indirect.h"
int copies;
Value::Value(int number) noexcept : value(number) {}
Value::Value(const Value &other) noexcept : value(other.value + 1) { ++copies; }
Value Value::successor(int amount) const noexcept { return Value(value + amount); }
Value make_value(int number) noexcept { return Value(number); }
int consume_value(Value value) noexcept { value.value += 100; return value.value; }
Value bounce_value(Value value) noexcept { return value; }
Value add_value(Value value, int amount) noexcept { return value.successor(amount); }
void fill_first(Value *target, int number) noexcept { target->value = number; }
void fill_last(int number, Value *target) noexcept { target->value = number; }
const Callbacks callbacks = { make_value, consume_value, bounce_value, fill_first, fill_last };
Holder::Holder(Value source) noexcept : observed(source.value) { source.value = -1; }
