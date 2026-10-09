// Ordinary C++ destruction and constructor rollback; the host supplies throws.
extern "C" void cleanup_throw_point(int point);
static unsigned long long trace_value;
static void record(int digit) { trace_value = trace_value * 10 + digit; }
extern "C" void cleanup_reset() { trace_value = 0; }
extern "C" unsigned long long cleanup_trace() { return trace_value; }
struct Item {
    int digit;
    Item(int value) : digit(value) { record(digit); cleanup_throw_point(digit); }
    ~Item() noexcept(false) { record(digit); }
};
extern "C" void cleanup_locals() {
    Item first(1);
    { Item second(2); cleanup_throw_point(3); record(7); }
}
struct Base {
    Base(const Item &) { record(4); cleanup_throw_point(4); }
    ~Base() noexcept(false) { record(5); }
};
struct Derived : Base {
    Derived() : Base(Item(3)) { record(6); cleanup_throw_point(6); }
    ~Derived() noexcept(false) { record(7); }
};
extern "C" void cleanup_constructor() {
    Derived value;
    record(8);
    cleanup_throw_point(8);
}

struct PayloadBase {
    int payload;
    PayloadBase() : payload(9) { record(4); cleanup_throw_point(4); }
    ~PayloadBase() noexcept(false) { record(5); }
};
struct MemberOwner : PayloadBase {
    Item member;
    MemberOwner() : member(2) { record(6); cleanup_throw_point(6); }
    ~MemberOwner() noexcept(false) { record(7); }
};
extern "C" void cleanup_members() {
    MemberOwner value;
    record(8);
    cleanup_throw_point(8);
}

static int read_item(const Item &value) { return value.digit; }
static int return_item_digit() { return read_item(Item(3)); }
extern "C" void cleanup_temporary_result() {
    int result = return_item_digit();
    record(result);
}

// The destructor definition follows the delegating constructor. Its signature
// must be captured at its own finalized pre-lowering boundary.
struct Delegating {
    Delegating();
    Delegating(int);
    ~Delegating() noexcept(false);
};
Delegating::Delegating() : Delegating(1) { record(2); cleanup_throw_point(2); }
Delegating::Delegating(int) { record(1); cleanup_throw_point(1); }
Delegating::~Delegating() noexcept(false) { record(3); }
extern "C" void cleanup_delegating() {
    Delegating value;
    record(8);
    cleanup_throw_point(8);
}

// Both operands share one full-expression and are sequenced by commas.
// Each conditional temporary has its own flag supplement in the EH table.
static int conditional_consume(const Item &value) {
    record(8);
    cleanup_throw_point(70 + value.digit);
    return value.digit;
}
static void conditional_pair(int left, int right) {
    Item outer(1);
    (left && conditional_consume(Item(2)),
     right && conditional_consume(Item(3)),
     cleanup_throw_point(9));
    record(6);
}
extern "C" void cleanup_conditional_both() { conditional_pair(1, 1); }
extern "C" void cleanup_conditional_left() { conditional_pair(1, 0); }
extern "C" void cleanup_conditional_right() { conditional_pair(0, 1); }
extern "C" void cleanup_conditional_none() { conditional_pair(0, 0); }
static void conditional_choice(int left) {
    Item outer(1);
    (left ? conditional_consume(Item(2)) : conditional_consume(Item(3)),
     cleanup_throw_point(9));
    record(6);
}
extern "C" void cleanup_choice_left() { conditional_choice(1); }
extern "C" void cleanup_choice_right() { conditional_choice(0); }
extern "C" void cleanup_conditional_loop() {
    // The same lowered temporary/flag is reused: true, false, true. The
    // middle unwind must not destroy the already-destroyed first temporary.
    for (int index = 0; index != 3; ++index) {
        (index != 1 && conditional_consume(Item(2)),
         cleanup_throw_point(10 + index));
        record(6);
    }
}

// A nontrivial temporary already has owned storage; selecting its member must
// not create another record copy or change the existing destruction boundary.
extern "C" void cleanup_temporary_member_result() {
    int result = Item(3).digit;
    record(result);
}
struct PlaceValue { int value; int other; };
struct NestedPlaceValue { PlaceValue inner; };
union UnionPlaceValue { int value; unsigned bits; };
struct BitPlaceValue { unsigned bits : 3; };
static int place_calls;
static int place_step() { return ++place_calls; }
static PlaceValue make_place_value(int value) {
    ++place_calls;
    return {value, value + 1};
}
static NestedPlaceValue make_nested_place_value() {
    ++place_calls;
    return {{41, 42}};
}
static UnionPlaceValue make_union_place_value() {
    ++place_calls;
    UnionPlaceValue result;
    result.value = 11;
    return result;
}
static BitPlaceValue make_bit_place_value() {
    ++place_calls;
    return {5};
}
extern "C" void cleanup_sequence_places() {
    PlaceValue object{5, 7};
    place_calls = 0;
    int read = (place_step(), object).value;
    (place_step(), object).value += 3;
    int *address = &(place_step(), object).value;
    int before = (place_step(), object).value++;
    int returned = make_place_value(13).value;
    int selected = (place_step(), make_place_value(17)).other;
    int union_value = make_union_place_value().value;
    PlaceValue rows[2]{{21, 22}, {31, 32}};
    PlaceValue *next = rows;
    int chosen = (place_step(), *next++).other;
    volatile int observed = 19;
    int volatile_read = (place_step(), observed);
    (place_step(), observed) = 23;
    int nested = make_nested_place_value().inner.other;
    unsigned bits = make_bit_place_value().bits;
    if (read == 5 && before == 8 && object.value == 9 && address == &object.value &&
        returned == 13 && selected == 18 && union_value == 11 && chosen == 22 &&
        next == rows + 1 && volatile_read == 19 && observed == 23 && nested == 42 &&
        bits == 5 && place_calls == 13)
        record(4);
    else
        record(9);
}

struct alignas(16) AlignedPlaceValue { int values[4]; };
static int aligned_place_calls;
static AlignedPlaceValue make_aligned_place_value(int value) {
    ++aligned_place_calls;
    return {{value, value + 1, value + 2, value + 3}};
}
static int check_aligned_place_array(const int *values, int value) {
    return (reinterpret_cast<unsigned long long>(values) & 15) == 0 &&
           values[0] == value && values[1] == value + 1 &&
           values[2] == value + 2 && values[3] == value + 3;
}
extern "C" void cleanup_aligned_materialization() {
    aligned_place_calls = 0;
    // Array decay takes the address of a member of the returned record.
    // A per-iteration stackalloc would grow beyond the ordinary thread stack;
    // each execution materialization site needs one reusable aligned slot.
    for (int index = 0; index != 300000; ++index) {
        if (!check_aligned_place_array(make_aligned_place_value(index).values, index)) {
            record(9);
            return;
        }
    }
    int last = make_aligned_place_value(7).values[2];
    record(last == 9 && aligned_place_calls == 300001 ? 4 : 9);
}
