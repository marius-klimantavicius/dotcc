#pragma once
extern int copies;
struct Value {
    int value;
    explicit Value(int number) noexcept;
    Value(const Value &other) noexcept;
    Value successor(int amount) const noexcept;
};
static_assert(__has_trivial_destructor(Value), "This fixture isolates call ABI from cleanup");
Value make_value(int number) noexcept;
int consume_value(Value value) noexcept;
Value bounce_value(Value value) noexcept;
Value add_value(Value value, int amount) noexcept;
void fill_first(Value *target, int number) noexcept;
void fill_last(int number, Value *target) noexcept;
struct Callbacks {
    Value (*make)(int) noexcept;
    int (*consume)(Value) noexcept;
    Value (*bounce)(Value) noexcept;
    void (*first)(Value *, int) noexcept;
    void (*last)(int, Value *) noexcept;
};
extern const Callbacks callbacks;
struct Holder {
    int observed;
    explicit Holder(Value source = Value(3)) noexcept;
};
static_assert(__has_trivial_destructor(Holder), "Default adapters also have trivial cleanup");
