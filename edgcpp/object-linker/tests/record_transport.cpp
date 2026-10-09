// Distinguish source construction/ABI transport from user copy assignment.
static int assignments, explicit_copies, destroyed;
struct Value {
    int value;
    Value &operator=(const Value &other) {
        ++assignments;
        value = other.value + 100;
        return *this;
    }
};
static_assert(!__is_trivially_copyable(Value), "custom assignment is nontrivial");
static_assert(__is_trivially_constructible(Value, const Value &), "construction is trivial");
struct Nested { Value first; int tag; };
union UnionValue {
    int value;
    UnionValue &operator=(const UnionValue &other) {
        ++assignments;
        value = other.value + 10;
        return *this;
    }
};
static Value make_value(int value) { return {value}; }
static Value pass_value(Value value) { return value; }
static Value choose_value(int yes, Value first, Value second) { return yes ? first : second; }
static Nested make_nested(int value) { return {{value}, value + 1}; }
static UnionValue make_union(int value) { UnionValue result; result.value=value; return result; }
struct ExplicitCopy {
    int value;
    ExplicitCopy(int x) : value(x) {}
    ExplicitCopy(const ExplicitCopy &other) : value(other.value) { ++explicit_copies; }
};
static int consume_explicit(ExplicitCopy value) { return value.value; }
struct GuardValue {
    int value;
    ~GuardValue() { destroyed = destroyed * 10 + value; }
};
static GuardValue make_guard(int value) { return {value}; }
static int consume_guard(GuardValue value) { return value.value; }
struct PlainValue { int value; };
static PlainValue pass_plain(PlainValue value) { return value; }
struct DeletedCopy {
    int value;
    explicit DeletedCopy(int x) : value(x) {}
    DeletedCopy(const DeletedCopy &) = delete;
};
static_assert(__is_trivially_copyable(PlainValue), "ordinary record control");
static_assert(__is_trivially_copyable(DeletedCopy), "deleted constructor does not establish ABI transport permission");
static DeletedCopy make_deleted(int value) { return DeletedCopy(value); }
extern "C" int record_transport_test() {
    assignments = explicit_copies = destroyed = 0;
    PlainValue plain = pass_plain({27});
    DeletedCopy deleted = make_deleted(31);
    if (plain.value != 27 || deleted.value != 31) return 8;
    Value first = make_value(7);
    Value copied = first;
    Value transported = pass_value(first);
    Value selected = choose_value(0, first, make_value(9));
    if (first.value != 7 || copied.value != 7 || transported.value != 7 || selected.value != 9 || assignments) return 1;
    copied = first;
    if (copied.value != 107 || assignments != 1 || make_value(23).value != 23) return 2;
    Nested nested = make_nested(11);
    Nested nested_copy = nested;
    nested_copy = nested;
    if (nested.first.value != 11 || nested_copy.first.value != 111 || nested_copy.tag != 12 || assignments != 2) return 3;
    UnionValue original_union = make_union(13);
    UnionValue copied_union = original_union;
    copied_union = original_union;
    if (copied_union.value != 23 || make_union(17).value != 17 || assignments != 3) return 4;
    ExplicitCopy original(19);
    ExplicitCopy copy(original);
    if (copy.value != 19 || consume_explicit(original) != 19 || explicit_copies != 2) return 5;
    {
        GuardValue guard = make_guard(4);
        if (guard.value != 4 || destroyed != 0) return 6;
        int guard_result = consume_guard(guard);
        if (guard_result != 4 || destroyed != 4) return 6;
    }
    if (destroyed != 44 || assignments != 3 || explicit_copies != 2) return 7;
    return 0;
}
int main() { return record_transport_test(); }
